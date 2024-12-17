using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Baigiamasis.Controllers;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;
using Baigiamasis.DTOs.Common;

namespace Baigiamasis_test
{
    public class AuthorizationTests
    {
        private readonly HumanInformationController _controller;
        private readonly Mock<IHumanInformationService> _humanInfoServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<ILogger<HumanInformationController>> _loggerMock;
        private readonly Guid _humanInfoId;

        public AuthorizationTests()
        {
            _humanInfoServiceMock = new Mock<IHumanInformationService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _imageServiceMock = new Mock<IImageService>();
            _loggerMock = new Mock<ILogger<HumanInformationController>>();
            _humanInfoId = Guid.NewGuid();

            _controller = new HumanInformationController(
                _humanInfoServiceMock.Object,
                _loggerMock.Object,
                _imageServiceMock.Object,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task UpdateHumanInfo_WhenUserNotOwner_ReturnsForbidden()
        {
            // Arrange
            var differentUserId = Guid.NewGuid();
            _currentUserServiceMock.Setup(x => x.UserId)
                .Returns(differentUserId);
            _currentUserServiceMock.Setup(x => x.CanAccessUser(It.IsAny<Guid>()))
                .Returns(false);

            var updateRequest = new StringUpdateDto { Value = "NewName" };
            var errorResponse = ApiResponse<bool>.Forbidden("Access denied");

            _humanInfoServiceMock.Setup(x => x.UpdateNameAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.UpdateName(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var forbiddenResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(403, forbiddenResult.StatusCode);
        }
    }
} 