using TimeCapsule.Models.Dto;
using TimeCapsule.Models;
using TimeCapsule.Services.Results;

namespace TimeCapsule.Interfaces
{
    public interface IUserManagementService
    {
        Task<ServiceResult<DataTableResponse<UserDto>>> GetAllUsers(DataTableRequest request);
        Task<ServiceResult> LockUser(string userId);
        Task<ServiceResult> UnlockUser(string userId);
        Task<ServiceResult<UserDto>> CreateUser(UserDto user);
        Task<ServiceResult> UpdateUser(UserDto model);
        Task<ServiceResult<UserDto>> GetUserById(string userId);
    }
}
