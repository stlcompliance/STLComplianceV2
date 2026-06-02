using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace LoadArr.Api;

public static class LoadArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        var frontendOrigin = builder.Configuration["Cors:LoadArrFrontendOrigin"] ?? "http://localhost:5182";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("LoadArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("LoadArrFrontend");
    }
}
