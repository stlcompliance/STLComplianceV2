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
        builder.Services.AddScoped<RecordArrStore>();
        builder.Services.AddSingleton<RecordArrDocumentStorageService>();
        builder.Services.AddSingleton<IPdfRenderer, StlPlainTextPdfRenderer>();
        builder.Services.Configure<DocumentStorageOptions>(builder.Configuration.GetSection(DocumentStorageOptions.SectionName));
        builder.Services.Configure<MalwareScanWorkerOptions>(builder.Configuration.GetSection(MalwareScanWorkerOptions.SectionName));
        builder.Services.Configure<ObjectStoreReconciliationWorkerOptions>(builder.Configuration.GetSection(ObjectStoreReconciliationWorkerOptions.SectionName));
        builder.Services.Configure<SignatureTrustServiceWorkerOptions>(builder.Configuration.GetSection(SignatureTrustServiceWorkerOptions.SectionName));
        builder.Services.Configure<RedactionProviderWorkerOptions>(builder.Configuration.GetSection(RedactionProviderWorkerOptions.SectionName));
        builder.Services.AddSingleton<IRecordArrMalwareScanVerdictProvider, ManifestRecordArrMalwareScanVerdictProvider>();
        builder.Services.AddSingleton<IRecordArrObjectStoreInventoryProvider, ManifestRecordArrObjectStoreInventoryProvider>();
        builder.Services.AddSingleton<IRecordArrSignatureTrustServiceManifestProvider, ManifestRecordArrSignatureTrustServiceManifestProvider>();
        builder.Services.AddSingleton<IRecordArrRedactionProviderManifestProvider, ManifestRecordArrRedactionProviderManifestProvider>();
        builder.Services.AddHostedService<RecordArrMalwareScanWorker>();
        builder.Services.AddHostedService<RecordArrObjectStoreReconciliationWorker>();
        builder.Services.AddHostedService<RecordArrSignatureTrustServiceWorker>();
        builder.Services.AddHostedService<RecordArrRedactionProviderWorker>();
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
