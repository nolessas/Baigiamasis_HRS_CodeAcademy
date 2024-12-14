using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Baigiamasis.DTOs.User;
using System.Security.Claims;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis.Controllers
{
    /// <summary>
    /// Controller responsible for administrative operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Initializes a new instance of the AdminController
        /// </summary>
        /// <param name="adminService">Service for administrative operations</param>
        /// <param name="logger">Logger for administrative events</param>
        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of all users in the system
        /// </summary>
        /// <returns>List of all users with their basic information</returns>
        /// <response code="200">Returns the list of users</response>
        /// <response code="403">If the user is not authorized as admin</response>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IEnumerable<UserDto>>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var currentUser = User.Identity.Name;
            var userRoles = string.Join(", ", User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value));
                    
            _logger.LogInformation("Getting all users. User: {User}, Roles: {Roles}", 
                currentUser, userRoles);
            
            var users = await _adminService.GetAllUsersAsync();
            return StatusCode(200, ApiResponse<IEnumerable<UserDto>>.Success(users));
        }

        /// <summary>
        /// Deletes a user and their associated information from the system
        /// </summary>
        /// <param name="id">The ID of the user to delete</param>
        /// <returns>Success status of the deletion operation</returns>
        /// <remarks>
        /// This operation will also delete:
        /// - User's human information
        /// - User's profile picture
        /// - Any associated data
        /// </remarks>
        /// <response code="200">If the user was successfully deleted</response>
        /// <response code="404">If the user was not found</response>
        /// <response code="403">If the current user is not authorized as admin</response>
        [HttpDelete("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid id)
        {
            _logger.LogInformation("Attempting to delete user {UserId}", id);
            var result = await _adminService.DeleteUserAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
} 