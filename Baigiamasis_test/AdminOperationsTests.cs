using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Baigiamasis.Controllers;
using Baigiamasis.DTOs.Common;
using Baigiamasis.DTOs.User;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis_test
{
    public class AdminOperationsTests
    {
        private readonly Mock<IAdminService> _adminServiceMock;
        private readonly Mock<ILogger<AdminController>> _loggerMock;
        private readonly AdminController _controller;

        public AdminOperationsTests()
        {
            _adminServiceMock = new Mock<IAdminService>();
            _loggerMock = new Mock<ILogger<AdminController>>();
            
            _controller = new AdminController(
                _adminServiceMock.Object,
                _loggerMock.Object);

            // Setup controller context with admin claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task DeleteUser_AsAdmin_DeletesUserAndRelatedInfo()
        {
            // Arrange 
            var userId = Guid.NewGuid();
            _adminServiceMock.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(ApiResponse<bool>.Success(true, "User deleted successfully"));

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(objectResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _adminServiceMock.Setup(x => x.DeleteUserAsync(userId))
                .ThrowsAsync(new KeyNotFoundException($"User with ID {userId} not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteUser(userId));
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = Guid.NewGuid(), Username = "user1" },
                new UserDto { Id = Guid.NewGuid(), Username = "user2" }
            };

            _adminServiceMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<UserDto>>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<IEnumerable<UserDto>>>(objectResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(2, ((List<UserDto>)response.Data).Count);
        }

        [Fact]
        public async Task GetUsers_WhenNoUsers_ThrowsKeyNotFoundException()
        {
            // Arrange
            _adminServiceMock.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new KeyNotFoundException("No users found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetUsers());
            Assert.Equal("No users found", exception.Message);
        }
    }
} 