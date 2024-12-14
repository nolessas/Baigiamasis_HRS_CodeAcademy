using Baigiamasis.Services.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Baigiamasis.Services.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageRepository> _logger;

        public ImageRepository(ApplicationDbContext context, ILogger<ImageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<byte[]> GetProfileImageAsync(Guid userId)
        {
            var humanInfo = await _context.HumanInformation
                .AsNoTracking()
                .Where(h => h.UserId == userId)
                .Select(h => h.ProfilePicture)
                .FirstOrDefaultAsync();

            return humanInfo;
        }

        public async Task<bool> UpdateProfileImageAsync(Guid userId, byte[] imageData)
        {
            var humanInfo = await _context.HumanInformation
                .Where(h => h.UserId == userId)
                .FirstOrDefaultAsync();

            if (humanInfo == null)
                return false;

            humanInfo.ProfilePicture = imageData;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await _context.HumanInformation
                .AsNoTracking()
                .AnyAsync(h => h.UserId == userId);
        }
    }
} 