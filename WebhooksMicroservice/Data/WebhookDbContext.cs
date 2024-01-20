using Microsoft.EntityFrameworkCore;
using WebhooksMicroservice.Model;

namespace WebhooksMicroservice.Data
{
    public class WebhookDbContext : DbContext
    {
        public DbSet<WebhookEvent> WebhookEvents { get; set; }
        public DbSet<WebhookUrl> WebhookUrls { get; set; }

        public WebhookDbContext(DbContextOptions<WebhookDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebhookUrl>()
                .HasOne(w => w.WebhookEvent)
                .WithMany(e => e.WebhookUrls)
                .HasForeignKey(w => w.WebhookEventId);
        }
    }
}
