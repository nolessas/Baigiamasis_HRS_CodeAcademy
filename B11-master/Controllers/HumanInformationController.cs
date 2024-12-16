using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Baigiamasis.Models;
using System.Security.Claims;
using Baigiamasis.DTOs.HumanInformation;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Baigiamasis.Controllers
{
    /// <summary>
    /// Controller responsible for managing human information and profile data
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class HumanInformationController : ControllerBase
    {
        private readonly IHumanInformationService _humanInfoService;
        private readonly ILogger<HumanInformationController> _logger;
        private readonly IImageService _imageService;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Initializes a new instance of the HumanInformationController
        /// </summary>
        /// <param name="humanInfoService">Service for human information operations</param>
        /// <param name="logger">Logger for human information events</param>
        /// <param name="imageService">Service for image processing</param>
        /// <param name="currentUserService">Service for current user context</param>
        public HumanInformationController(
            IHumanInformationService humanInfoService,
            ILogger<HumanInformationController> logger,
            IImageService imageService,
            ICurrentUserService currentUserService)
        {
            _humanInfoService = humanInfoService;
            _logger = logger;
            _imageService = imageService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Retrieves human information by ID
        /// </summary>
        /// <param name="id">The ID of the human information to retrieve</param>
        /// <returns>Human information including profile picture</returns>
        /// <response code="200">Returns the human information</response>
        /// <response code="404">If the human information is not found</response>
        /// <response code="403">If the user is not authorized to access this information</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<HumanInformationDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<HumanInformationDto>>> Get(Guid id)
        {
            var result = await _humanInfoService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Creates new human information with profile picture
        /// </summary>
        /// <param name="dto">The human information data</param>
        /// <returns>Created human information</returns>
        /// <remarks>
        /// Profile picture requirements:
        /// - Must be JPEG, PNG, or GIF format
        /// - Maximum size: 5MB
        /// - Will be resized to 200x200 pixels
        /// </remarks>
        /// <response code="201">Returns the created human information</response>
        /// <response code="400">If the request data is invalid or profile picture is missing</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromForm] CreateHumanInformationDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            if (Request.Form.Files.Count == 0 || Request.Form.Files[0] == null)
                return StatusCode(400, ApiResponse<object>.BadRequest("Profile picture is required"));

            var profilePicture = Request.Form.Files[0];
            
            var pictureDto = new ProfilePictureUpdateDto { ProfilePicture = profilePicture };
            var validationContext = new ValidationContext(pictureDto);
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(pictureDto, validationContext, validationResults, true))
            {
                _logger.LogWarning("Invalid profile picture upload attempt: {Error}", 
                    validationResults.First().ErrorMessage);
                return StatusCode(400, ApiResponse<object>.BadRequest(validationResults.First().ErrorMessage));
            }

            var result = await _humanInfoService.CreateAsync(dto, profilePicture, userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the name of a person
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="request">The new name value</param>
        /// <returns>Success status</returns>
        /// <response code="200">If the update was successful</response>
        /// <response code="400">If the new name is invalid</response>
        /// <response code="403">If the user is not authorized to update this information</response>
        [HttpPatch("{id}/name")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateName(Guid id, [FromBody] StringUpdateDto request)
        {
            var result = await _humanInfoService.UpdateNameAsync(id, request.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the email address
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="request">The new email value</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateEmail(Guid id, [FromBody] StringUpdateDto request)
        {
            var result = await _humanInfoService.UpdateEmailAsync(id, request.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the personal code
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="request">The new personal code value</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/personal-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdatePersonalCode(Guid id, [FromBody] StringUpdateDto request)
        {
            var result = await _humanInfoService.UpdatePersonalCodeAsync(id, request.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the phone number
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="request">The new phone number value</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/phone")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdatePhone(Guid id, [FromBody] StringUpdateDto request)
        {
            var result = await _humanInfoService.UpdatePhoneAsync(id, request.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the city
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="request">The new city value</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/city")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateCity(Guid id, [FromBody] StringUpdateDto request)
        {
            var result = await _humanInfoService.UpdateCityAsync(id, request.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the street address
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="request">The new street value</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/street")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStreet(Guid id, [FromBody] StringUpdateDto request)
        {
            var result = await _humanInfoService.UpdateStreetAsync(id, request.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the house number
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="request">The new house number value</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/house-number")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateHouseNumber(Guid id, [FromBody] StringUpdateDto request)
        {
            var result = await _humanInfoService.UpdateHouseNumberAsync(id, request.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the apartment number
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="request">The new apartment number value</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/apartment-number")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateApartmentNumber(Guid id, [FromBody] StringUpdateDto request)
        {
            var result = await _humanInfoService.UpdateApartmentNumberAsync(id, request.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieves human information by user ID
        /// </summary>
        /// <param name="userId">The ID of the user whose information to retrieve</param>
        /// <returns>Human information including profile picture</returns>
        /// <response code="200">Returns the human information</response>
        /// <response code="404">If the human information is not found</response>
        /// <response code="403">If the user is not authorized to access this information</response>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<HumanInformationDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<HumanInformationDto>>> GetByUserId(Guid userId)
        {
            try 
            {
                // Check if human information exists for this user
                var info = await _humanInfoService.GetByUserIdAsync(userId);
                
                if (info == null || info.Data == null)
                {
                    return NotFound(new ApiResponse<HumanInformationDto> 
                    { 
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Please complete your profile",
                        Data = null
                    });
                }
                
                return Ok(info);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new ApiResponse<HumanInformationDto>
                {
                    IsSuccess = false,
                    StatusCode = 403,
                    Message = "Access denied",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting human information for user {UserId}", userId);
                return NotFound(new ApiResponse<HumanInformationDto>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = "Please complete your profile",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Updates the profile picture
        /// </summary>
        /// <param name="id">The ID of the human information to update</param>
        /// <param name="profilePicture">The new profile picture file</param>
        /// <returns>Success status</returns>
        /// <remarks>
        /// Profile picture requirements:
        /// - Must be JPEG, PNG, or GIF format
        /// - Maximum size: 5MB
        /// - Will be resized to 200x200 pixels
        /// </remarks>
        [HttpPatch("{id}/profile-picture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateProfilePicture(Guid id, [FromForm] ProfilePictureUpdateDto request)
        {
            var result = await _humanInfoService.UpdateProfilePictureAsync(id, request.ProfilePicture);
            return StatusCode(result.StatusCode, result);
        }
    }
} 