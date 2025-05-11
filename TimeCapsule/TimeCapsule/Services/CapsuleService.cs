using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using TimeCapsule.Models;
using TimeCapsule.Models.Dto;
using TimeCapsule.Models.DatabaseModels;
using TimeCapsule.Services.Results;

namespace TimeCapsule.Services
{
    public class CapsuleService
    {
        private readonly TimeCapsuleContext _context;
        private readonly ILogger<CapsuleService> _logger;
        private readonly IEmailSender _emailSender;

        public CapsuleService(
            TimeCapsuleContext context,
            ILogger<CapsuleService> logger,
            IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Ładuje sekcje i pytania do formularza tworzenia/edycji.
        /// </summary>
        public async Task<CreateCapsuleDto> GetSectionsWithQuestions(CreateCapsuleDto capsule)
        {
            try
            {
                var sections = await _context.CapsuleSections
                    .Include(s => s.Questions)
                    .Where(s => s.CapsuleType == capsule.Type)
                    .OrderBy(s => s.DisplayOrder)
                    .ToListAsync();

                var sectionDtos = sections.Select(s => new CapsuleSectionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    DisplayOrder = s.DisplayOrder,
                    CapsuleType = s.CapsuleType,
                    Questions = s.Questions
                        .OrderBy(q => q.DisplayOrder)
                        .Select(q => new CapsuleQuestionDto
                        {
                            Id = q.Id,
                            QuestionText = q.QuestionText,
                            SectionId = s.Id,
                            SectionName = s.Name,
                            DisplayOrder = q.DisplayOrder
                        }).ToList()
                }).ToList();

                capsule.CapsuleSections = sectionDtos;

                _logger.LogInformation(
                    "Loaded {SectionCount} sections with {QuestionCount} questions for capsule type {CapsuleType}",
                    sectionDtos.Count,
                    sectionDtos.Sum(s => s.Questions.Count),
                    capsule.Type);

                return capsule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading questions for capsule type {CapsuleType}", capsule.Type);
                return new CreateCapsuleDto();
            }
        }

        /// <summary>
        /// Tworzy nową kapsułę lub aktualizuje istniejącą.
        /// </summary>
        public async Task<ServiceResult<int>> SaveCapsule(CreateCapsuleDto capsuleDto, IdentityUser user)
        {
            try
            {
                if (capsuleDto.OpeningDate == default || capsuleDto.OpeningDate == DateTime.MinValue)
                    return ServiceResult<int>.Failure("Data otwarcia kapsuły jest wymagana i musi być prawidłową datą w przyszłości.");

                if (capsuleDto.OpeningDate <= DateTime.UtcNow)
                    return ServiceResult<int>.Failure("Data otwarcia kapsuły musi być w przyszłości.");

                var capsule = new Capsule
                {
                    CreatedByUserId = user.Id,
                    Title = capsuleDto.Title,
                    Type = capsuleDto.Type!.Value,
                    Icon = capsuleDto.Icon,
                    Color = capsuleDto.Color,
                    Introduction = capsuleDto.Introduction,
                    MessageContent = capsuleDto.MessageContent,
                    OpeningDate = capsuleDto.OpeningDate.ToUniversalTime(),
                    Status = Status.Created
                };

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1) Główny zapis kapsuły
                    _context.Capsules.Add(capsule);
                    await _context.SaveChangesAsync();

                    // 2) Odpowiedzi
                    if (capsuleDto.Answers != null && capsuleDto.Answers.Any())
                    {
                        foreach (var answerDto in capsuleDto.Answers.Where(a => !string.IsNullOrWhiteSpace(a.AnswerText)))
                        {
                            var answer = new CapsuleAnswer
                            {
                                CapsuleId = capsule.Id,
                                QuestionId = answerDto.QuestionId,
                                AnswerText = answerDto.AnswerText
                            };
                            _context.CapsuleAnswers.Add(answer);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 3) Zdjęcia
                    if (capsuleDto.UploadedImages != null && capsuleDto.UploadedImages.Any())
                    {
                        foreach (var imageDto in capsuleDto.UploadedImages)
                        {
                            var image = new CapsuleImage
                            {
                                CapsuleId = capsule.Id,
                                FileName = imageDto.FileName,
                                Content = Convert.FromBase64String(imageDto.Base64Content)
                            };
                            _context.CapsuleImages.Add(image);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 4) Linki
                    if (capsuleDto.Links != null && capsuleDto.Links.Any())
                    {
                        foreach (var link in capsuleDto.Links.Where(l => !string.IsNullOrWhiteSpace(l)))
                        {
                            var capsuleLink = new CapsuleLink
                            {
                                CapsuleId = capsule.Id,
                                Url = link
                            };
                            _context.CapsuleLinks.Add(capsuleLink);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 5) Odbiorcy (tylko dla DlaKogos)
                    if (capsuleDto.Type == CapsuleType.DlaKogos
                        && capsuleDto.Recipients != null
                        && capsuleDto.Recipients.Any())
                    {
                        foreach (var email in capsuleDto.Recipients.Where(e => !string.IsNullOrWhiteSpace(e)))
                        {
                            var recipient = new CapsuleRecipient
                            {
                                CapsuleId = capsule.Id,
                                Email = email,
                                EmailSent = false
                            };
                            _context.CapsuleRecipients.Add(recipient);
                        }
                        await _context.SaveChangesAsync();

                        if (capsuleDto.NotifyRecipients)
                            await SendNotificationsToRecipients(capsule, capsuleDto, user);
                    }

                    // 6) Commit
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Kapsuła {CapsuleId} utworzona pomyślnie przez użytkownika {UserId}",
                        capsule.Id, user.Id);

                    return ServiceResult<int>.Success(capsule.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Błąd podczas zapisywania kapsuły dla użytkownika {UserId}", user.Id);
                    return ServiceResult<int>.Failure("Wystąpił błąd podczas zapisywania kapsuły. Spróbuj ponownie później.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nieoczekiwany błąd podczas zapisywania kapsuły dla użytkownika {UserId}", user.Id);
                return ServiceResult<int>.Failure("Wystąpił nieoczekiwany błąd. Spróbuj ponownie później.");
            }
        }

        /// <summary>
        /// Wysyła powiadomienia mailowe do odbiorców.
        /// </summary>
        private async Task SendNotificationsToRecipients(Capsule capsule, CreateCapsuleDto dto, IdentityUser user)
        {
            foreach (var email in dto.Recipients.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                try
                {
                    var subject = $"Masz nową kapsułę czasu od {user.UserName ?? "znajomego"}!";
                    var message = GenerateEmailTemplate(user.UserName ?? "znajomego", dto.Title, dto.OpeningDate);

                    await _emailSender.SendEmailAsync(email, subject, message);

                    var r = await _context.CapsuleRecipients
                        .FirstOrDefaultAsync(x => x.CapsuleId == capsule.Id && x.Email == email);
                    if (r != null)
                    {
                        r.EmailSent = true;
                        _context.CapsuleRecipients.Update(r);
                    }

                    _logger.LogInformation(
                        "Wysłano powiadomienie o kapsule {CapsuleId} do {Email}",
                        capsule.Id, email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Błąd podczas wysyłania maila o kapsule {CapsuleId} do {Email}",
                        capsule.Id, email);
                }
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Generuje HTML szablonu maila.
        /// </summary>
        private string GenerateEmailTemplate(string senderName, string capsuleTitle, DateTime openingDate)
        {
            return $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        h1 {{ color: #3057B5; }}
        .date {{ font-weight: bold; color: #3057B5; }}
        .footer {{ margin-top: 30px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Masz nową kapsułę czasu!</h1>
        <p>Witaj!</p>
        <p>{senderName} utworzył(a) dla Ciebie kapsułę czasu o nazwie <strong>{capsuleTitle}</strong>.</p>
        <p>Kapsuła będzie dostępna do otwarcia w dniu: <span class='date'>{openingDate:dd/MM/yyyy HH:mm}</span></p>
        <p>Pozdrawiamy,<br />Zespół TimeCapsule</p>
        <div class='footer'>
            <p>To jest automatyczna wiadomość, prosimy na nią nie odpowiadać.</p>
        </div>
    </div>
</body>
</html>";
        }

        // =====================
        //  NOWE METODY CRUD
        // =====================

        /// <summary>
        /// Pobiera wszystkie kapsuły do panelu admina.
        /// </summary>
        public async Task<ServiceResult<List<CapsuleAdminDto>>> GetAllCapsules()
        {
            try
            {
                var capsules = await _context.Capsules
                    .Include(c => c.CreatedByUser)
                    .OrderByDescending(c => c.OpeningDate)
                    .ToListAsync();

                var dtos = capsules.Select(c => new CapsuleAdminDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Type = ((CapsuleType)c.Type).ToString(),
                    CreatedBy = c.CreatedByUser?.UserName ?? "–",
                    OpeningDate = c.OpeningDate.ToLocalTime(),
                    Status = c.Status.ToString()
                }).ToList();

                return ServiceResult<List<CapsuleAdminDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy pobieraniu listy kapsuł");
                return ServiceResult<List<CapsuleAdminDto>>.Failure("Nie można wczytać kapsuł");
            }
        }

        /// <summary>
        /// Pobiera jedną kapsułę wraz z powiązanymi danymi.
        /// </summary>
        public async Task<ServiceResult<CreateCapsuleDto>> GetCapsuleById(int id)
        {
            try
            {
                var cap = await _context.Capsules.FindAsync(id);
                if (cap == null)
                    return ServiceResult<CreateCapsuleDto>
                        .Failure("Kapsuła nie znaleziona", "404");

                var answers = await _context.CapsuleAnswers.Where(a => a.CapsuleId == id).ToListAsync();
                var links = await _context.CapsuleLinks.Where(l => l.CapsuleId == id).ToListAsync();
                var images = await _context.CapsuleImages.Where(i => i.CapsuleId == id).ToListAsync();
                var recipients = await _context.CapsuleRecipients.Where(r => r.CapsuleId == id).ToListAsync();

                var dto = new CreateCapsuleDto
                {
                    Id = cap.Id,
                    Type = cap.Type,
                    Title = cap.Title,
                    Introduction = cap.Introduction,
                    MessageContent = cap.MessageContent,
                    OpeningDate = cap.OpeningDate.ToLocalTime(),
                    Color = cap.Color,
                    Icon = cap.Icon,
                    NotifyRecipients = false,

                    Answers = answers.Select(a => new CapsuleAnswerDto
                    {
                        QuestionId = a.QuestionId,
                        AnswerText = a.AnswerText
                    }).ToList(),

                    Links = links.Select(l => l.Url).ToList(),

                    UploadedImages = images.Select(i => new UploadedImageDto
                    {
                        FileName = i.FileName,
                        Base64Content = Convert.ToBase64String(i.Content)
                    }).ToList(),

                    Recipients = recipients.Select(r => r.Email).ToList(),
                    Status = cap.Status
                };

                return ServiceResult<CreateCapsuleDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy pobieraniu kapsuły o id {Id}", id);
                return ServiceResult<CreateCapsuleDto>.Failure("Nie można wczytać kapsuły");
            }
        }

        /// <summary>
        /// Usuwa kapsułę o wskazanym Id.
        /// </summary>
        public async Task<ServiceResult> DeleteCapsule(int capsuleId)
        {
            try
            {
                var cap = await _context.Capsules.FindAsync(capsuleId);
                if (cap == null)
                    return ServiceResult.Failure("Kapsuła nie istnieje.");

                _context.Capsules.Remove(cap);
                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd przy usuwaniu kapsuły {Id}", capsuleId);
                return ServiceResult.Failure("Nie można usunąć kapsuły.");
            }
        }
    }
}
