using MaintainArr.Api;
using MaintainArr.Api.Data;
using MaintainArr.Api.Endpoints;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<MaintainArrDbContext>(
    new ProductDescriptor("maintainarr", "MaintainArr", 5104),
    args,
    MaintainArrServiceRegistration.ConfigureServices,
    MaintainArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapMaintainArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapMaintainArrSettingsEndpoints();
        app.MapMaintainArrAssetClassEndpoints();
        app.MapMaintainArrAssetTypeEndpoints();
        app.MapMaintainArrAssetEndpoints();
        app.MapMaintainArrPreventiveMaintenanceEndpoints();
        app.MapMaintainArrPmProgramEndpoints();
        app.MapMaintainArrInspectionTemplateEndpoints();
        app.MapMaintainArrInspectionEndpoints();
        app.MapMaintainArrDefectEndpoints();
        app.MapMaintainArrDefectEvidenceEndpoints();
        app.MapMaintainArrDocumentEndpoints();
        app.MapMaintainArrWorkOrderEndpoints();
        app.MapMaintainArrTechnicianRefEndpoints();
        app.MapMaintainArrWorkOrderLaborEvidenceEndpoints();
        app.MapMaintainArrWorkOrderPartsDemandEndpoints();
        app.MapMaintainArrWorkOrderSupplyReadinessEndpoints();
        app.MapMaintainArrIntegrationEndpoints();
        app.MapMaintainArrMeterEndpoints();
        app.MapMaintainArrMaintenanceHistoryEndpoints();
        app.MapMaintainArrAssetReadinessEndpoints();
        app.MapMaintainArrEventAndAuditEndpoints();
        app.MapMaintainArrFieldInboxEndpoints();
        app.MapMaintainArrInternalPmDueScanEndpoints();
        app.MapMaintainArrInternalTechnicianRefSyncEndpoints();
        app.MapMaintainArrPmDueScanSettingsEndpoints();
        app.MapMaintainArrNotificationSettingsEndpoints();
        app.MapMaintainArrInternalMaintenanceNotificationEndpoints();
        app.MapMaintainArrAuditPackageEndpoints();
        app.MapMaintainArrInternalAuditPackageGenerationEndpoints();
        app.MapMaintainArrDefectEscalationSettingsEndpoints();
        app.MapMaintainArrInternalDefectEscalationEndpoints();
        app.MapMaintainArrAssetStatusRollupSettingsEndpoints();
        app.MapMaintainArrAssetStatusRollupEndpoints();
        app.MapMaintainArrInternalAssetStatusRollupEndpoints();
        app.MapMaintainArrDowntimeTrackingSettingsEndpoints();
        app.MapMaintainArrAssetDowntimeEndpoints();
        app.MapMaintainArrInternalAssetDowntimeSyncEndpoints();
        app.MapMaintainArrMaintenancePlatformEventSettingsEndpoints();
        app.MapMaintainArrInternalMaintenancePlatformEventEndpoints();
        app.MapMaintainArrMaintenanceHistoryRollupSettingsEndpoints();
        app.MapMaintainArrInternalMaintenanceHistoryRollupEndpoints();
        app.MapMaintainArrMaintenanceReportEndpoints();
        app.MapMaintainArrExecutiveReportEndpoints();
        app.MapMaintainArrComplianceReportEndpoints();
        app.MapMaintainArrReportIndexEndpoints();
        app.MapMaintainArrAssetImportEndpoints();
        app.MapMaintainArrEntityExportEndpoints();
        await Task.CompletedTask;
    });
