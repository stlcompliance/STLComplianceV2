using CustomArr.Api.Data;
using CustomArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Integration;

namespace CustomArr.Api;

public static class CustomArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(StlDatabaseConnection.Resolve(builder.Configuration)))
        {
            builder.Services.AddDbContext<CustomArrDbContext>(options =>
                options.UseInMemoryDatabase("customarr"));
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddSingleton<CustomArrStore>();
        builder.Services.AddScoped<CustomArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();

        var frontendOrigin = builder.Configuration["Cors:CustomArrFrontendOrigin"] ?? "http://localhost:5186";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CustomArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("CustomArrFrontend");
    }
}
