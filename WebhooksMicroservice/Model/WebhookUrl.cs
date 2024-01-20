namespace WebhooksMicroservice.Model
{
    public class WebhookUrl
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int WebhookEventId { get; set; }
        public WebhookEvent WebhookEvent { get; set; }
    }
}
