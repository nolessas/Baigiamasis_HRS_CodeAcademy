using Baigiamasis.DTOs;
using Baigiamasis.Models;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Services.Repositories.Interfaces;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis.Services.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordService _passwordService;
        private readonly IUserRepository _userRepository;

        public UserService(
            ILogger<UserService> logger,
            IPasswordService passwordService,
            IUserRepository userRepository)
        {
            _logger = logger;
            _passwordService = passwordService;
            _userRepository = userRepository;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
                return null;

            if (!_passwordService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("Invalid password attempt for user: {Username}", username);
                return null;
            }

            await _userRepository.UpdateLastLoginAsync(user.Id);
            return user;
        }

        public async Task<ApiResponse<object>> Signup(string username, string password)
        {
            try
            {
                // Validate input
                if (!IsValidUsername(username))
                {
                    return ApiResponse<object>.BadRequest("Invalid username format");
                }

                if (!IsValidPassword(password))
                {
                    return ApiResponse<object>.BadRequest("Invalid password format");
                }

                // Check for existing username
                var existingUser = await _userRepository.GetByUsernameAsync(username);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing username: {Username}", username);
                    throw new InvalidOperationException("Username already exists");
                }

                // Create password hash
                _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Roles = new List<string> { "User" },
                    CreatedDate = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(user);
                _logger.LogInformation("User created successfully: {Username}", username);

                return ApiResponse<object>.Created(
                    new { createdUser.Id }, 
                    "User created successfully"
                );
            }
            catch (InvalidOperationException ex)
            {
                // Rethrow username exists exception to be handled by controller
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup for user {Username}", username);
                return ApiResponse<object>.Failure("An error occurred during registration", 500);
            }
        }

        private bool IsValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   username.Length >= 3 &&
                   username.Length <= 50 &&
                   username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '-');
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return false;

            return password.Any(char.IsUpper) && 
                   password.Any(char.IsLower) && 
                   password.Any(char.IsDigit);
        }
    }
}