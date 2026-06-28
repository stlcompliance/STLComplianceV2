using Microsoft.EntityFrameworkCore;
using OrdArr.Api.Data;
using OrdArr.Api.Services;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;

namespace OrdArr.Api;

public static class OrdArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var connectionString = StlDatabaseConnection.Resolve(builder.Configuration);
        if (string.IsNullOrWhiteSpace(connectionString) && builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<OrdArrDbContext>(options =>
                options.UseInMemoryDatabase("ordarr"));
        }
        else if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("OrdArr requires DATABASE_URL or ConnectionStrings:Database outside Testing.");
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddScoped<OrdArrStore>();
        builder.Services.AddScoped<OrdArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();

        var frontendOrigin = builder.Configuration["Cors:OrdArrFrontendOrigin"] ?? "http://localhost:5187";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "OrdArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5187");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("OrdArrFrontend");
    }
}
