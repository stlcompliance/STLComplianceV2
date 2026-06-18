using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;
using LoadArr.Api.Settings;
using LoadArr.Api.Services;

namespace LoadArr.Api;

public static class LoadArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddScoped<LoadArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<FieldInboxService>();
        builder.Services.AddScoped<LoadArrAuthorizationService>();
        builder.Services.AddScoped<LoadArrTenantSettingsDefaults>();
        builder.Services.AddScoped<LoadArrTenantSettingsValidator>();
        builder.Services.AddScoped<LoadArrTenantSettingsService>();

        var frontendOrigin = builder.Configuration["Cors:LoadArrFrontendOrigin"] ?? "http://localhost:5182";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "LoadArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5182");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("LoadArrFrontend");
    }
}
