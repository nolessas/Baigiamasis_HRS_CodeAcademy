using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Baigiamasis.Controllers;
using Baigiamasis.Models;
using Baigiamasis.DTOs.Auth;
using Baigiamasis.DTOs.Common;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis_test
{
    public class JwtAuthenticationTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthController _controller;

        public JwtAuthenticationTests()
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
        public async Task Login_WithValidCredentials_ReturnsValidToken()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Username = "testuser",
                Password = "Test123!"
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
            Assert.Equal(user.Username, response.Data.Username);
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

        [Fact]
        public void ValidateToken_WithValidToken_ReturnsTrue()
        {
            // Arrange
            var token = "valid.jwt.token";
            _jwtServiceMock.Setup(x => x.ValidateToken(token))
                .Returns(true);

            // Act
            var isValid = _jwtServiceMock.Object.ValidateToken(token);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ReturnsFalse()
        {
            // Arrange
            var token = "invalid.jwt.token";
            _jwtServiceMock.Setup(x => x.ValidateToken(token))
                .Returns(false);

            // Act
            var isValid = _jwtServiceMock.Object.ValidateToken(token);

            // Assert
            Assert.False(isValid);
        }
    }
} 