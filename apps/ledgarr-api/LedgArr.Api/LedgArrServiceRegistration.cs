using LedgArr.Api.Data;
using LedgArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;

namespace LedgArr.Api;

public static class LedgArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(StlDatabaseConnection.Resolve(builder.Configuration)))
        {
            builder.Services.AddDbContext<LedgArrDbContext>(options =>
                options.UseInMemoryDatabase("ledgarr"));
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddScoped<LedgArrStore>();
        builder.Services.AddScoped<LedgArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();

        var frontendOrigin = builder.Configuration["Cors:LedgArrFrontendOrigin"] ?? "http://localhost:5188";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "LedgArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5188");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("LedgArrFrontend");
    }
}
