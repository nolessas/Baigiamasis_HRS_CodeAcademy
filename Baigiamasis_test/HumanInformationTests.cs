using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;
using Baigiamasis.Controllers;
using Baigiamasis.DTOs.HumanInformation;
using Baigiamasis.DTOs.Common;
using Baigiamasis.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Baigiamasis.Services.Auth.Interfaces;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis_test
{
    public class HumanInformationTests
    {
        private readonly Mock<IHumanInformationService> _serviceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly HumanInformationController _controller;
        private readonly ILogger<HumanInformationController> _logger;

        public HumanInformationTests()
        {
            _serviceMock = new Mock<IHumanInformationService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _imageServiceMock = new Mock<IImageService>();
            _logger = Mock.Of<ILogger<HumanInformationController>>();

            _controller = new HumanInformationController(
                _serviceMock.Object,
                _logger,
                _imageServiceMock.Object,
                _currentUserServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateHumanInfo_WhenUserAlreadyHasInfo_ReturnsError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

            var request = new CreateHumanInformationDto
            {
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "39001010000",
                PhoneNumber = "+37061234567",
                Email = "test@gmail.com",
                Street = "Gedimino",
                City = "Vilnius",
                HouseNumber = "1"
            };

            // Setup form file
            var formFile = CreateMockFormFile("test.jpg", "image/jpeg");
            
            // Setup form collection with file
            var formCollection = new FormCollection(
                new Dictionary<string, StringValues>(),
                new FormFileCollection { formFile.Object }
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Form = formCollection;

            // Setup user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims));

            // Create the error response
            var errorResponse = ApiResponse<HumanInformation>.BadRequest("User already has associated human information");

            // Setup service mock
            _serviceMock
                .Setup(x => x.CreateAsync(
                    It.IsAny<CreateHumanInformationDto>(), 
                    It.IsAny<IFormFile>(), 
                    userId))
                .Returns(Task.FromResult(errorResponse));

            // Setup image service mock to process the image
            _imageServiceMock
                .Setup(x => x.ProcessProfileImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 }); // Return some dummy processed image data

            // Act
            var result = await _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
            
            // Verify the response
            Assert.NotNull(badRequestResult.Value);
            var response = badRequestResult.Value as ApiResponse<HumanInformation>;
            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("already has associated", response.Message);
        }

        private Mock<IFormFile> CreateMockFormFile(string fileName, string contentType)
        {
            var content = "fake image content";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.ContentDisposition).Returns($"form-data; name=\"file\"; filename=\"{fileName}\"");

            return fileMock;
        }
    }
}
