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
        app.MapRoutArrTripEndpoints();
        app.MapRoutArrTripProofDvirEndpoints();
        app.MapRoutArrTripCaptureAttachmentEndpoints();
        app.MapRoutArrTripPartsDemandEndpoints();
        app.MapRoutArrIntegrationEndpoints();
        app.MapRoutArrRouteEndpoints();
        app.MapRoutArrDispatchEndpoints();
        app.MapRoutArrDriverEndpoints();
        app.MapRoutArrVehicleRefEndpoints();
        app.MapRoutArrDriverPortalEndpoints();
        app.MapRoutArrDispatchReportEndpoints();
        app.MapRoutArrRouteReportEndpoints();
        app.MapRoutArrProofDvirReportEndpoints();
        app.MapRoutArrEntityExportEndpoints();
        app.MapRoutArrAuditPackageEndpoints();
        app.MapRoutArrInternalAuditPackageGenerationEndpoints();
        app.MapRoutArrDriverAvailabilityEndpoints();
        app.MapRoutArrEquipmentAvailabilityEndpoints();
        app.MapRoutArrDriverEligibilityEndpoints();
        app.MapRoutArrAssetDispatchabilityEndpoints();
        app.MapRoutArrDispatchWorkflowGateEndpoints();
        app.MapRoutArrLoadTestJourneySeedEndpoints();
        app.MapRoutArrFieldInboxEndpoints();
        app.MapRoutArrNotificationSettingsEndpoints();
        app.MapRoutArrTripExecutionCaptureEndpoints();
        app.MapRoutArrInternalDispatchNotificationEndpoints();
        app.MapRoutArrTripCompletionRollupSettingsEndpoints();
        app.MapRoutArrInternalTripCompletionRollupEndpoints();
        app.MapRoutArrTripCompletionEndpoints();
        app.MapRoutArrAttachmentRetentionSettingsEndpoints();
        app.MapRoutArrInternalAttachmentRetentionEndpoints();
        await Task.CompletedTask;
    });
