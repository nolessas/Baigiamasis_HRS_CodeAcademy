using Microsoft.EntityFrameworkCore;
using Baigiamasis.Models;
using Baigiamasis.Services.Repositories.Interfaces;

namespace Baigiamasis.Services.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminRepository> _logger;

        public AdminRepository(ApplicationDbContext context, ILogger<AdminRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.HumanInformation)
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.HumanInformation)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .Include(u => u.HumanInformation)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return false;

                if (user.Roles.Contains("Admin"))
                {
                    var adminCount = await _context.Users
                        .CountAsync(u => u.Roles.Contains("Admin"));
                        
                    if (adminCount <= 1)
                        throw new InvalidOperationException("Cannot delete the last admin user");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
} 