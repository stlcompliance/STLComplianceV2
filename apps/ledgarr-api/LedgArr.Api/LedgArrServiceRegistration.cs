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
        builder.Services.Configure<StaffArrPayrollClientOptions>(builder.Configuration.GetSection(StaffArrPayrollClientOptions.SectionName));
        builder.Services.AddHttpClient<PayrollIntegrationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrPayrollClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<LedgArrStore>();
        builder.Services.AddScoped<LedgArrTenantSettingsService>();
        builder.Services.AddScoped<PayrollService>();
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
