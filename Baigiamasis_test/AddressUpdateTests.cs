using Baigiamasis.Controllers;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Baigiamasis_test
{
    public class AddressUpdateTests
    {
        private readonly Mock<IHumanInformationService> _humanInfoServiceMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<ILogger<HumanInformationController>> _loggerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly HumanInformationController _controller;
        private readonly Guid _humanInfoId;

        public AddressUpdateTests()
        {
            _humanInfoServiceMock = new Mock<IHumanInformationService>();
            _imageServiceMock = new Mock<IImageService>();
            _loggerMock = new Mock<ILogger<HumanInformationController>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _humanInfoId = Guid.NewGuid();

            _controller = new HumanInformationController(
                _humanInfoServiceMock.Object,
                _loggerMock.Object,
                _imageServiceMock.Object,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task UpdateCity_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = "Vilnius" };
            var successResponse = ApiResponse<bool>.Success(true, "City updated successfully");
            successResponse.StatusCode = 200;

            _humanInfoServiceMock.Setup(x => x.UpdateCityAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UpdateCity(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("City updated successfully", response.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task UpdateCity_WithInvalidData_ReturnsBadRequest(string invalidCity)
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = invalidCity };
            var errorResponse = ApiResponse<bool>.BadRequest("City cannot be empty");

            _humanInfoServiceMock.Setup(x => x.UpdateCityAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.UpdateCity(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("City cannot be empty", response.Message);
        }

        [Fact]
        public async Task UpdateStreet_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = "Test Street" };
            var successResponse = ApiResponse<bool>.Success(true, "Street updated successfully");
            successResponse.StatusCode = 200;

            _humanInfoServiceMock.Setup(x => x.UpdateStreetAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UpdateStreet(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Street updated successfully", response.Message);
        }

        [Fact]
        public async Task UpdateHouseNumber_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = "123A" };
            var successResponse = ApiResponse<bool>.Success(true, "House number updated successfully");
            successResponse.StatusCode = 200;

            _humanInfoServiceMock.Setup(x => x.UpdateHouseNumberAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UpdateHouseNumber(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("House number updated successfully", response.Message);
        }

        [Fact]
        public async Task UpdateApartmentNumber_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = "45" };
            var successResponse = ApiResponse<bool>.Success(true, "Apartment number updated successfully");
            successResponse.StatusCode = 200;

            _humanInfoServiceMock.Setup(x => x.UpdateApartmentNumberAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UpdateApartmentNumber(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Apartment number updated successfully", response.Message);
        }

        [Fact]
        public async Task UpdateCity_UnauthorizedAccess_ReturnsForbidden()
        {
            // Arrange
            var updateRequest = new StringUpdateDto { Value = "Vilnius" };
            var forbiddenResponse = ApiResponse<bool>.Forbidden("Access denied");

            _humanInfoServiceMock.Setup(x => x.UpdateCityAsync(_humanInfoId, updateRequest.Value))
                .ReturnsAsync(forbiddenResponse);

            // Act
            var result = await _controller.UpdateCity(_humanInfoId, updateRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var forbiddenResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<bool>>(forbiddenResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(403, forbiddenResult.StatusCode);
            Assert.Equal("Access denied", response.Message);
        }
    }
} 