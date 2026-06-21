using ReportArr.Api.Data;
using ReportArr.Api.Options;
using ReportArr.Api.Services;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Print;

namespace ReportArr.Api;

public static class ReportArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<RecordArrClientOptions>(builder.Configuration.GetSection(RecordArrClientOptions.SectionName));
        builder.Services.AddSingleton<ReportArrStore>();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddScoped<ReportArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHttpClient<ReportArrRecordArchiveClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RecordArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<IPdfRenderer, StlPlainTextPdfRenderer>();
        builder.Services.AddScoped<IPrintTemplateCatalog, ReportArrPrintTemplateCatalog>();
        builder.Services.AddScoped<IPrintableProvider, ReportArrPrintableProvider>();
        builder.Services.AddScoped<IRecordArchiveClient, ReportArrRecordArchiveClient>();

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
