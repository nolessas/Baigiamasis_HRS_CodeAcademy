using Microsoft.EntityFrameworkCore;
using Baigiamasis.Models;
using Baigiamasis.Services.Repositories.Interfaces;

namespace Baigiamasis.Services.Repositories
{
    public class HumanInformationRepository : IHumanInformationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HumanInformationRepository> _logger;

        public HumanInformationRepository(ApplicationDbContext context, ILogger<HumanInformationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HumanInformation> GetByIdAsync(Guid id)
        {
            return await _context.HumanInformation
                .Include(h => h.Address)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<HumanInformation> GetByUserIdAsync(Guid userId)
        {
            return await _context.HumanInformation
                .Include(h => h.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.UserId == userId);
        }

        public async Task<HumanInformation> CreateAsync(HumanInformation humanInfo)
        {
            _context.HumanInformation.Add(humanInfo);
            await _context.SaveChangesAsync();
            return humanInfo;
        }

        public async Task<bool> ExistsForUserAsync(Guid userId)
        {
            return await _context.HumanInformation
                .AnyAsync(h => h.UserId == userId);
        }

        public async Task<bool> UpdateAsync(HumanInformation humanInfo)
        {
            _context.Entry(humanInfo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateNameAsync(Guid id, string firstName, string lastName)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.FirstName = firstName;
            info.LastName = lastName;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEmailAsync(Guid id, string email)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.Email = email;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePersonalCodeAsync(Guid id, string personalCode)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.PersonalCode = personalCode;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePhoneAsync(Guid id, string phone)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.PhoneNumber = phone;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCityAsync(Guid id, string city)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.Address.City = city;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStreetAsync(Guid id, string street)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.Address.Street = street;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateHouseNumberAsync(Guid id, string houseNumber)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.Address.HouseNumber = houseNumber;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateApartmentNumberAsync(Guid id, string apartmentNumber)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.Address.ApartmentNumber = apartmentNumber;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProfilePictureAsync(Guid id, byte[] profilePicture)
        {
            var info = await GetByIdAsync(id);
            if (info == null) return false;

            info.ProfilePicture = profilePicture;
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 