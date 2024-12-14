using Baigiamasis.Models;

namespace Baigiamasis.Services.Repositories.Interfaces
{
    public interface IHumanInformationRepository
    {
        Task<HumanInformation> GetByIdAsync(Guid id);
        Task<HumanInformation> GetByUserIdAsync(Guid userId);
        Task<HumanInformation> CreateAsync(HumanInformation humanInfo);
        Task<bool> UpdateAsync(HumanInformation humanInfo);
        Task<bool> ExistsForUserAsync(Guid userId);
        Task<bool> UpdateNameAsync(Guid id, string firstName, string lastName);
        Task<bool> UpdateEmailAsync(Guid id, string email);
        Task<bool> UpdatePersonalCodeAsync(Guid id, string personalCode);
        Task<bool> UpdatePhoneAsync(Guid id, string phone);
        Task<bool> UpdateCityAsync(Guid id, string city);
        Task<bool> UpdateStreetAsync(Guid id, string street);
        Task<bool> UpdateHouseNumberAsync(Guid id, string houseNumber);
        Task<bool> UpdateApartmentNumberAsync(Guid id, string apartmentNumber);
        Task<bool> UpdateProfilePictureAsync(Guid id, byte[] profilePicture);
    }
}