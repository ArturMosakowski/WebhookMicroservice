using Microsoft.AspNetCore.Mvc;
using WebhooksMicroservice.Model;
using WebhooksMicroservice.Services;

namespace WebhooksMicroservice.Conrollers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly WebhookService _webhookService;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(WebhookService webhookService, ILogger<WebhooksController> logger)
        {
            _webhookService = webhookService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUrls()
        {
            var urls = _webhookService.GetWebhookUrls();
            return Ok(urls);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddUrl([FromBody] WebhookUrlDto webhookUrlDto)
        {
            if (webhookUrlDto == null || string.IsNullOrWhiteSpace(webhookUrlDto.Url))
            {
                return BadRequest("Invalid data");
            }

            _webhookService.AddWebhookUrl(webhookUrlDto);
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteUrl(int id)
        {
            _webhookService.DeleteWebhookUrl(id);
            return Ok();
        }

        [HttpGet("events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetEvents()
        {
            var events = _webhookService.GetWebhookEvents();
            return Ok(events);
        }

        [HttpPost("events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ProcessEventAsync([FromBody] EventDto eventDto)
        {
            try
            {
                if (eventDto == null || string.IsNullOrWhiteSpace(eventDto.EventType) || eventDto.OrderId <= 0)
                {
                    _logger.LogError("Invalid data received in the request");
                    return BadRequest("Invalid data");
                }

                await _webhookService.ProcessEventAsync(eventDto.EventType, eventDto.OrderId);
                _logger.LogInformation($"Event processed successfully: {eventDto.EventType}, OrderId: {eventDto.OrderId}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing event: {eventDto?.EventType}, OrderId: {eventDto?.OrderId}. Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
