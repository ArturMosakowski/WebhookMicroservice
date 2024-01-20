namespace WebhooksMicroservice.Model
{
    public class WebhookEvent
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public List<WebhookUrl> WebhookUrls { get; set; }
    }
}
