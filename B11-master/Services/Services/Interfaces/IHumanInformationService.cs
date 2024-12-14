using Baigiamasis.Models;
using Baigiamasis.DTOs.Common;
using Baigiamasis.DTOs.HumanInformation;

namespace Baigiamasis.Services.Services.Interfaces
{
    public interface IHumanInformationService
    {
        Task<ApiResponse<HumanInformationDto>> GetByIdAsync(Guid id);
        Task<ApiResponse<HumanInformation>> CreateAsync(CreateHumanInformationDto dto, IFormFile profilePicture, Guid userId);
        Task<ApiResponse<bool>> UpdateNameAsync(Guid id, string name);
        Task<ApiResponse<bool>> UpdateEmailAsync(Guid id, string email);
        Task<ApiResponse<bool>> UpdatePersonalCodeAsync(Guid id, string personalCode);
        Task<ApiResponse<bool>> UpdatePhoneAsync(Guid id, string phone);
        Task<ApiResponse<bool>> UpdateCityAsync(Guid id, string city);
        Task<ApiResponse<bool>> UpdateStreetAsync(Guid id, string street);
        Task<ApiResponse<bool>> UpdateHouseNumberAsync(Guid id, string houseNumber);
        Task<ApiResponse<bool>> UpdateApartmentNumberAsync(Guid id, string apartmentNumber);
        Task<ApiResponse<HumanInformationDto>> GetByUserIdAsync(Guid userId);
        Task<ApiResponse<bool>> UpdateProfilePictureAsync(Guid id, IFormFile profilePicture);
    }
}