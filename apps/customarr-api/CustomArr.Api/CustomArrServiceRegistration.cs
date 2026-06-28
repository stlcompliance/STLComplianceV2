using CustomArr.Api.Data;
using CustomArr.Api.Options;
using CustomArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.SmartImport;

namespace CustomArr.Api;

public static class CustomArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var connectionString = StlDatabaseConnection.Resolve(builder.Configuration);
        if (string.IsNullOrWhiteSpace(connectionString) && builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<CustomArrDbContext>(options =>
                options.UseInMemoryDatabase("customarr"));
        }
        else if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("CustomArr requires DATABASE_URL or ConnectionStrings:Database outside Testing.");
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.Configure<OrdArrClientOptions>(builder.Configuration.GetSection(OrdArrClientOptions.SectionName));
        builder.Services.AddScoped<CustomArrStore>();
        builder.Services.AddScoped<CustomArrCrmWorkspaceService>();
        builder.Services.AddScoped<CustomArrTenantSettingsService>();
        builder.Services.AddScoped<CustomArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<ISmartImportDestinationCommitHandler, CustomArrSmartImportCommitHandler>();
        builder.Services.AddHttpClient<OrdArrOrderRequestClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OrdArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        var frontendOrigin = builder.Configuration["Cors:CustomArrFrontendOrigin"] ?? "http://localhost:5186";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "CustomArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5186");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("CustomArrFrontend");
    }
}
