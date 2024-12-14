using Baigiamasis.Services.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Baigiamasis.Services.Services
{
    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly IImageRepository _imageRepository;
        private const int TargetSize = 200;

        public ImageService(
            ILogger<ImageService> logger,
            IImageRepository imageRepository)
        {
            _logger = logger;
            _imageRepository = imageRepository;
        }

        public async Task<byte[]> GetProfileImageAsync(Guid userId)
        {
            try
            {
                var exists = await _imageRepository.UserExistsAsync(userId);
                if (!exists)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return null;
                }

                var image = await _imageRepository.GetProfileImageAsync(userId);
                if (image == null)
                {
                    _logger.LogWarning("Profile image not found for user {UserId}", userId);
                    return null;
                }
                return image;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile image for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateProfileImageAsync(Guid userId, IFormFile imageFile)
        {
            try
            {
                var processedImage = await ProcessProfileImageAsync(imageFile);
                return await _imageRepository.UpdateProfileImageAsync(userId, processedImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for user {UserId}", userId);
                throw;
            }
        }

        public async Task<byte[]> ProcessProfileImageAsync(IFormFile imageFile)
        {
            using var image = await Image.LoadAsync(imageFile.OpenReadStream());

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(TargetSize, TargetSize),
                Mode = ResizeMode.Stretch
            }));

            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            return ms.ToArray();
        }
    }
}