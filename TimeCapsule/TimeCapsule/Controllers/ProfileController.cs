using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using TimeCapsule.Services;
using TimeCapsule.Extentions;

namespace TimeCapsule.Controllers
{
    [Authorize]
    public class ProfileController : TimeCapsuleBaseController
    {
        private readonly ProfileService _profileService;
        private readonly IMemoryCache _cache;

        public ProfileController(ILogger<HomeController> logger, ProfileService profileService, IMemoryCache cache)
        {
            _profileService = profileService;
            _cache = cache;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> MyCapsules()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Nie można zidentyfikować użytkownika. Prosimy zalogować się ponownie.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var result = await _cache.GetOrSetAsync(
                cacheKey: $"mycapsules_{userId}",
                expiration: TimeSpan.FromMinutes(10),
                loadData: () => _profileService.GetMyCapsules(userId));

            return View("~/Views/Home/MyCapsules.cshtml", result);
        }
    }
}
