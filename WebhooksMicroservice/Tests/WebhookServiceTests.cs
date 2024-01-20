using Moq;
using Moq.Protected;
using WebhooksMicroservice.Data;
using WebhooksMicroservice.Model;
using WebhooksMicroservice.Services;
using Xunit;

namespace WebhooksMicroservice.Tests
{
    public class WebhookServiceTests
    {
        private readonly WebhookService _webhookService;
        private readonly Mock<WebhookDbContext> _mockDbContext;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<WebhookService>> _mockLogger;

        public WebhookServiceTests()
        {
            _mockDbContext = new Mock<WebhookDbContext>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<WebhookService>>();
            _webhookService = new WebhookService(_mockDbContext.Object, _mockHttpClientFactory.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetWebhookUrls_ReturnsEmptyList()
        {
            // Arrange
            var webhookUrls = new List<WebhookUrl>();
            var mockDbSet = TestUtils.GetMockDbSet(webhookUrls);
            _mockDbContext.Setup(db => db.WebhookUrls).Returns(mockDbSet.Object);

            // Act
            var result = _webhookService.GetWebhookUrls();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void AddWebhookUrl_ValidData_AddsUrlToDbContext()
        {
            // Arrange
            var dto = new WebhookUrlDto { Url = "https://example.com", WebhookEventId = 1 };
            var webhookUrls = new List<WebhookUrl>();
            var mockDbSet = TestUtils.GetMockDbSet(webhookUrls);
            _mockDbContext.Setup(db => db.WebhookUrls).Returns(mockDbSet.Object);

            // Act
            _webhookService.AddWebhookUrl(dto);

            // Assert
            Assert.Single(webhookUrls);
            _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteWebhookUrl_ValidData_RemovesUrlFromDbContext()
        {
            // Arrange
            int urlId = 1;
            var webhookUrls = new List<WebhookUrl>
        {
            new WebhookUrl { Id = urlId, Url = "https://example.com", WebhookEventId = 1 }
        };
            var mockDbSet = TestUtils.GetMockDbSet(webhookUrls);
            _mockDbContext.Setup(db => db.WebhookUrls).Returns(mockDbSet.Object);

            // Act
            _webhookService.DeleteWebhookUrl(urlId);

            // Assert
            Assert.Empty(webhookUrls);
            _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetWebhookEvents_ReturnsEmptyList()
        {
            // Arrange
            var webhookEvents = new List<WebhookEvent>();
            var mockDbSet = TestUtils.GetMockDbSet(webhookEvents);
            _mockDbContext.Setup(db => db.WebhookEvents).Returns(mockDbSet.Object);

            // Act
            var result = _webhookService.GetWebhookEvents();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ProcessEventAsync_SuccessfulProcessing_LogsInformation()
        {
            // Arrange
            var eventType = "OrderPlaced";
            int orderId = 123;

            // Act
            await _webhookService.ProcessEventAsync(eventType, orderId);

            // Assert
            _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessEventAsync_ExceptionThrown_LogsErrorAndThrows()
        {
            // Arrange
            var eventType = "OrderPlaced";
            int orderId = 123;
            _mockDbContext.Setup(db => db.WebhookUrls).Throws(new Exception("Simulated error"));

            // Act/Assert
            await Assert.ThrowsAsync<Exception>(() => _webhookService.ProcessEventAsync(eventType, orderId));
            _mockLogger.Verify(x => x.LogError(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}