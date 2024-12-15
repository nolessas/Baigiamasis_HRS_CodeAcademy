using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Baigiamasis.Services.Services;
using Baigiamasis.Services.Services.Interfaces;
using Baigiamasis.Services.Repositories.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Baigiamasis_test
{
    public class ImageProcessingTests
    {
        private readonly ILogger<ImageService> _logger;
        private readonly Mock<IImageRepository> _imageRepositoryMock;
        private readonly ImageService _imageService;

        public ImageProcessingTests()
        {
            _logger = Mock.Of<ILogger<ImageService>>();
            _imageRepositoryMock = new Mock<IImageRepository>();
            _imageService = new ImageService(_logger, _imageRepositoryMock.Object);
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
    }
} 