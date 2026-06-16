using Microsoft.EntityFrameworkCore;
using OrdArr.Api.Data;
using OrdArr.Api.Services;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Integration;

namespace OrdArr.Api;

public static class OrdArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(StlDatabaseConnection.Resolve(builder.Configuration)))
        {
            builder.Services.AddDbContext<OrdArrDbContext>(options =>
                options.UseInMemoryDatabase("ordarr"));
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddSingleton<OrdArrStore>();
        builder.Services.AddScoped<OrdArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();

        var frontendOrigin = builder.Configuration["Cors:OrdArrFrontendOrigin"] ?? "http://localhost:5187";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("OrdArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("OrdArrFrontend");
    }
}
