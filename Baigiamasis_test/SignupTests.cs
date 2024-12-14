using Baigiamasis.Controllers;
using Baigiamasis.DTOs.User;
using Baigiamasis.DTOs.Common;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Baigiamasis.Models;
using Baigiamasis.DTOs.Auth;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis_test
{
    public class SignupTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthController _controller;

        public SignupTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _mapperMock = new Mock<IMapper>();
            _controller = new AuthController(
                _userServiceMock.Object,
                _jwtServiceMock.Object,
                _loggerMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task Signup_WithValidData_CreatesUserWithDefaultRole()
        {
            // Arrange
            var request = new UserRegistrationDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var successResponse = ApiResponse<object>.Created(
                new { Id = Guid.NewGuid() },
                "User created successfully"
            );

            _userServiceMock.Setup(x => x.Signup(request.Username, request.Password))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.Signup(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<object>>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.Contains("created successfully", response.Message);
        }

        [Theory]
        [InlineData("", "Password123!", "Username is required")]
        [InlineData("user", "", "Password is required")]
        [InlineData("us", "Password123!", "Username must be between 3 and 50 characters")]
        [InlineData("user", "pass", "Password must be at least 6 characters")]
        public async Task Signup_WithInvalidData_ReturnsBadRequest(string username, string password, string expectedError)
        {
            // Arrange
            var signupRequest = new UserRegistrationDto
            {
                Username = username,
                Password = password
            };

            var errorResponse = ApiResponse<object>.BadRequest(expectedError);
            _userServiceMock.Setup(x => x.Signup(signupRequest.Username, signupRequest.Password))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Signup(signupRequest);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(actionResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains(expectedError, response.Message);
        }

        [Theory]
        [InlineData("user@123", "Password123!", "Username contains invalid characters")]
        [InlineData("admin", "Password123!", "Username 'admin' is reserved")]
        [InlineData("validuser", "pass word", "Password contains invalid characters")]
        public async Task Signup_WithInvalidFormat_ReturnsBadRequest(string username, string password, string expectedError)
        {
            // Arrange
            var signupRequest = new UserRegistrationDto
            {
                Username = username,
                Password = password
            };

            var errorResponse = ApiResponse<object>.BadRequest(expectedError);
            _userServiceMock.Setup(x => x.Signup(signupRequest.Username, signupRequest.Password))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Signup(signupRequest);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(actionResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains(expectedError, response.Message);
        }

        [Fact]
        public async Task Signup_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var request = new UserRegistrationDto
            {
                Username = "existinguser",
                Password = "Password123!"
            };

            var errorResponse = ApiResponse<object>.BadRequest("Username already exists");
            _userServiceMock.Setup(x => x.Signup(request.Username, request.Password))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Signup(request);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(actionResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Equal("Username already exists", response.Message);
        }

        [Fact]
        public async Task Signup_WithNullRequest_ReturnsBadRequest()
        {
            // Arrange
            UserRegistrationDto? nullRequest = null;

            // Act &  Assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(
                async () => await _controller.Signup(nullRequest));
            Assert.NotNull(exception);
        }

        [Fact]
        public void UserRegistrationDto_ValidationAttributes_AreCorrect()
        {
            // Arrange
            var properties = typeof(UserRegistrationDto).GetProperties();
            
            // Assert
            var usernameProperty = properties.First(p => p.Name == "Username");
            var usernameRequired = usernameProperty.GetCustomAttributes(typeof(RequiredAttribute), false).Any();
            var usernameLength = usernameProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault() as StringLengthAttribute;
            
            Assert.True(usernameRequired);
            Assert.NotNull(usernameLength);
            Assert.Equal(50, usernameLength.MaximumLength);
            Assert.Equal(3, usernameLength.MinimumLength);

            var passwordProperty = properties.First(p => p.Name == "Password");
            var passwordRequired = passwordProperty.GetCustomAttributes(typeof(RequiredAttribute), false).Any();
            var passwordMinLength = passwordProperty.GetCustomAttributes(typeof(MinLengthAttribute), false).FirstOrDefault() as MinLengthAttribute;

            Assert.True(passwordRequired);
            Assert.NotNull(passwordMinLength);
            Assert.Equal(6, passwordMinLength.Length);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Roles = new List<string> { "User" }
            };

            var loginResponse = new LoginResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Token = "test.jwt.token",
                Roles = user.Roles,
                ExpiresAt = DateTime.UtcNow.AddHours(3)
            };

            _userServiceMock.Setup(x => x.ValidateUserAsync(request.Username, request.Password))
                .ReturnsAsync(user);

            _mapperMock.Setup(x => x.Map<LoginResponseDto>(user))
                .Returns(loginResponse);

            _jwtServiceMock.Setup(x => x.GenerateToken(user.Id.ToString(), user.Username, user.Roles))
                .Returns(loginResponse.Token);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<LoginResponseDto>>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<LoginResponseDto>>(objectResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Data.Token);
            Assert.Equal(user.Id, response.Data.UserId);
            Assert.Equal(user.Username, response.Data.Username);
            Assert.Contains("User", response.Data.Roles);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            _userServiceMock.Setup(x => x.ValidateUserAsync(request.Username, request.Password))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<LoginResponseDto>>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<LoginResponseDto>>(objectResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(401, response.StatusCode);
            Assert.Contains("Invalid username or password", response.Message);
        }
    }
} 