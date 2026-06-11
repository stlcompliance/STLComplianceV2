using RoutArr.Api;
using RoutArr.Api.Data;
using RoutArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<RoutArrDbContext>(
    new ProductDescriptor("routarr", "RoutArr", 5105),
    args,
    RoutArrServiceRegistration.ConfigureServices,
    RoutArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapRoutArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapStlProductAiAssistanceEndpoints();
        app.MapRoutArrSettingsEndpoints();
        app.MapRoutArrTripEndpoints();
        app.MapRoutArrTripProofDvirEndpoints();
        app.MapRoutArrDispatchReportEndpoints();
        app.MapRoutArrRouteReportEndpoints();
        app.MapRoutArrDispatchOverrideReportEndpoints();
        app.MapRoutArrTripCaptureAttachmentEndpoints();
        app.MapGroup("/api/driver-portal").MapRoutArrDriverPortalTimeTrackingEndpoints();
        app.MapRoutArrTripPartsDemandEndpoints();
        app.MapRoutArrIntegrationEndpoints();
        app.MapRoutArrIntegrationValidationEndpoints();
        app.MapRoutArrIntegrationResourceEndpoints();
        app.MapRoutArrLoadVisibilityEndpoints();
        app.MapRoutArrDockAppointmentEndpoints();
        app.MapRoutArrRouteEndpoints();
        app.MapRoutArrDispatchEndpoints();
        app.MapRoutArrDispatchMessageEndpoints();
        app.MapRoutArrDriverEndpoints();
        app.MapRoutArrVehicleRefEndpoints();
        app.MapRoutArrDriverPortalEndpoints();
        app.MapRoutArrEntityExportEndpoints();
        app.MapRoutArrAuditPackageEndpoints();
        app.MapRoutArrEventAndAuditEndpoints();
        app.MapRoutArrV1FeatureAliasEndpoints();
        app.MapRoutArrInternalAuditPackageGenerationEndpoints();
        app.MapRoutArrDriverAvailabilityEndpoints();
        app.MapRoutArrEquipmentAvailabilityEndpoints();
        app.MapRoutArrDriverEligibilityEndpoints();
        app.MapRoutArrAssetDispatchabilityEndpoints();
        app.MapRoutArrDispatchWorkflowGateEndpoints();
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
        {
            app.MapRoutArrLoadTestJourneySeedEndpoints();
        }
        app.MapRoutArrFieldInboxEndpoints();
        app.MapRoutArrNotificationSettingsEndpoints();
        app.MapRoutArrIntegrationEventSettingsEndpoints();
        app.MapRoutArrTripExecutionCaptureEndpoints();
        app.MapRoutArrInternalDispatchNotificationEndpoints();
        app.MapRoutArrInternalIntegrationEventEndpoints();
        app.MapRoutArrTripCompletionRollupSettingsEndpoints();
        app.MapRoutArrInternalTripCompletionRollupEndpoints();
        app.MapRoutArrTripCompletionEndpoints();
        app.MapRoutArrAttachmentRetentionSettingsEndpoints();
        app.MapRoutArrInternalAttachmentRetentionEndpoints();
        app.MapStlSmartImportAdapterEndpoints();
        await Task.CompletedTask;
    });
