using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeCapsule.Models.Dto;
using TimeCapsule.Models;
using TimeCapsule.Services.Results;
using TimeCapsule.Interfaces;

namespace TimeCapsule.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly TimeCapsuleContext _context;
        private readonly ILogger<UserManagementService> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        public UserManagementService(TimeCapsuleContext context, ILogger<UserManagementService> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }
        public async Task<ServiceResult<DataTableResponse<UserDto>>> GetAllUsers(DataTableRequest request)
        {
            try
            {
                string[] columnNames = { "UserName", "Email", "Role" };
                string sortColumn = (request.OrderColumn >= 0 && request.OrderColumn < columnNames.Length) ? columnNames[request.OrderColumn] : "UserName";
                string sortDirection = request.OrderDir?.ToUpper() == "DESC" ? "DESC" : "ASC";

                var filteredUsers = await _context.Set<UserDto>()
                    .FromSqlInterpolated($"SELECT * FROM public.get_users_with_roles({request.SearchValue ?? (object)DBNull.Value}, {sortColumn}, {sortDirection})")
                    .ToListAsync();

                int totalRecords = await _context.Users.CountAsync();
                int filteredRecords = filteredUsers.Count;

                return ServiceResult<DataTableResponse<UserDto>>.Success(
                    new DataTableResponse<UserDto>
                    {
                        Draw = request.Draw,
                        RecordsTotal = totalRecords,
                        RecordsFiltered = filteredRecords,
                        Data = filteredUsers
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users with roles: {ErrorDetails}", ex.ToString());
                return ServiceResult<DataTableResponse<UserDto>>.Failure($"Error fetching data: {ex.Message}");
            }
        }


        public async Task<ServiceResult> LockUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ServiceResult.Failure("User not found");

                await _userManager.SetLockoutEnabledAsync(user, true);

                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

                _logger.LogInformation("User {UserId} locked permanently", userId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error permanently locking user {UserId}", userId);
                return ServiceResult.Failure($"Failed to lock user: {ex.Message}");
            }
        }

        public async Task<ServiceResult> UnlockUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ServiceResult.Failure("User not found");

                await _userManager.SetLockoutEndDateAsync(user, null);

                _logger.LogInformation("User {UserId} unlocked", userId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user {UserId}", userId);
                return ServiceResult.Failure($"Failed to unlock user: {ex.Message}");
            }
        }

        public async Task<ServiceResult<UserDto>> CreateUser(UserDto user)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _userManager.FindByEmailAsync(user.Email);
                if (existing != null)
                    return ServiceResult<UserDto>.Failure($"User {user.Email} already exists");

                var role = await _context.Roles.FindAsync(user.RoleId);
                if (role == null)
                    return ServiceResult<UserDto>.Failure($"Role with ID {user.RoleId} not found");

                if (string.IsNullOrEmpty(role.Name))
                    return ServiceResult<UserDto>.Failure($"Role with ID {user.RoleId} has no name specified");

                var newUser = new IdentityUser
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(newUser, "DefaultPassword123!");
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user: {Errors}", errors);
                    return ServiceResult<UserDto>.Failure($"Failed to create user: {errors}");
                }

                var roleResult = await _userManager.AddToRoleAsync(newUser, role.Name);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add user to role: {Errors}", errors);
                    return ServiceResult<UserDto>.Failure($"Failed to add user to role: {errors}");
                }

                await transaction.CommitAsync();

                user.UserId = newUser.Id;
                return ServiceResult<UserDto>.Success(user);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating user {Email}", user.Email);
                return ServiceResult<UserDto>.Failure($"Failed to save user: {ex.Message}");
            }
        }


        public async Task<ServiceResult> UpdateUser(UserDto model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                    return ServiceResult.Failure("User not found");

                var role = await _context.Roles.FindAsync(model.RoleId);
                if (role == null)
                    return ServiceResult.Failure($"Role with ID {model.RoleId} not found");

                if (string.IsNullOrEmpty(role.Name))
                    return ServiceResult.Failure($"Role with ID {model.RoleId} has no name specified");

                user.UserName = model.UserName;
                user.Email = model.Email;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to update user: {Errors}", errors);
                    return ServiceResult.Failure($"Failed to update user: {errors}");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await _userManager.AddToRoleAsync(user, role.Name);

                await transaction.CommitAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating roles for user {model.UserId}");
                return ServiceResult.Failure("Error updating user roles");
            }
        }

        public async Task<ServiceResult<UserDto>> GetUserById(string userId)
        {
            try
            {
                var userData = await _context.Users
                     .Where(u => u.Id == userId)
                     .Join(_context.UserRoles,
                           u => u.Id,
                           ur => ur.UserId,
                           (u, ur) => new { User = u, UserRole = ur })
                     .Join(_context.Roles,
                           ur => ur.UserRole.RoleId,
                           r => r.Id,
                           (ur, r) => new
                           {
                               User = ur.User,
                               Role = r
                           })
                     .FirstOrDefaultAsync();

                if (userData == null)
                    return ServiceResult<UserDto>.Failure($"User with ID {userId} not found");

                var userDto = new UserDto
                {
                    UserId = userData.User.Id,
                    UserName = userData.User.UserName ?? string.Empty,
                    Email = userData.User.Email ?? string.Empty,
                    RoleId = userData.Role.Id,
                    RoleName = userData.Role.Name ?? string.Empty
                };

                return ServiceResult<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with ID {userId}");
                return ServiceResult<UserDto>.Failure("Error retrieving user details");
            }
        }
    }
}
