using Microsoft.OpenApi.Models;
using WebhooksMicroservice.Data;
using WebhooksMicroservice.Services;
using Microsoft.EntityFrameworkCore;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {

        services.AddDbContext<WebhookDbContext>(options =>
            options.UseInMemoryDatabase("WebhookDb"));

        services.AddHttpClient();

        services.AddScoped<WebhookService>();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Webhooks API", Version = "v1" });
        });
        services.AddMvcCore()
        .AddApiExplorer();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<WebhookDbContext>();
            WebhookDbInitializer.Initialize(dbContext);
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Webhooks API V1");
        });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
