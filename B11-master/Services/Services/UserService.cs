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

            // Update last login time
            await _userRepository.UpdateLastLoginAsync(user.Id);

            return user;
        }

        public async Task<ApiResponse<object>> Signup(string username, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required");

            if (password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters long");

            // Check if username exists
            var existingUser = await _userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
                throw new InvalidOperationException("Username already exists");

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

            try
            {
                var createdUser = await _userRepository.CreateAsync(user);
                return ApiResponse<object>.Created(new { createdUser.Id }, "User created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup for user {Username}", username);
                throw;
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
            return !string.IsNullOrWhiteSpace(password) &&
                   password.Length >= 6 &&
                   password.Length <= 100;
        }
    }
}