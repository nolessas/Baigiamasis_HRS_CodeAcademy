using Baigiamasis.Models;

namespace Baigiamasis.Services.Repositories.Interfaces
{
    public interface IImageRepository
    {
        Task<byte[]> GetProfileImageAsync(Guid userId);
        Task<bool> UpdateProfileImageAsync(Guid userId, byte[] imageData);
        Task<bool> UserExistsAsync(Guid userId);
    }
}
