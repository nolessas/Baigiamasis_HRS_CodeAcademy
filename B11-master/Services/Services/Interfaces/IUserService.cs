using Baigiamasis.DTOs;
using Baigiamasis.Models;
using Baigiamasis.DTOs.Common;

namespace Baigiamasis.Services.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> ValidateUserAsync(string username, string password);
        Task<ApiResponse<object>> Signup(string username, string password);
    }
}
