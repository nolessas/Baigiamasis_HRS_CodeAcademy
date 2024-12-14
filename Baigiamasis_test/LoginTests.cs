using Baigiamasis.Controllers;
using Baigiamasis.DTOs.Auth;
using Baigiamasis.DTOs.Common;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis_test
{
    public class LoginTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthController _controller;

        public LoginTests()
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
        public async Task Login_WithValidCredentials_ReturnsSuccessResponse()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var user = new Baigiamasis.Models.User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Roles = new List<string> { "User" }
            };

            var loginResponse = new LoginResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Roles = user.Roles,
                Token = "test-token",
                ExpiresAt = DateTime.UtcNow.AddHours(3)
            };

            _userServiceMock.Setup(x => x.ValidateUserAsync(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync(user);

            _jwtServiceMock.Setup(x => x.GenerateToken(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<List<string>>()))
                .Returns("test-token");

            _mapperMock.Setup(x => x.Map<LoginResponseDto>(It.IsAny<Baigiamasis.Models.User>()))
                .Returns(loginResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<LoginResponseDto>>(actionResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Data?.Token);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                Username = "wronguser",
                Password = "wrongpass"
            };

            _userServiceMock.Setup(x => x.ValidateUserAsync(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync((Baigiamasis.Models.User?)null);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<LoginResponseDto>>(actionResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(401, response.StatusCode);
        }
    }
} 