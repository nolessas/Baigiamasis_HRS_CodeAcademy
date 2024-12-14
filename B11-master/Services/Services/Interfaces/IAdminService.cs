using Baigiamasis.DTOs.User;
using Baigiamasis.DTOs.Common;

namespace Baigiamasis.Services.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
        Task<UserDto> GetUserByIdAsync(Guid id);
    }
}