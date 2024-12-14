using Baigiamasis.Models;
using AutoMapper;
using Baigiamasis.DTOs.HumanInformation;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Services.Repositories.Interfaces;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Baigiamasis.Services.Services
{
    public class HumanInformationService : IHumanInformationService
    {
        private readonly IHumanInformationRepository _humanInfoRepository;
        private readonly IImageService _imageService;
        private readonly ILogger<HumanInformationService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public HumanInformationService(
            IHumanInformationRepository humanInfoRepository,
            IImageService imageService,
            ILogger<HumanInformationService> logger,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _humanInfoRepository = humanInfoRepository;
            _imageService = imageService;
            _logger = logger;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<HumanInformationDto>> GetByIdAsync(Guid id)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);

            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var response = _mapper.Map<HumanInformationDto>(info);

            if (info.ProfilePicture != null)
            {
                response.ProfilePictureBase64 = Convert.ToBase64String(info.ProfilePicture);
            }

            return ApiResponse<HumanInformationDto>.Success(response);
        }

        public async Task<ApiResponse<HumanInformation>> CreateAsync(CreateHumanInformationDto dto, IFormFile profilePicture, Guid userId)
        {
            try
            {
                // Validate using the attribute
                var pictureDto = new ProfilePictureUpdateDto { ProfilePicture = profilePicture };
                var validationContext = new ValidationContext(pictureDto);
                var validationResults = new List<ValidationResult>();
                
                if (!Validator.TryValidateObject(pictureDto, validationContext, validationResults, true))
                {
                    return ApiResponse<HumanInformation>.BadRequest(validationResults.First().ErrorMessage);
                }

                var exists = await _humanInfoRepository.ExistsForUserAsync(userId);
                if (exists)
                    return ApiResponse<HumanInformation>.BadRequest("User already has associated human information");

                var humanInfo = _mapper.Map<HumanInformation>(dto);
                humanInfo.UserId = userId;
                humanInfo.ProfilePicture = await _imageService.ProcessProfileImageAsync(profilePicture);

                var created = await _humanInfoRepository.CreateAsync(humanInfo);

                // Return a simplified response without the profile picture
                var response = new HumanInformation
                {
                    Id = created.Id,
                    FirstName = created.FirstName,
                    LastName = created.LastName,
                    PersonalCode = created.PersonalCode,
                    PhoneNumber = created.PhoneNumber,
                    Email = created.Email,
                    UserId = created.UserId,
                    Address = created.Address
                };

                return ApiResponse<HumanInformation>.Created(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating human information for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ApiResponse<bool>> UpdateNameAsync(Guid id, string name)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);
            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var nameParts = name.Trim().Split(' ', 2);
            if (nameParts.Length != 2)
                throw new ArgumentException("Name must include both first and last name");

            var success = await _humanInfoRepository.UpdateNameAsync(id, nameParts[0], nameParts[1]);
            return ApiResponse<bool>.Success(success, "Name updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateEmailAsync(Guid id, string email)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);
            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var success = await _humanInfoRepository.UpdateEmailAsync(id, email);
            return ApiResponse<bool>.Success(success, "Email updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdatePersonalCodeAsync(Guid id, string personalCode)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);
            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var success = await _humanInfoRepository.UpdatePersonalCodeAsync(id, personalCode);
            return ApiResponse<bool>.Success(success, "Personal code updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdatePhoneAsync(Guid id, string phone)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);
            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var success = await _humanInfoRepository.UpdatePhoneAsync(id, phone);
            return ApiResponse<bool>.Success(success, "Phone number updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateCityAsync(Guid id, string city)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);
            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var success = await _humanInfoRepository.UpdateCityAsync(id, city);
            return ApiResponse<bool>.Success(success, "City updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateStreetAsync(Guid id, string street)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);
            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var success = await _humanInfoRepository.UpdateStreetAsync(id, street);
            return ApiResponse<bool>.Success(success, "Street updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateHouseNumberAsync(Guid id, string houseNumber)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);
            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var success = await _humanInfoRepository.UpdateHouseNumberAsync(id, houseNumber);
            return ApiResponse<bool>.Success(success, "House number updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateApartmentNumberAsync(Guid id, string apartmentNumber)
        {
            var info = await _humanInfoRepository.GetByIdAsync(id);
            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var success = await _humanInfoRepository.UpdateApartmentNumberAsync(id, apartmentNumber);
            return ApiResponse<bool>.Success(success, "Apartment number updated successfully");
        }

        public async Task<ApiResponse<HumanInformationDto>> GetByUserIdAsync(Guid userId)
        {
            var info = await _humanInfoRepository.GetByUserIdAsync(userId);

            if (info == null)
                throw new KeyNotFoundException("Human information not found");

            if (!_currentUserService.CanAccessUser(info.UserId))
                throw new UnauthorizedAccessException("Access denied");

            var response = _mapper.Map<HumanInformationDto>(info);

            if (info.ProfilePicture != null)
            {
                response.ProfilePictureBase64 = Convert.ToBase64String(info.ProfilePicture);
            }

            return ApiResponse<HumanInformationDto>.Success(response);
        }

        public async Task<ApiResponse<bool>> UpdateProfilePictureAsync(Guid id, IFormFile profilePicture)
        {
            try
            {
                // Validate using the attribute
                var pictureDto = new ProfilePictureUpdateDto { ProfilePicture = profilePicture };
                var validationContext = new ValidationContext(pictureDto);
                var validationResults = new List<ValidationResult>();
                
                if (!Validator.TryValidateObject(pictureDto, validationContext, validationResults, true))
                {
                    return ApiResponse<bool>.BadRequest(validationResults.First().ErrorMessage);
                }

                var info = await _humanInfoRepository.GetByIdAsync(id);
                if (info == null)
                    return ApiResponse<bool>.NotFound("Human information not found");

                if (!_currentUserService.CanAccessUser(info.UserId))
                    throw new UnauthorizedAccessException("Access denied");

                var processedImage = await _imageService.ProcessProfileImageAsync(profilePicture);
                var success = await _humanInfoRepository.UpdateProfilePictureAsync(id, processedImage);

                return ApiResponse<bool>.Success(success, "Profile picture updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile picture for ID {Id}", id);
                throw;
            }
        }
    }
}