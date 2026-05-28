using MaintainArr.Api.Options;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace MaintainArr.Api;

public static class MaintainArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));
        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddScoped<MaintainArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<MaintainArrAuthorizationService>();
        builder.Services.AddScoped<AssetClassService>();
        builder.Services.AddScoped<AssetTypeService>();
        builder.Services.AddScoped<AssetService>();
        builder.Services.AddScoped<PmScheduleService>();
        builder.Services.AddScoped<PmProgramService>();
        builder.Services.AddScoped<PmDueScanService>();
        builder.Services.AddScoped<InspectionTemplateService>();
        builder.Services.AddScoped<InspectionRunService>();
        builder.Services.AddScoped<DefectService>();
        builder.Services.AddScoped<AssetMeterService>();
        builder.Services.AddScoped<MeterReadingService>();
        builder.Services.AddScoped<MeterPmForecastService>();
        builder.Services.AddScoped<WorkOrderService>();
        builder.Services.AddScoped<WorkOrderLaborEvidenceService>();
        builder.Services.AddScoped<WorkOrderPartsDemandService>();
        builder.Services.AddScoped<WorkOrderPartsDemandStatusIngestionService>();
        builder.Services.Configure<SupplyArrClientOptions>(builder.Configuration.GetSection(SupplyArrClientOptions.SectionName));
        builder.Services.AddHttpClient<SupplyArrDemandClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.Configure<EvidenceStorageOptions>(builder.Configuration.GetSection(EvidenceStorageOptions.SectionName));
        builder.Services.AddSingleton<MaintainArrEvidenceStorageService>();
        builder.Services.AddScoped<MaintenanceHistoryService>();
        builder.Services.AddScoped<AssetReadinessService>();
        builder.Services.AddScoped<FieldInboxService>();
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddScoped<IMaintainArrAuditService, MaintainArrAuditService>();
        builder.Services.AddScoped<MaintenanceNotificationSettingsService>();
        builder.Services.AddScoped<MaintenanceNotificationEnqueueService>();
        builder.Services.AddScoped<MaintenanceNotificationDispatchService>();
        builder.Services.AddScoped<DefectEscalationSettingsService>();
        builder.Services.AddScoped<DefectEscalationWorkerService>();
        builder.Services.AddScoped<AssetStatusRollupSettingsService>();
        builder.Services.AddScoped<AssetStatusRollupWorkerService>();
        builder.Services.AddScoped<AssetStatusRollupService>();
        builder.Services.AddScoped<AuditPackageService>();
        builder.Services.AddScoped<AuditPackageGenerationService>();
        builder.Services.AddHttpClient(MaintenanceNotificationDispatchService.WebhookHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        var frontendOrigin = builder.Configuration["Cors:MaintainArrFrontendOrigin"] ?? "http://localhost:5178";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("MaintainArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("MaintainArrFrontend");
    }
}
