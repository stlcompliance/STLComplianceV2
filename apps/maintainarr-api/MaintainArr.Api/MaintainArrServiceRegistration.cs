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
        builder.Services.AddScoped<AssetInstalledComponentService>();
        builder.Services.AddScoped<AssetTelematicsIngestionService>();
        builder.Services.AddScoped<CatalogService>();
        builder.Services.AddScoped<CatalogSeedService>();
        builder.Services.AddScoped<FieldsetService>();
        builder.Services.AddScoped<PendingCatalogValueService>();
        builder.Services.AddScoped<ControlledValueValidationService>();
        builder.Services.AddScoped<ComplianceCoreReferenceAdapter>();
        builder.Services.AddScoped<StaffArrReferenceAdapter>();
        builder.Services.AddScoped<SupplyArrReferenceAdapter>();
        builder.Services.AddScoped<StaffArrSiteReferenceService>();
        builder.Services.AddScoped<IExternalReferenceAdapter>(sp => sp.GetRequiredService<ComplianceCoreReferenceAdapter>());
        builder.Services.AddScoped<IExternalReferenceAdapter>(sp => sp.GetRequiredService<StaffArrReferenceAdapter>());
        builder.Services.AddScoped<IExternalReferenceAdapter>(sp => sp.GetRequiredService<SupplyArrReferenceAdapter>());
        builder.Services.AddScoped<PmScheduleService>();
        builder.Services.AddScoped<PmOccurrenceService>();
        builder.Services.AddScoped<PmProgramService>();
        builder.Services.AddScoped<PmDueScanService>();
        builder.Services.AddScoped<PmDueScanSettingsService>();
        builder.Services.AddScoped<InspectionTemplateService>();
        builder.Services.AddScoped<InspectionRunService>();
        builder.Services.AddScoped<InspectionVoiceGuidanceService>();
        builder.Services.AddScoped<DefectService>();
        builder.Services.AddScoped<DefectEvidenceService>();
        builder.Services.AddScoped<DocumentAlertService>();
        builder.Services.AddScoped<AssetMeterService>();
        builder.Services.AddScoped<MeterReadingService>();
        builder.Services.AddScoped<MeterPmForecastService>();
        builder.Services.AddScoped<WorkOrderService>();
        builder.Services.AddScoped<WorkOrderDiscussionService>();
        builder.Services.AddScoped<TechnicianRefService>();
        builder.Services.AddScoped<StaffarrPersonSyncIngestionService>();
        builder.Services.AddScoped<RoutarrEventIngestionService>();
        builder.Services.AddScoped<TechnicianRefSyncService>();
        builder.Services.Configure<StaffArrClientOptions>(builder.Configuration.GetSection(StaffArrClientOptions.SectionName));
        builder.Services.AddHttpClient<StaffArrPersonLookupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<StaffArrSiteLookupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.Configure<TrainArrClientOptions>(builder.Configuration.GetSection(TrainArrClientOptions.SectionName));
        builder.Services.AddHttpClient<TrainArrQualificationCheckClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TrainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.Configure<ComplianceCoreClientOptions>(builder.Configuration.GetSection(ComplianceCoreClientOptions.SectionName));
        builder.Services.AddHttpClient<ComplianceCoreWorkOrderGateClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<ComplianceCoreAssetReadinessGateClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<WorkOrderLaborEvidenceService>();
        builder.Services.AddScoped<WorkOrderPartsDemandService>();
        builder.Services.AddScoped<WorkOrderPartsDemandStatusIngestionService>();
        builder.Services.AddScoped<AssetQualityHoldService>();
        builder.Services.AddScoped<AssetReadinessCheckService>();
        builder.Services.AddScoped<MaintenancePartsKitService>();
        builder.Services.AddScoped<MaintenanceVendorWorkService>();
        builder.Services.Configure<SupplyArrClientOptions>(builder.Configuration.GetSection(SupplyArrClientOptions.SectionName));
        builder.Services.AddHttpClient<SupplyArrDemandClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<SupplyArrSupplyReadinessClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<WorkOrderSupplyReadinessService>();
        builder.Services.Configure<EvidenceStorageOptions>(builder.Configuration.GetSection(EvidenceStorageOptions.SectionName));
        builder.Services.AddSingleton<MaintainArrEvidenceStorageService>();
        builder.Services.AddScoped<MaintenanceHistoryService>();
        builder.Services.AddScoped<MaintenanceHistoryRollupSettingsService>();
        builder.Services.AddScoped<MaintenanceHistoryRollupWorkerService>();
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
        builder.Services.AddScoped<DowntimeTrackingSettingsService>();
        builder.Services.AddScoped<AssetDowntimeService>();
        builder.Services.AddScoped<AssetDowntimeSyncWorkerService>();
        builder.Services.AddScoped<MaintenancePlatformEventSettingsService>();
        builder.Services.AddScoped<MaintenancePlatformEventProcessingService>();
        builder.Services.AddScoped<MaintenancePlatformOutboxEnqueueService>();
        builder.Services.AddScoped<MaintenanceReportService>();
        builder.Services.AddScoped<ExecutiveReportService>();
        builder.Services.AddScoped<DashboardService>();
        builder.Services.AddScoped<ComplianceReportService>();
        builder.Services.AddScoped<AssetBulkImportService>();
        builder.Services.AddScoped<EntityBulkExportService>();
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
