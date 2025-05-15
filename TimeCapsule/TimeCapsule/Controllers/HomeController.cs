using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TimeCapsule.Models;
using TimeCapsule.Models.ViewModels;
using TimeCapsule.Services;

namespace TimeCapsule.Controllers
{
    public class HomeController : TimeCapsuleBaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ContactService _contactService;

        public HomeController(ILogger<HomeController> logger, ContactService contactService)
        {
            _logger = logger;
            _contactService = contactService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Privacy()
        {
            return View();
        }

    
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SubmitMessage([FromForm] ContactMessageViewModel msg)
        {
            if (!ModelState.IsValid)
            {
                TempData["ContactError"] = "Proszê wype³niæ wszystkie wymagane pola.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _contactService.SubmitMessage(msg);

            if (result)
            {
                TempData["ContactSuccess"] = "Twoja wiadomoœæ zosta³a wys³ana. Dziêkujemy za kontakt!";    
            }
            else
            {
                TempData["ContactError"] = "Wyst¹pi³ b³¹d podczas wysy³ania wiadomoœci. Spróbuj ponownie póŸniej.";
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult FAQ()
        {
            return View();
        }
       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl ?? "~/");
        }

    }
}
