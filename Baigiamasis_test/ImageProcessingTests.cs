using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Baigiamasis.Services.Services;
using Baigiamasis.Services.Services.Interfaces;
using Baigiamasis.Services.Repositories.Interfaces;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Controllers;
using Baigiamasis.DTOs.Common;
using Baigiamasis.DTOs.HumanInformation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Baigiamasis_test
{
    public class ImageProcessingTests
    {
        private readonly ILogger<ImageService> _logger;
        private readonly Mock<IImageRepository> _imageRepositoryMock;
        private readonly Mock<IHumanInformationService> _humanInfoServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly ImageService _imageService;
        private readonly HumanInformationController _controller;
        private readonly Guid _humanInfoId;

        public ImageProcessingTests()
        {
            _logger = Mock.Of<ILogger<ImageService>>();
            _imageRepositoryMock = new Mock<IImageRepository>();
            _humanInfoServiceMock = new Mock<IHumanInformationService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _imageService = new ImageService(_logger, _imageRepositoryMock.Object);
            _humanInfoId = Guid.NewGuid();

            var controllerLogger = Mock.Of<ILogger<HumanInformationController>>();
            _controller = new HumanInformationController(
                _humanInfoServiceMock.Object,
                controllerLogger,
                _imageService,
                _currentUserServiceMock.Object);
        }

        private IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
        {
            var stream = new MemoryStream(content);
            var file = new FormFile(
                baseStream: stream,
                baseStreamOffset: 0,
                length: content.Length,
                name: "file",
                fileName: fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
            return file;
        }

        private byte[] CreateTestImage(int width, int height)
        {
            using var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height);
            using var memStream = new MemoryStream();
            image.Save(memStream, new JpegEncoder());
            return memStream.ToArray();
        }

        [Fact]
        public async Task ProcessProfileImage_ValidImage_ResizesTo200x200()
        {
            // Arrange
            var imageBytes = CreateTestImage(400, 300);
            var file = CreateFormFile(imageBytes, "test.jpg", "image/jpeg");

            // Act
            var result = await _imageService.ProcessProfileImageAsync(file);

            // Assert
            using var processedImage = Image.Load(result);
            Assert.Equal(200, processedImage.Width);
            Assert.Equal(200, processedImage.Height);
        }

        [Fact]
        public async Task ProcessProfileImage_SmallImage_StretchesTo200x200()
        {
            // Arrange
            var imageBytes = CreateTestImage(100, 100);
            var file = CreateFormFile(imageBytes, "small-test.jpg", "image/jpeg");

            // Act
            var result = await _imageService.ProcessProfileImageAsync(file);

            // Assert
            using var processedImage = Image.Load(result);
            Assert.Equal(200, processedImage.Width);
            Assert.Equal(200, processedImage.Height);
        }

        [Fact]
        public async Task UpdateProfileImage_ValidImage_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var imageBytes = CreateTestImage(200, 200);
            var file = CreateFormFile(imageBytes, "test.jpg", "image/jpeg");

            _imageRepositoryMock.Setup(x => x.UpdateProfileImageAsync(userId, It.IsAny<byte[]>()))
                .ReturnsAsync(true);

            // Act
            var result = await _imageService.UpdateProfileImageAsync(userId, file);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetProfileImage_ReturnsImage_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedImage = CreateTestImage(200, 200);
            _imageRepositoryMock.Setup(x => x.UserExistsAsync(userId))
                .ReturnsAsync(true);
            _imageRepositoryMock.Setup(x => x.GetProfileImageAsync(userId))
                .ReturnsAsync(expectedImage);

            // Act
            var result = await _imageService.GetProfileImageAsync(userId);

            // Assert
            Assert.Equal(expectedImage, result);
        }

        [Fact]
        public async Task GetProfileImage_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _imageRepositoryMock.Setup(x => x.UserExistsAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _imageService.GetProfileImageAsync(userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProfilePicture_WithInvalidFileType_ReturnsBadRequest()
        {
            // Arrange
            var file = new Mock<IFormFile>();
            file.Setup(f => f.ContentType).Returns("application/pdf");
            file.Setup(f => f.Length).Returns(1024); // 1KB
            var dto = new ProfilePictureUpdateDto { ProfilePicture = file.Object };
            
            var errorResponse = ApiResponse<bool>.BadRequest("Invalid file type. Only JPEG, PNG and GIF are allowed.");
            _humanInfoServiceMock.Setup(x => x.UpdateProfilePictureAsync(_humanInfoId, file.Object))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.UpdateProfilePicture(_humanInfoId, dto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProfilePicture_WithFileTooLarge_ReturnsBadRequest()
        {
            // Arrange
            var file = new Mock<IFormFile>();
            file.Setup(f => f.ContentType).Returns("image/jpeg");
            file.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB
            var dto = new ProfilePictureUpdateDto { ProfilePicture = file.Object };
            
            var errorResponse = ApiResponse<bool>.BadRequest("File size too large. Maximum size is 5MB.");
            _humanInfoServiceMock.Setup(x => x.UpdateProfilePictureAsync(_humanInfoId, file.Object))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.UpdateProfilePicture(_humanInfoId, dto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<bool>>>(result);
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
} 