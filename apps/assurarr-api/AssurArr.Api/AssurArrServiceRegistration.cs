using AssurArr.Api.Data;
using AssurArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;
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

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddScoped<AssurArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<AssurArrQualityService>();

        var frontendOrigin = builder.Configuration["Cors:AssurArrFrontendOrigin"] ?? "http://localhost:5183";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "AssurArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5183");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("AssurArrFrontend");
    }

}
