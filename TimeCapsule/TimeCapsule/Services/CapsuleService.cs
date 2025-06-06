﻿using Microsoft.EntityFrameworkCore;
using TimeCapsule.Models;
using TimeCapsule.Models.Dto;
using TimeCapsule.Services.Results;
using TimeCapsule.Models.DatabaseModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;

namespace TimeCapsule.Services
{
    public class CapsuleService
    {
        private readonly TimeCapsuleContext _context;
        private readonly ILogger<CapsuleService> _logger;
        private readonly IEmailSender _emailSender;

        public CapsuleService(TimeCapsuleContext context, ILogger<CapsuleService> logger, IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
        }

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

                _logger.LogInformation("Loaded {SectionCount} sections with {QuestionCount} questions for capsule type {CapsuleType}",
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

        public async Task<ServiceResult<int>> SaveCapsule(CreateCapsuleDto capsuleDto, IdentityUser user)
        {
            try
            {
                if (capsuleDto.OpeningDate == default || capsuleDto.OpeningDate == DateTime.MinValue)
                {
                    return ServiceResult<int>.Failure("Data otwarcia kapsuły jest wymagana i musi być prawidłową datą w przyszłości.");
                }

                if (capsuleDto.OpeningDate <= DateTime.UtcNow)
                {
                    return ServiceResult<int>.Failure("Data otwarcia kapsuły musi być w przyszłości.");
                }

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
                    _context.Capsules.Add(capsule);
                    await _context.SaveChangesAsync();

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

                    // Zapis zdjęć
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

                    // Zapis linków
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

                    // Zapis odbiorców dla kapsuł parnych
                    if (capsuleDto.Type == CapsuleType.DlaKogos && capsuleDto.Recipients != null &&  capsuleDto.Recipients.Any())
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
                        
                        if (capsuleDto.NotifyRecipients && _emailSender != null)
                        {
                            await SendNotificationsToRecipients(capsule, capsuleDto, user);
                        }
                    }

                    // Zatwierdzenie transakcji
                    await transaction.CommitAsync();

                    _logger.LogInformation("Kapsuła {CapsuleId} utworzona pomyślnie przez użytkownika {user.Id}", capsule.Id, user.Id);
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

        private async Task SendNotificationsToRecipients(Capsule capsule, CreateCapsuleDto capsuleDto, IdentityUser user)
        {
            foreach (var email in capsuleDto.Recipients.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                try
                {
                    string subject = $"Masz nową kapsułę czasu od {user.UserName ?? "znajomego"}!";
                    string? message = await GenerateEmailTemplateAsync(user.UserName, capsuleDto.Title, capsuleDto.OpeningDate);

                    await _emailSender.SendEmailAsync(email, subject, message);

                    var recipientInDb = await _context.CapsuleRecipients
                        .FirstOrDefaultAsync(r => r.CapsuleId == capsule.Id && r.Email == email);
                    if (recipientInDb != null)
                    {
                        recipientInDb.EmailSent = true;
                        _context.CapsuleRecipients.Update(recipientInDb);
                    }

                    _logger.LogInformation("Wysłano powiadomienie o kapsule {CapsuleId} do odbiorcy {Email}",
                        capsule.Id, email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wysyłania powiadomienia o kapsule {CapsuleId} do odbiorcy {Email}",
                        capsule.Id, email);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string?> GenerateEmailTemplateAsync(string? senderName, string capsuleTitle, DateTime openingDate)
        {
            string templateFileName = "CapsuleNotificationEmail.html";
            string templateDirectory = Path.Combine(AppContext.BaseDirectory, "Templates");
            string templatePath = Path.Combine(templateDirectory, templateFileName);

            string emailTemplateContent;
            try
            {
                emailTemplateContent = await File.ReadAllTextAsync(templatePath);
            }
            catch (DirectoryNotFoundException)
            {
                _logger.LogError("Nie znaleziono folderu szablonów email: {TemplateDirectory}", templateDirectory);
                return null;
            }
            catch (FileNotFoundException)
            {
                _logger.LogError("Nie znaleziono pliku szablonu email: {TemplatePath}", templatePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas odczytu szablonu email z pliku: {TemplatePath}", templatePath);
                return null;
            }

            emailTemplateContent = emailTemplateContent.Replace("{SenderName}", senderName ?? "Znajomy");
            emailTemplateContent = emailTemplateContent.Replace("{CapsuleTitle}", capsuleTitle);
            emailTemplateContent = emailTemplateContent.Replace("{OpeningDate}", openingDate.ToString("dd/MM/yyyy HH:mm"));

            return emailTemplateContent;
        }
    }
}


