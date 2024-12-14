using Baigiamasis.Models;

namespace Baigiamasis.Services.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(Guid id);
        Task<bool> DeleteUserAsync(Guid id);
    }
}