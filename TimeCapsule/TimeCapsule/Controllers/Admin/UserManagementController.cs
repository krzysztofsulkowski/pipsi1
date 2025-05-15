using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeCapsule.Models;
using TimeCapsule.Models.Dto;
using TimeCapsule.Services.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TimeCapsule.Interfaces;

namespace TimeCapsule.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("AdminPanel/Users")]
    public class UserManagementController : TimeCapsuleBaseController
    {
        private readonly IUserManagementService _userManagementService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(IUserManagementService userManagementService, RoleManager<IdentityRole> roleManager)
        {
            _userManagementService = userManagementService;
            _roleManager = roleManager;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UserManagementPanel()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var viewModel = new UserDto
            {
                AvailableRoles = roles
            };
            return View("~/Views/AdminPanel/Users/UsersManagementView.cshtml", viewModel);
        }

        [HttpPost("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromForm] DataTableRequest request)
        {
            var serviceResponse = await _userManagementService.GetAllUsers(request);
            return HandleStatusCodeServiceResult(serviceResponse);
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromForm] UserDto user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                       .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                       .ToList();
                return BadRequest(ServiceResult.Failure("Invalid data:\n" + string.Join("\n", errors)));
            }
            var result = await _userManagementService.CreateUser(user);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"User {user.UserName} created successfully";
                TempData["SuccessMessageId"] = $"user_create_{user.UserId}_{DateTime.UtcNow.Ticks}";
                return HandleStatusCodeServiceResult(result);
            }
            else
            {
                return BadRequest(ServiceResult.Failure(result.Error.Description));
            }
        }

        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromForm] UserDto user)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                       .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                       .ToList();
                return BadRequest(ServiceResult.Failure("Invalid data", string.Join("\n", errors)));
            }
            var result = await _userManagementService.UpdateUser(user);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"User {user.UserName} updated successfully";
                TempData["SuccessMessageId"] = $"user_update_{user.UserId}_{DateTime.UtcNow.Ticks}";
                return HandleServiceResult(result);
            }
            else
            {
                return BadRequest(ServiceResult.Failure(result.Error.Description));
            }
        }

        [HttpPost("LockUser/{userId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> LockUser([FromRoute] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Nieprawidłowy identyfikator użytkownika";
                return RedirectToAction("UserManagementPanel");
            }

            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null && userId == currentUserId)
            {
                TempData["ErrorMessage"] = "Nie można zablokować własnego konta";
                TempData["SuccessMessageId"] = $"user_lock_{userId}_{DateTime.UtcNow.Ticks}";
                return RedirectToAction("UserManagementPanel");
            }

            var result = await _userManagementService.LockUser(userId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Konto użytkownika zostało zablokowane";
                TempData["SuccessMessageId"] = $"user_lock_{userId}_{DateTime.UtcNow.Ticks}";
            }
            else
            {
                TempData["ErrorMessage"] = $"Wystąpił błąd podczas blokowania konta: {result.Error?.Description}";
            }

            return RedirectToAction("UserManagementPanel");
        }

        [HttpPost("UnlockUser/{userId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UnlockUser([FromRoute] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Nieprawidłowy identyfikator użytkownika";
                return RedirectToAction("UserManagementPanel");
            }

            var result = await _userManagementService.UnlockUser(userId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Konto użytkownika zostało odblokowane";
                TempData["SuccessMessageId"] = $"user_unlock_{userId}_{DateTime.UtcNow.Ticks}";
            }
            else
            {
                TempData["ErrorMessage"] = $"Wystąpił błąd podczas odblokowywania konta: {result.Error?.Description}";
            }

            return RedirectToAction("UserManagementPanel");
        }

        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ServiceResult.Failure("Nieprawidłowy parametr: ID użytkownika nie może być puste\r\n"));
            }

            var result = await _userManagementService.GetUserById(userId);
            return HandleStatusCodeServiceResult(result);
        }
    }
}


