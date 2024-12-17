using Baigiamasis.Controllers;
using Baigiamasis.DTOs.User;
using Baigiamasis.DTOs.Auth;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Models;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace Baigiamasis_test
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
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
        public async Task Signup_WithValidData_ReturnsCreatedResponse()
        {
            // Arrange
            var request = new UserRegistrationDto
            {
                Username = "testuser",
                Password = "Password123"
            };

            var successResponse = ApiResponse<object>.Created(
                new { Id = Guid.NewGuid() },
                "User registered successfully"
            );

            _userServiceMock.Setup(x => x.Signup(request.Username, request.Password))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.Signup(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<object>>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(201, objectResult.StatusCode);

            var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(201, response.StatusCode);
            Assert.Contains("registered successfully", response.Message);
        }

        [Theory]
        [InlineData("", "Password123", "Username is required")]
        [InlineData("us", "Password123", "Username must be between 3 and 50 characters")]
        [InlineData("user", "pass", "Password must be at least 6 characters")]
        [InlineData("user@123", "Password123", "Username can only contain letters, numbers, and .-_")]
        public async Task Signup_WithInvalidData_ReturnsBadRequest(string username, string password, string expectedError)
        {
            // Arrange
            var request = new UserRegistrationDto
            {
                Username = username,
                Password = password
            };

            var errorResponse = ApiResponse<object>.BadRequest(expectedError);
            _userServiceMock.Setup(x => x.Signup(request.Username, request.Password))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.Signup(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<object>>>(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);
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
                Password = "Password123"
            };

            _userServiceMock.Setup(x => x.Signup(request.Username, request.Password))
                .ThrowsAsync(new InvalidOperationException("Username already exists"));

            // Act
            var result = await _controller.Signup(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<object>>>(result);
            var objectResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Username already taken", response.Message);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccessWithToken()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Username = "testuser",
                Password = "Password123"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Roles = new List<string> { "User" }
            };

            var expectedResponse = new LoginResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Token = "test.jwt.token",
                Roles = user.Roles,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _userServiceMock.Setup(x => x.ValidateUserAsync(request.Username, request.Password))
                .ReturnsAsync(user);
            _mapperMock.Setup(x => x.Map<LoginResponseDto>(user))
                .Returns(expectedResponse);
            _jwtServiceMock.Setup(x => x.GenerateToken(user.Id.ToString(), user.Username, user.Roles))
                .Returns(expectedResponse.Token);
            _jwtServiceMock.Setup(x => x.GetTokenExpirationTime())
                .Returns(expectedResponse.ExpiresAt);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<LoginResponseDto>>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(200, objectResult.StatusCode);

            var response = Assert.IsType<ApiResponse<LoginResponseDto>>(objectResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(expectedResponse.Token, response.Data.Token);
            Assert.Equal(expectedResponse.Username, response.Data.Username);
            Assert.Equal(expectedResponse.UserId, response.Data.UserId);
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
            Assert.Equal(401, objectResult.StatusCode);

            var response = Assert.IsType<ApiResponse<LoginResponseDto>>(objectResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Contains("Invalid username or password", response.Message);
        }

        [Fact]
        public void UserRegistrationDto_ValidationAttributes_AreCorrect()
        {
            // Arrange & Act
            var properties = typeof(UserRegistrationDto).GetProperties();
            
            // Assert Username attributes
            var usernameProperty = properties.First(p => p.Name == "Username");
            var usernameRequired = usernameProperty.GetCustomAttributes(typeof(RequiredAttribute), false).Any();
            var usernameLength = usernameProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault() as StringLengthAttribute;
            var usernameRegex = usernameProperty.GetCustomAttributes(typeof(RegularExpressionAttribute), false).FirstOrDefault() as RegularExpressionAttribute;
            
            Assert.True(usernameRequired);
            Assert.NotNull(usernameLength);
            Assert.Equal(50, usernameLength.MaximumLength);
            Assert.Equal(3, usernameLength.MinimumLength);
            Assert.NotNull(usernameRegex);
            Assert.Equal(@"^[a-zA-Z0-9._-]+$", usernameRegex.Pattern);

            // Assert Password attributes
            var passwordProperty = properties.First(p => p.Name == "Password");
            var passwordRequired = passwordProperty.GetCustomAttributes(typeof(RequiredAttribute), false).Any();
            var passwordMinLength = passwordProperty.GetCustomAttributes(typeof(MinLengthAttribute), false).FirstOrDefault() as MinLengthAttribute;
            var passwordRegex = passwordProperty.GetCustomAttributes(typeof(RegularExpressionAttribute), false).FirstOrDefault() as RegularExpressionAttribute;

            Assert.True(passwordRequired);
            Assert.NotNull(passwordMinLength);
            Assert.Equal(6, passwordMinLength.Length);
            Assert.NotNull(passwordRegex);
            Assert.Equal(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", passwordRegex.Pattern);
        }
    }
} 