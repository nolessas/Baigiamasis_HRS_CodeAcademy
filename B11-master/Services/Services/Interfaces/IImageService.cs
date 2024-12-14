public interface IImageService
{
    Task<byte[]> GetProfileImageAsync(Guid userId);
    Task<bool> UpdateProfileImageAsync(Guid userId, IFormFile imageFile);
    Task<byte[]> ProcessProfileImageAsync(IFormFile imageFile);
} 