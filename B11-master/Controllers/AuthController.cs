using Baigiamasis.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Baigiamasis.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Baigiamasis.DTOs;
using AutoMapper;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis.Controllers
{
    /// <summary>
    /// Controller responsible for authentication operations including user login and registration
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the AuthController
        /// </summary>
        /// <param name="userService">Service for user-related operations</param>
        /// <param name="jwtService">Service for JWT token operations</param>
        /// <param name="logger">Logger for authentication events</param>
        /// <param name="mapper">AutoMapper instance for DTO mappings</param>
        public AuthController(
            IUserService userService,
            IJwtService jwtService,
            ILogger<AuthController> logger,
            IMapper mapper)
        {
            _userService = userService;
            _jwtService = jwtService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="request">Login credentials containing username and password</param>
        /// <returns>JWT token and user information on successful login</returns>
        /// <response code="200">Returns the JWT token and user data</response>
        /// <response code="401">If the username or password is incorrect</response>
        /// <response code="400">If the request model is invalid</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<LoginResponseDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userService.ValidateUserAsync(request.Username, request.Password);
            
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                return StatusCode(401, ApiResponse<LoginResponseDto>.Unauthorized("Invalid username or password"));
            }

            var response = _mapper.Map<LoginResponseDto>(user);
            response.Token = _jwtService.GenerateToken(user.Id.ToString(), user.Username, user.Roles);
            response.ExpiresAt = _jwtService.GetTokenExpirationTime();

            return StatusCode(200, ApiResponse<LoginResponseDto>.Success(response, "Login successful"));
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request">Registration data containing username and password</param>
        /// <returns>Success message on successful registration</returns>
        /// <remarks>
        /// The password must meet the following requirements:
        /// - At least 6 characters long
        /// - Contains at least one uppercase letter
        /// - Contains at least one lowercase letter
        /// - Contains at least one number
        /// </remarks>
        /// <response code="201">Returns success message if registration is successful</response>
        /// <response code="400">If the username is taken or password requirements are not met</response>
        [HttpPost("signup")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<object>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> Signup([FromBody] UserRegistrationDto request)
        {
            try 
            {
                var result = await _userService.Signup(request.Username, request.Password);
                return StatusCode(result.StatusCode, result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Username already exists"))
            {
                return BadRequest(ApiResponse<object>.Failure("Username already taken", 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, ApiResponse<object>.Failure("An error occurred during registration"));
            }
        }
    }
} 