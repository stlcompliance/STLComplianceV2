using RoutArr.Api.Options;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.SmartImport;

namespace RoutArr.Api;

public static class RoutArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));
        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));
        builder.Services.Configure<DriverEligibilityOptions>(builder.Configuration.GetSection(DriverEligibilityOptions.SectionName));
        builder.Services.Configure<TrainArrClientOptions>(builder.Configuration.GetSection(TrainArrClientOptions.SectionName));
        builder.Services.Configure<StaffArrClientOptions>(builder.Configuration.GetSection(StaffArrClientOptions.SectionName));
        builder.Services.Configure<AssetDispatchabilityOptions>(builder.Configuration.GetSection(AssetDispatchabilityOptions.SectionName));
        builder.Services.Configure<MaintainArrClientOptions>(builder.Configuration.GetSection(MaintainArrClientOptions.SectionName));
        builder.Services.Configure<SupplyArrClientOptions>(builder.Configuration.GetSection(SupplyArrClientOptions.SectionName));
        builder.Services.Configure<ComplianceCoreClientOptions>(builder.Configuration.GetSection(ComplianceCoreClientOptions.SectionName));
        builder.Services.Configure<DispatchWorkflowGateOptions>(builder.Configuration.GetSection(DispatchWorkflowGateOptions.SectionName));
        builder.Services.Configure<CaptureAttachmentStorageOptions>(builder.Configuration.GetSection(CaptureAttachmentStorageOptions.SectionName));

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddHttpClient<TrainArrQualificationCheckClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TrainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<TrainArrIncidentRemediationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TrainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<StaffArrReadinessClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<StaffArrProductIncidentClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<StaffArrSiteLookupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<MaintainArrAssetReadinessClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaintainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<MaintainArrRoutarrEventClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaintainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<ComplianceCoreWorkflowGateClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<ComplianceCoreProductFactClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddHttpClient<SupplyArrDemandClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<SupplyArrSupplierOrderClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddScoped<RoutArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<RoutArrAuthorizationService>();
        builder.Services.AddScoped<DriverEligibilityService>();
        builder.Services.AddScoped<AssetDispatchabilityService>();
        builder.Services.AddScoped<DispatchWorkflowGateService>();
        builder.Services.AddScoped<StaffArrSiteReferenceService>();
        builder.Services.AddScoped<DispatchAssignmentService>();
        builder.Services.AddScoped<BulkDispatchService>();
        builder.Services.AddScoped<DispatchCloseoutService>();
        builder.Services.AddScoped<DispatchPlanService>();
        builder.Services.AddScoped<TripSupplierReadinessService>();
        builder.Services.AddScoped<TripService>();
        builder.Services.AddScoped<TmsRuntimeService>();
        builder.Services.AddScoped<SupplyArrSupplierOrderEventIngestionService>();
        builder.Services.AddScoped<TripEtaService>();
        builder.Services.AddScoped<TripPartsDemandService>();
        builder.Services.AddScoped<TripPartsDemandStatusIngestionService>();
        builder.Services.AddScoped<SupplyArrShipmentIntentService>();
        builder.Services.AddScoped<LoadVisibilityService>();
        builder.Services.AddScoped<DockAppointmentService>();
        builder.Services.AddScoped<RouteService>();
        builder.Services.AddScoped<DispatchBoardService>();
        builder.Services.AddScoped<DispatchBoardStateService>();
        builder.Services.AddScoped<StaffarrPersonRefService>();
        builder.Services.AddScoped<VehicleRefService>();
        builder.Services.AddScoped<DispatchCommandCenterService>();
        builder.Services.AddScoped<DispatchExceptionService>();
        builder.Services.AddScoped<DispatchMessageService>();
        builder.Services.AddScoped<ActiveTripsService>();
        builder.Services.AddScoped<UnassignedWorkQueueService>();
        builder.Services.AddScoped<DriverPortalService>();
        builder.Services.AddScoped<DriverTimeTrackingService>();
        builder.Services.AddScoped<DispatchReportService>();
        builder.Services.AddScoped<DispatchOverrideReportService>();
        builder.Services.AddScoped<RouteReportService>();
        builder.Services.AddScoped<ProofDvirReportService>();
        builder.Services.AddScoped<RoutArrEntityBulkExportService>();
        builder.Services.AddScoped<AuditPackageService>();
        builder.Services.AddScoped<AuditPackageGenerationService>();
        builder.Services.AddScoped<TripProofDvirService>();
        builder.Services.AddScoped<TripCaptureAttachmentService>();
        builder.Services.AddSingleton<RoutArrCaptureAttachmentStorageService>();
        builder.Services.AddScoped<TripExecutionCaptureService>();
        builder.Services.AddScoped<TripAuditTrailService>();
        builder.Services.AddScoped<RouteCalendarService>();
        builder.Services.AddScoped<DriverAvailabilityService>();
        builder.Services.AddScoped<EquipmentAvailabilityService>();
        builder.Services.AddScoped<FieldInboxService>();
        builder.Services.AddScoped<IRoutArrAuditService, RoutArrAuditService>();
        builder.Services.AddScoped<ISmartImportDestinationCommitHandler, RoutArrSmartImportCommitHandler>();
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddScoped<DispatchNotificationSettingsService>();
        builder.Services.AddScoped<DispatchNotificationEnqueueService>();
        builder.Services.AddScoped<DispatchNotificationDispatchService>();
        builder.Services.AddScoped<IntegrationEventSettingsService>();
        builder.Services.AddScoped<IntegrationOutboxEnqueueService>();
        builder.Services.AddScoped<StaffArrProductIncidentPublisherService>();
        builder.Services.AddScoped<TrainArrIncidentRemediationPublisherService>();
        builder.Services.AddScoped<MaintainArrRoutarrEventPublisherService>();
        builder.Services.AddScoped<ComplianceCoreIncidentFactPublisherService>();
        builder.Services.AddScoped<IntegrationEventProcessingService>();
        builder.Services.AddScoped<TripCompletionRollupSettingsService>();
        builder.Services.AddScoped<TripCompletionRollupWorkerService>();
        builder.Services.AddScoped<TripCompletionService>();
        builder.Services.AddScoped<AttachmentRetentionSettingsService>();
        builder.Services.AddScoped<AttachmentRetentionWorkerService>();
        builder.Services.AddScoped<RoutArrTenantSettingsService>();
        builder.Services.AddHttpClient(DispatchNotificationDispatchService.WebhookHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        var frontendOrigin = builder.Configuration["Cors:RoutArrFrontendOrigin"] ?? "http://localhost:5180";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "RoutArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5180");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("RoutArrFrontend");
    }
}
