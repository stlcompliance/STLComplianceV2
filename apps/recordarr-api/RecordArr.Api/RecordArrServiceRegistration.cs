using RecordArr.Api.Data;
using STLCompliance.Shared.Integration;

namespace RecordArr.Api;

public static class RecordArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<RecordArrStore>();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        var frontendOrigin = builder.Configuration["Cors:RecordArrFrontendOrigin"] ?? "http://localhost:5184";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("RecordArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin, "http://127.0.0.1:5184")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("RecordArrFrontend");
    }
}
