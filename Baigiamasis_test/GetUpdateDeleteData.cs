using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baigiamasis.Controllers;
using Baigiamasis.DTOs.Common;
using Baigiamasis.DTOs.HumanInformation;
using Baigiamasis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis_test
{
    public class GetUpdateDeleteDataTests
    {
        private readonly Mock<IHumanInformationService> _humanInfoServiceMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<ILogger<HumanInformationController>> _loggerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly HumanInformationController _controller;
        private readonly Guid _userId;
        private readonly Guid _humanInfoId;

        public GetUpdateDeleteDataTests()
        {
            _humanInfoServiceMock = new Mock<IHumanInformationService>();
            _imageServiceMock = new Mock<IImageService>();
            _loggerMock = new Mock<ILogger<HumanInformationController>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _userId = Guid.NewGuid();
            _humanInfoId = Guid.NewGuid();

            _controller = new HumanInformationController(
                _humanInfoServiceMock.Object,
                _loggerMock.Object,
                _imageServiceMock.Object,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetHumanInfo_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var expectedInfo = new HumanInformationDto
            {
                Id = _humanInfoId,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "12345678901",
                PhoneNumber = "+37061234567",
                Email = "john@gmail.com",
                City = "Vilnius",
                Street = "Test St.",
                HouseNumber = "1",
                ApartmentNumber = "1",
                UserId = _userId
            };

            var successResponse = ApiResponse<HumanInformationDto>.Success(expectedInfo);
            _humanInfoServiceMock.Setup(x => x.GetByIdAsync(_humanInfoId))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.Get(_humanInfoId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<HumanInformationDto>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<HumanInformationDto>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(expectedInfo.FirstName, response.Data.FirstName);
        }

        [Fact]
        public async Task UpdateName_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = "John Doe" };
            var successResponse = ApiResponse<bool>.Success(true, "Name updated successfully");

            _humanInfoServiceMock.Setup(x => x.UpdateNameAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UpdateName(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEmail_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = "john@gmail.com" };
            var successResponse = ApiResponse<bool>.Success(true, "Email updated successfully");

            _humanInfoServiceMock.Setup(x => x.UpdateEmailAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UpdateEmail(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePhone_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = "+37061234567" };
            var successResponse = ApiResponse<bool>.Success(true, "Phone number updated successfully");

            _humanInfoServiceMock.Setup(x => x.UpdatePhoneAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UpdatePhone(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProfilePicture_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var file = new Mock<IFormFile>();
            var dto = new ProfilePictureUpdateDto { ProfilePicture = file.Object };
            
            var successResponse = ApiResponse<bool>.Success(true);
            _humanInfoServiceMock
                .Setup(x => x.UpdateProfilePictureAsync(_humanInfoId, file.Object))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UpdateProfilePicture(_humanInfoId, dto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetByUserId_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var expectedInfo = new HumanInformationDto
            {
                Id = _humanInfoId,
                UserId = _userId,
                FirstName = "John",
                LastName = "Doe"
            };

            var successResponse = ApiResponse<HumanInformationDto>.Success(expectedInfo);
            _humanInfoServiceMock.Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.GetByUserId(_userId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<HumanInformationDto>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<HumanInformationDto>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(_userId, response.Data.UserId);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task UpdateName_WithInvalidData_ReturnsBadRequest(string invalidName)
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = invalidName };
            var errorResponse = ApiResponse<bool>.BadRequest("Name cannot be empty");

            _humanInfoServiceMock.Setup(x => x.UpdateNameAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.UpdateName(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
        }
    }
}
