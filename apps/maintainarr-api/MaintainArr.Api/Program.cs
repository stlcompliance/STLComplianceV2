using MaintainArr.Api;
using MaintainArr.Api.Data;
using MaintainArr.Api.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<MaintainArrDbContext>(
    new ProductDescriptor("maintainarr", "MaintainArr", 5104),
    args,
    MaintainArrServiceRegistration.ConfigureServices,
    MaintainArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapMaintainArrAuthEndpoints();
        app.MapMaintainArrAssetClassEndpoints();
        app.MapMaintainArrAssetTypeEndpoints();
        app.MapMaintainArrAssetEndpoints();
        app.MapMaintainArrPreventiveMaintenanceEndpoints();
        app.MapMaintainArrPmProgramEndpoints();
        app.MapMaintainArrInspectionTemplateEndpoints();
        app.MapMaintainArrInspectionEndpoints();
        app.MapMaintainArrDefectEndpoints();
        app.MapMaintainArrWorkOrderEndpoints();
        app.MapMaintainArrWorkOrderLaborEvidenceEndpoints();
        app.MapMaintainArrWorkOrderPartsDemandEndpoints();
        app.MapMaintainArrIntegrationEndpoints();
        app.MapMaintainArrMeterEndpoints();
        app.MapMaintainArrMaintenanceHistoryEndpoints();
        app.MapMaintainArrAssetReadinessEndpoints();
        app.MapMaintainArrFieldInboxEndpoints();
        app.MapMaintainArrInternalPmDueScanEndpoints();
        app.MapMaintainArrNotificationSettingsEndpoints();
        app.MapMaintainArrInternalMaintenanceNotificationEndpoints();
        await Task.CompletedTask;
    });
