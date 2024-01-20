using Microsoft.AspNetCore.Mvc;
using Moq;
using WebhooksMicroservice.Conrollers;
using WebhooksMicroservice.Model;
using WebhooksMicroservice.Services;
using Xunit;

namespace WebhooksMicroservice.Tests
{
    public class WebhooksControllerTests
    {
        private readonly WebhooksController _webhooksController;
        private readonly Mock<WebhookService> _mockWebhookService;
        private readonly Mock<ILogger<WebhooksController>> _mockLogger;

        public WebhooksControllerTests()
        {
            _mockWebhookService = new Mock<WebhookService>();
            _mockLogger = new Mock<ILogger<WebhooksController>>();
            _webhooksController = new WebhooksController(_mockWebhookService.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetUrls_ReturnsOkResult()
        {
            // Arrange
            _mockWebhookService.Setup(service => service.GetWebhookUrls())
                .Returns(new List<WebhookUrl>());

            // Act
            var result = _webhooksController.GetUrls();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void AddUrl_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            _webhooksController.ModelState.AddModelError("Url", "Url is required");

            // Act
            var result = _webhooksController.AddUrl(new WebhookUrlDto());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void DeleteUrl_ReturnsOkResult()
        {
            // Arrange
            int urlId = 1;
            _mockWebhookService.Setup(service => service.DeleteWebhookUrl(urlId));

            // Act
            var result = _webhooksController.DeleteUrl(urlId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void GetEvents_ReturnsOkResult()
        {
            // Arrange
            _mockWebhookService.Setup(service => service.GetWebhookEvents())
                .Returns(new List<WebhookEvent>());

            // Act
            var result = _webhooksController.GetEvents();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async Task ProcessEventAsync_ValidData_SuccessfulProcessing()
        {
            // Arrange
            var eventDto = new EventDto { EventType = "OrderPlaced", OrderId = 123 };

            // Act
            var result = await _webhooksController.ProcessEventAsync(eventDto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _mockWebhookService.Verify(x => x.ProcessEventAsync(eventDto.EventType, eventDto.OrderId), Times.Once);
            _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessEventAsync_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var eventDto = new EventDto(); // Invalid data

            // Act
            var result = await _webhooksController.ProcessEventAsync(eventDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid data", badRequestResult.Value);
            _mockWebhookService.Verify(x => x.ProcessEventAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _mockLogger.Verify(x => x.LogError(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessEventAsync_ExceptionThrown_ReturnsInternalServerError()
        {
            // Arrange
            var eventDto = new EventDto { EventType = "OrderPlaced", OrderId = 123 };
            _mockWebhookService.Setup(x => x.ProcessEventAsync(eventDto.EventType, eventDto.OrderId)).Throws(new Exception("Simulated error"));

            // Act
            var result = await _webhooksController.ProcessEventAsync(eventDto);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            _mockLogger.Verify(x => x.LogError(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
