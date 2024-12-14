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

        [Theory]
        [InlineData("image/jpeg", true)]
        [InlineData("image/jpg", true)]
        [InlineData("image/png", true)]
        [InlineData("image/gif", false)]
        [InlineData("text/plain", false)]
        public void IsValidImage_ChecksFileType(string contentType, bool expectedResult)
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.jpg");

            // Act
            var result = _imageService.IsValidImage(fileMock.Object);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IsValidImage_FileTooLarge_ReturnsFalse()
        {
            // Arrange
            var file = CreateFormFile(new byte[6 * 1024 * 1024], "large.jpg", "image/jpeg");

            // Act
            var result = _imageService.IsValidImage(file);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidImage_EmptyFile_ReturnsFalse()
        {
            // Arrange
            var file = CreateFormFile(Array.Empty<byte>(), "empty.jpg", "image/jpeg");

            // Act
            var result = _imageService.IsValidImage(file);

            // Assert
            Assert.False(result);
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