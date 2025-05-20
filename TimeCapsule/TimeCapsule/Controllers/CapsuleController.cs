using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeCapsule.Extensions;
using TimeCapsule.Models.DatabaseModels;
using TimeCapsule.Models.Dto;
using TimeCapsule.Models.ViewModels;
using TimeCapsule.Services;

namespace TimeCapsule.Controllers
{
    [Authorize]
    [Route("TimeCapsule")]
    public class CapsuleController : TimeCapsuleBaseController
    {
        private readonly CapsuleService _capsuleService;
        private readonly UserManager<IdentityUser> _userManager;

        public CapsuleController(CapsuleService capsuleService, UserManager<IdentityUser> userManager)
        {
            _capsuleService = capsuleService;
            _userManager = userManager;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("Step1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Step1()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (capsule == null)
            {
                var newCapsule = new CreateCapsuleDto();

                HttpContext.Session.SetObject("CurrentCapsule", newCapsule);
                return View("~/Views/Capsule/CreateStep1.cshtml", newCapsule);
            }
            return View("~/Views/Capsule/CreateStep1.cshtml", capsule);
        }

        [HttpPost]
        [Route("SaveStep1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SaveStep1([FromForm] CreateCapsuleDto capsule)
        {
            if (capsule.Type == null || !Enum.IsDefined(typeof(CapsuleType), capsule.Type))
            {
                TempData["ErrorMessage"] = "Wybór typu kapsuły jest niezbędny do kontynuowania procesu.";
                return View("~/Views/Capsule/CreateStep1.cshtml", capsule);
            }

            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule != null)
            {
                fullCapsule.Type = capsule.Type;
                fullCapsule.NotifyRecipients = capsule.NotifyRecipients;

                if (capsule.Type == CapsuleType.DlaKogos)
                {
                    var validEmails = new List<string>();
                    var invalidEmails = new List<string>();

                    if (capsule.Recipients != null)
                    {
                        foreach (var recipient in capsule.Recipients)
                        {
                            if (string.IsNullOrWhiteSpace(recipient))
                                continue;

                            if (IsValidEmail(recipient))
                                validEmails.Add(recipient);
                            else
                                invalidEmails.Add(recipient);
                        }
                    }

                    if (!validEmails.Any())
                    {
                        ModelState.AddModelError("Recipients", "Należy podać co najmniej jeden poprawny adres email");
                        TempData["ErrorMessage"] = "Dla kapsuły typu 'Dla kogoś' należy podać co najmniej jeden poprawny adres email.";
                        return View("~/Views/Capsule/CreateStep1.cshtml", capsule);
                    }
                    if (invalidEmails.Any())
                    {
                        TempData["WarningMessage"] = $"Pominięto niepoprawne adresy email: {string.Join(", ", invalidEmails)}";
                    }

                    fullCapsule.Recipients = validEmails;
                }

                HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);
            }
            else
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            return RedirectToAction("Step2");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


        [HttpGet]
        [Route("Step2")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Step2()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (capsule != null)
            {
                return View("~/Views/Capsule/CreateStep2.cshtml", capsule);
            }

            TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
            return RedirectToAction("Step1");
        }

        [HttpPost]
        [Route("SaveStep2")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SaveStep2([FromForm] CreateCapsuleDto capsule)
        {
            if (string.IsNullOrWhiteSpace(capsule.Title))
            {
                ModelState.AddModelError("Title", "Tytuł jest wymagany");
                return View("~/Views/Capsule/CreateStep2.cshtml", capsule);
            }

            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule != null)
            {
                fullCapsule.Title = capsule.Title;
                fullCapsule.Color = capsule.Color;
                fullCapsule.Icon = capsule.Icon;
                HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);
            }
            else
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            return RedirectToAction("Step3");
        }

        [HttpGet]
        [Route("Step3")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Step3()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (capsule != null)
            {
                return View("~/Views/Capsule/CreateStep3.cshtml", capsule);
            }

            TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
            return RedirectToAction("Step1");
        }

        [HttpPost]
        [Route("SaveStep3")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SaveStep3([FromForm] CreateCapsuleDto capsule)
        {
            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule != null)
            {
                fullCapsule.Introduction = capsule.Introduction;
                HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);
            }
            else
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            return RedirectToAction("Step4");
        }

        [HttpGet]
        [Route("Step4")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Step4()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (capsule == null)
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            if (capsule.Type == null || !Enum.IsDefined(typeof(CapsuleType), capsule.Type))
            {
                TempData["ErrorMessage"] = "Wybór typu kapsuły jest niezbędny do kontynuowania procesu. Prosimy wybrać typ kapsuły.";
                return RedirectToAction("Step1");
            }

            var capsuleWithSections = await _capsuleService.GetSectionsWithQuestions(capsule);

            HttpContext.Session.SetObject("CurrentCapsule", capsuleWithSections);

            return View("~/Views/Capsule/CreateStep4.cshtml", capsuleWithSections);
        }

        [HttpPost]
        [Route("SaveStep4")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SaveStep4([FromForm] CreateCapsuleDto capsule)
        {
            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule != null)
            {
                fullCapsule.Answers = capsule.Answers ?? new List<CapsuleAnswerDto>();

                HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);
            }
            else
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            return RedirectToAction("Step5");
        }


        [HttpGet]
        [Route("Step5")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Step5()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");
            if (capsule != null)
            {
                return View("~/Views/Capsule/CreateStep5.cshtml", capsule);
            }
            TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";

            return RedirectToAction("Step1");
        }

        [HttpPost]
        [Route("SaveStep5")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SaveStep5([FromForm] CreateCapsuleDto capsule)
        {
            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule != null)
            {
                fullCapsule.MessageContent = capsule.MessageContent;
                HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);
            }
            else
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            return RedirectToAction("Step6");
        }

        [HttpGet]
        [Route("Step6")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Step6()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");
            if (capsule != null)
            {
                return View("~/Views/Capsule/CreateStep6.cshtml", capsule);
            }
            TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";

            return RedirectToAction("Step1");
        }

        [HttpGet]
        [Route("DeleteImage")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult DeleteImage(int imageIndex, string returnStep = "Step6")
        {
            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule != null && fullCapsule.UploadedImages != null &&
                imageIndex >= 0 && imageIndex < fullCapsule.UploadedImages.Count)
            {
                fullCapsule.UploadedImages.RemoveAt(imageIndex);
                HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);
            }

            return RedirectToAction(returnStep);
        }

        [HttpPost]
        [Route("SaveStep6")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SaveStep6([FromForm] CreateCapsuleDto capsule, List<IFormFile> uploadedFiles)
        {
            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule != null)
            {
                fullCapsule.Links = capsule.Links ?? new List<string>();

                if (uploadedFiles != null && uploadedFiles.Any())
                {
                    if (fullCapsule.UploadedImages == null)
                        fullCapsule.UploadedImages = new List<UploadedImageDto>();

                    foreach (var file in uploadedFiles)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            var base64 = Convert.ToBase64String(ms.ToArray());

                            fullCapsule.UploadedImages.Add(new UploadedImageDto
                            {
                                FileName = file.FileName,
                                Base64Content = base64,
                                ContentType = file.ContentType
                            });
                        }
                    }
                }
                HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);
            }
            else
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            return RedirectToAction("Step7");
        }

        [HttpGet]
        [Route("Step7")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Step7()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");
            if (capsule != null)
            {
                return View("~/Views/Capsule/CreateStep7.cshtml", capsule);
            }
            TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";

            return RedirectToAction("Step1");
        }

        [HttpPost]
        [Route("SaveStep7")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SaveStep7([FromForm] CreateCapsuleDto capsule, string OpenDate, string OpenTime, string PredefinedPeriod)
        {
            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule == null)
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            DateTime openingDateTime = DateTime.Now;

            if (!string.IsNullOrEmpty(PredefinedPeriod))
            {
                if (PredefinedPeriod.EndsWith("m"))
                {
                    int months = int.Parse(PredefinedPeriod.TrimEnd('m'));
                    openingDateTime = openingDateTime.AddMonths(months);
                }
                else if (PredefinedPeriod.EndsWith("y"))
                {
                    int years = int.Parse(PredefinedPeriod.TrimEnd('y'));
                    openingDateTime = openingDateTime.AddYears(years);
                }                
            }
            else if (!string.IsNullOrEmpty(OpenDate) && !string.IsNullOrEmpty(OpenTime))
            {
                if (DateTime.TryParse($"{OpenDate} {OpenTime}", out DateTime parsedDateTime))
                {
                    if (parsedDateTime <= DateTime.Now)
                    {
                        TempData["ErrorMessage"] = "Data otwarcia kapsuły musi być w przyszłości.";
                        return RedirectToAction("Step7");
                    }

                    openingDateTime = parsedDateTime;
                }
                else
                {
                    TempData["ErrorMessage"] = "Nieprawidłowy format daty lub czasu.";
                    return RedirectToAction("Step7");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Wybierz datę i godzinę otwarcia kapsuły.";
                return RedirectToAction("Step7");
            }

            fullCapsule.OpeningDate = openingDateTime;
            HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);

            return RedirectToAction("Step8");
        }

        [HttpGet]
        [Route("Step8")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Step8()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");
            if (capsule != null)
            {
                return View("~/Views/Capsule/CreateStep8.cshtml", capsule);
            }
            TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";

            return RedirectToAction("Step1");
        }

        [HttpPost]
        [Route("SaveStep8")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SaveStep8([FromForm] CreateCapsuleDto capsule)
        {
            var fullCapsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (fullCapsule == null)
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            if (fullCapsule.Type == null || string.IsNullOrWhiteSpace(fullCapsule.Title) || fullCapsule.OpeningDate == default)
            {
                TempData["ErrorMessage"] = "Brakuje niektórych wymaganych danych. Prosimy uzupełnić wszystkie wymagane pola.";
                return RedirectToAction("Step8");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Nie można zidentyfikować użytkownika. Prosimy zalogować się ponownie.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono konta użytkownika. Prosimy zalogować się ponownie.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var result = await _capsuleService.SaveCapsule(fullCapsule, user);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Error?.Description ?? "Wystąpił błąd podczas zapisywania kapsuły.";
                return RedirectToAction("Step8");
            }

            HttpContext.Session.SetObject("CurrentCapsule", fullCapsule);

            return RedirectToAction("Step9");
        }

        [HttpGet]
        [Route("Step9")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Step9()
        {
            var capsule = HttpContext.Session.GetObject<CreateCapsuleDto>("CurrentCapsule");

            if (capsule == null)
            {
                TempData["ErrorMessage"] = "Twoja sesja wygasła lub dane zostały utracone. Prosimy rozpocząć proces tworzenia kapsuły od początku.";
                return RedirectToAction("Step1");
            }

            var timeRemaining = capsule.OpeningDate - DateTime.UtcNow;
            var timeRemainingViewModel = new TimeRemainingViewModel
            {
                Years = timeRemaining.Days / 365,
                Days = timeRemaining.Days % 365,
                Hours = timeRemaining.Hours,
                Minutes = timeRemaining.Minutes
            };

            HttpContext.Session.Remove("CurrentCapsule");

            return View("~/Views/Capsule/CreateStep9.cshtml", timeRemainingViewModel);
        }
    }
}




