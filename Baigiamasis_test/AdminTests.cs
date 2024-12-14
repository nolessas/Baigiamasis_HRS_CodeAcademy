using Baigiamasis.Controllers;
using Baigiamasis.DTOs.Common;
using Baigiamasis.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis_test
{
    public class AdminTests
    {
        private readonly Mock<IAdminService> _adminServiceMock;
        private readonly Mock<ILogger<AdminController>> _loggerMock;
        private readonly AdminController _controller;

        public AdminTests()
        {
            _adminServiceMock = new Mock<IAdminService>();
            _loggerMock = new Mock<ILogger<AdminController>>();
            _controller = new AdminController(_adminServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var successResponse = ApiResponse<bool>.Success(true, "User deleted successfully");

            _adminServiceMock.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsSuccess()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto 
                { 
                    Id = Guid.NewGuid(),
                    Username = "testuser1",
                    Roles = new List<string> { "User" },
                    CreatedDate = DateTime.UtcNow,
                    HasHumanInformation = false
                },
                new UserDto 
                { 
                    Id = Guid.NewGuid(),
                    Username = "testuser2",
                    Roles = new List<string> { "User" },
                    CreatedDate = DateTime.UtcNow,
                    HasHumanInformation = true
                }
            };

            _adminServiceMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            SetupAdminUser();

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<IEnumerable<UserDto>>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count());
        }

        [Fact]
        public async Task GetAllUsers_WithNoUsers_ReturnsNotFound()
        {
            // Arrange
            SetupAdminUser();
            _adminServiceMock.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new KeyNotFoundException("No users found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _controller.GetUsers());
        }

        [Fact]
        public async Task GetAllUsers_WithUnauthorizedAccess_ReturnsForbidden()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "regularuser"),
                new Claim(ClaimTypes.Role, "User") // Non-admin role
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            _adminServiceMock.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => 
                await _controller.GetUsers());
        }

        [Fact]
        public async Task VerifyAdminAccess_ReturnsSuccess()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Name, "testAdmin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var response = Assert.IsType<ApiResponse<IEnumerable<UserDto>>>(okResult.Value);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnUsers_WhenUserIsAdmin()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Name, "testAdmin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            var users = new List<UserDto> { new UserDto { /* properties */ } };
            _adminServiceMock.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            var response = Assert.IsType<ApiResponse<IEnumerable<UserDto>>>(okResult.Value);
            Assert.Equal(users, response.Data);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnSuccess_WhenUserIsAdmin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            // Setup admin claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Name, "testAdmin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            _adminServiceMock.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(ApiResponse<bool>.Success(true));

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var response = Assert.IsType<ApiResponse<bool>>(((ObjectResult)result.Result).Value);
            Assert.True(response.IsSuccess);
        }

        private void SetupAdminUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }
    }
} 