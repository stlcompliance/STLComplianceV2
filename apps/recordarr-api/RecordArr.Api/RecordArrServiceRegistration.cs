using RecordArr.Api.Data;
using RecordArr.Api.Services;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Print;
using RecordArr.Api.Options;

namespace RecordArr.Api;

public static class RecordArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<RecordArrStore>();
        builder.Services.AddSingleton<RecordArrDocumentStorageService>();
        builder.Services.AddSingleton<IPdfRenderer, StlPlainTextPdfRenderer>();
        builder.Services.Configure<DocumentStorageOptions>(builder.Configuration.GetSection(DocumentStorageOptions.SectionName));
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.AddScoped<RecordArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<IPrintTemplateCatalog, RecordArrPrintTemplateCatalog>();
        builder.Services.AddScoped<IPrintableProvider, RecordArrPrintableProvider>();
        builder.Services.AddScoped<IRecordArchiveClient, RecordArrRecordArchiveClient>();

        var frontendOrigin = builder.Configuration["Cors:RecordArrFrontendOrigin"] ?? "http://localhost:5184";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "RecordArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5184");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("RecordArrFrontend");
    }
}
