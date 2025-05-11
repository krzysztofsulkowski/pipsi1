using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeCapsule.Models.Dto;
using TimeCapsule.Services;
using TimeCapsule.Services.Results;

namespace TimeCapsule.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("AdminPanel/Capsules")]
    public class CapsuleManagementController : Controller
    {
        private readonly CapsuleService _capsuleService;
        private readonly UserManager<IdentityUser> _userManager;

        public CapsuleManagementController(
            CapsuleService capsuleService,
            UserManager<IdentityUser> userManager)
        {
            _capsuleService = capsuleService;
            _userManager = userManager;
        }

        // GET: /AdminPanel/Capsules
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _capsuleService.GetAllCapsules();
            var list = result.IsSuccess
                ? result.Data
                : new List<CapsuleAdminDto>();
            return View("~/Views/AdminPanel/Capsules/CapsulesManagementView.cshtml", list);
        }

        // AJAX: pobierz szczegóły do edycji
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _capsuleService.GetCapsuleById(id);
            if (!result.IsSuccess) return NotFound(result.Error.Description);
            return Json(result.Data);
        }

        // POST: zapisz (create i update)
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromForm] CreateCapsuleDto dto)
        {
            var user = await _userManager.GetUserAsync(User)!;
            var result = await _capsuleService.SaveCapsule(dto, user);
            if (!result.IsSuccess)
                TempData["Error"] = result.Error.Description;
            else
                TempData["Success"] = "Kapsuła zapisana pomyślnie.";
            return RedirectToAction(nameof(Index));
        }

        // POST: usuń
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _capsuleService.DeleteCapsule(id);
            if (!result.IsSuccess)
                TempData["Error"] = result.Error.Description;
            else
                TempData["Success"] = "Kapsuła usunięta.";
            return RedirectToAction(nameof(Index));
        }
    }
}
