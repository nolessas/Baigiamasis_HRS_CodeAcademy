using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Baigiamasis.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Services.Repositories.Interfaces;
using Baigiamasis.Services.Auth.Interfaces;

namespace Baigiamasis.Controllers
{
    /// <summary>
    /// Controller responsible for handling image-related operations
    /// </summary>
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ImageController> _logger;

        /// <summary>
        /// Initializes a new instance of the ImageController
        /// </summary>
        /// <param name="imageService">Service for image processing operations</param>
        /// <param name="currentUserService">Service for current user operations</param>
        /// <param name="logger">Logger for image-related events</param>
        public ImageController(
            IImageService imageService,
            ICurrentUserService currentUserService,
            ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a user's profile picture
        /// </summary>
        /// <param name="userId">The ID of the user whose profile picture to retrieve</param>
        /// <returns>The profile picture as a JPEG image</returns>
        /// <remarks>
        /// The image is returned in JPEG format regardless of the original upload format.
        /// The image dimensions are 200x200 pixels.
        /// </remarks>
        /// <response code="200">Returns the profile picture</response>
        /// <response code="404">If the profile picture is not found</response>
        /// <response code="403">If the user is not authorized to access this image</response>
        [HttpGet("profile/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetProfileImage(Guid userId)
        {
            if (!_currentUserService.CanAccessUser(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to profile picture. RequestingUser: {RequestingUserId}, TargetUser: {UserId}", 
                    _currentUserService.UserId, userId);
                return StatusCode(403, ApiResponse<object>.Forbidden("Access denied"));
            }

            var profilePicture = await _imageService.GetProfileImageAsync(userId);
            
            if (profilePicture == null)
                return StatusCode(404, ApiResponse<object>.NotFound("Profile picture not found"));

            return File(profilePicture, "image/jpeg");
        }
    }
} 