using System.Text;
using WebhooksMicroservice.Data;
using WebhooksMicroservice.Model;
using Newtonsoft.Json;

namespace WebhooksMicroservice.Services
{
    public class WebhookService
    {
        private readonly WebhookDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(WebhookDbContext dbContext, IHttpClientFactory httpClientFactory, ILogger<WebhookService> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IEnumerable<WebhookUrl> GetWebhookUrls()
        {
            return _dbContext.WebhookUrls.ToList();
        }

        public void AddWebhookUrl(WebhookUrlDto webhookUrlDto)
        {
            var webhookUrl = new WebhookUrl
            {
                Url = webhookUrlDto.Url,
                WebhookEventId = webhookUrlDto.WebhookEventId
            };

            _dbContext.WebhookUrls.Add(webhookUrl);
            _dbContext.SaveChanges();
        }

        public void DeleteWebhookUrl(int id)
        {
            var webhookUrl = _dbContext.WebhookUrls.Find(id);
            if (webhookUrl != null)
            {
                _dbContext.WebhookUrls.Remove(webhookUrl);
                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<WebhookEvent> GetWebhookEvents()
        {
            return _dbContext.WebhookEvents.ToList();
        }

        public async Task ProcessEventAsync(string eventType, int orderId)
        {
            try
            {
                _logger.LogInformation($"Processing event: {eventType}, OrderId: {orderId}");

                var eventUrls = _dbContext.WebhookUrls
                    .Where(url => url.WebhookEvent.EventType == eventType)
                    .ToList();

                foreach (var url in eventUrls)
                {
                    await NotifyWebhookAsync(url, eventType, orderId);
                }

                _logger.LogInformation($"Event processed successfully: {eventType}, OrderId: {orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing event: {eventType}, OrderId: {orderId}. Error: {ex.Message}");
                throw;
            }
        }

        private async Task NotifyWebhookAsync(WebhookUrl url, string eventType, int orderId)
        {
            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var requestData = new { EventType = eventType, OrderId = orderId };
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(url.Url, jsonContent);

                    _logger.LogInformation($"Webhook notified - URL: {url.Url}, Response Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error notifying webhook - URL: {url.Url}. Error: {ex.Message}");
            }
        }
    }
}
