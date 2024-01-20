using WebhooksMicroservice.Model;

namespace WebhooksMicroservice.Data
{
    public static class WebhookDbInitializer
    {
        public static void Initialize(WebhookDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.WebhookEvents.Any())
            {
                var events = new WebhookEvent[]
                {
                new WebhookEvent { EventType = "OrderPlaced" },
                new WebhookEvent { EventType = "OrderPaid" },
                new WebhookEvent { EventType = "OrderCancelled" },
                };

                context.WebhookEvents.AddRange(events);
                context.SaveChanges();
            }

            if (!context.WebhookUrls.Any())
            {
                var urls = new WebhookUrl[]
                {
                new WebhookUrl { Url = "https://example.com/webhook1", WebhookEventId = 1 },
                new WebhookUrl { Url = "https://example.com/webhook2", WebhookEventId = 2 },
                new WebhookUrl { Url = "https://example.com/webhook3", WebhookEventId = 3 },
                };

                context.WebhookUrls.AddRange(urls);
                context.SaveChanges();
            }
        }
    }
}
