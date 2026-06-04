using AssurArr.Api.Data;
using AssurArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Integration;

namespace AssurArr.Api;

public static class AssurArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(StlDatabaseConnection.Resolve(builder.Configuration)))
        {
            builder.Services.AddDbContext<AssurArrDbContext>(options =>
                options.UseInMemoryDatabase("assurarr"));
        }

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddScoped<AssurArrQualityService>();

        var frontendOrigin = builder.Configuration["Cors:AssurArrFrontendOrigin"] ?? "http://localhost:5183";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AssurArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("AssurArrFrontend");
    }

    public static async Task SeedAsync(WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<AssurArrQualityService>();
        await service.EnsureDemoDataAsync();
    }
}
