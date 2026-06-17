using ReportArr.Api.Data;
using ReportArr.Api.Services;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;

namespace ReportArr.Api;

public static class ReportArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ReportArrStore>();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddScoped<ReportArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();

        var frontendOrigin = builder.Configuration["Cors:ReportArrFrontendOrigin"] ?? "http://localhost:5185";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "ReportArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5185");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("ReportArrFrontend");
    }
}
