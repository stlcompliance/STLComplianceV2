using MaintainArr.Api;
using MaintainArr.Api.Data;
using MaintainArr.Api.Endpoints;
using MaintainArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<MaintainArrDbContext>(
    new ProductDescriptor("maintainarr", "MaintainArr", 5104),
    args,
    MaintainArrServiceRegistration.ConfigureServices,
    MaintainArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        await SeedCatalogsAsync(app);

        app.MapMaintainArrAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapMaintainArrSettingsEndpoints();
        app.MapMaintainArrAssetClassEndpoints();
        app.MapMaintainArrAssetTypeEndpoints();
        app.MapMaintainArrAssetEndpoints();
        app.MapMaintainArrAssetComponentEndpoints();
        app.MapMaintainArrCatalogEndpoints();
        app.MapMaintainArrFieldsetEndpoints();
        app.MapMaintainArrReferenceEndpoints();
        app.MapMaintainArrExternalIntelligenceEndpoints();
        app.MapMaintainArrRecallEndpoints();
        app.MapMaintainArrPreventiveMaintenanceEndpoints();
        app.MapMaintainArrPmProgramEndpoints();
        app.MapMaintainArrInspectionTemplateEndpoints();
        app.MapMaintainArrInspectionEndpoints();
        app.MapMaintainArrDefectEndpoints();
        app.MapMaintainArrDefectEvidenceEndpoints();
        app.MapMaintainArrDocumentEndpoints();
        app.MapMaintainArrWorkOrderEndpoints();
        app.MapMaintainArrTechnicianRefEndpoints();
        app.MapMaintainArrWorkOrderDiscussionEndpoints();
        app.MapMaintainArrWorkOrderLaborEvidenceEndpoints();
        app.MapMaintainArrWorkOrderPartsDemandEndpoints();
        app.MapMaintainArrMaintenanceVendorWorkEndpoints();
        app.MapMaintainArrWorkOrderSupplyReadinessEndpoints();
        app.MapMaintainArrMaintenancePartsKitEndpoints();
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
        app.MapMaintainArrDashboardEndpoints();
        app.MapMaintainArrAssetImportEndpoints();
        app.MapMaintainArrEntityExportEndpoints();
        await Task.CompletedTask;
    });

static async Task SeedCatalogsAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();

    string? providerName;
    try
    {
        providerName = db.Database.ProviderName;
    }
    catch (InvalidOperationException)
    {
        app.Logger.LogWarning(
            "Skipping MaintainArr catalog/fieldset seed: database provider is not configured.");
        return;
    }

    if (string.IsNullOrWhiteSpace(providerName))
    {
        app.Logger.LogWarning(
            "Skipping MaintainArr catalog/fieldset seed: database provider is unavailable.");
        return;
    }

    if (db.Database.IsRelational())
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (!canConnect)
        {
            app.Logger.LogWarning(
                "Skipping MaintainArr catalog/fieldset seed: relational provider is configured but not reachable.");
            return;
        }
    }

    var seeder = scope.ServiceProvider.GetRequiredService<CatalogSeedService>();
    await seeder.SeedDefaultsAsync();
}
