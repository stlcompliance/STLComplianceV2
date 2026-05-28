using NexArr.Api;
using NexArr.Api.Data;
using NexArr.Api.Endpoints;
using NexArr.Api.Options;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<NexArrDbContext>(
    new ProductDescriptor("nexarr", "NexArr", 5101),
    args,
    NexArrServiceRegistration.ConfigureServices,
    NexArrServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapAuthEndpoints();
        app.MapTenantEndpoints();
        app.MapProductEndpoints();
        app.MapEntitlementEndpoints();
        app.MapServiceTokenEndpoints();
        app.MapInternalIntegrationTokenEndpoints();
        app.MapLaunchEndpoints();
        app.MapPlatformAdminEndpoints();
        app.MapPlatformAuditPackageEndpoints();
        app.MapNexArrInternalPlatformAuditPackageGenerationEndpoints();
        app.MapPlatformServiceTokenCleanupEndpoints();
        app.MapNexArrInternalServiceTokenCleanupEndpoints();
        app.MapPlatformEntitlementReconciliationEndpoints();
        app.MapNexArrInternalEntitlementReconciliationEndpoints();
        app.MapPlatformHealthEndpoints();
        app.MapCompanionEndpoints();
        app.MapCompanionNotificationEndpoints();
        app.MapCompanionPushEndpoints();
        app.MapCompanionOfflineEndpoints();
        app.MapCompanionFieldEvidenceEndpoints();
        app.MapCompanionFieldSubmissionEndpoints();
        app.MapCompanionScanEndpoints();
        app.MapInternalCompanionNotificationEndpoints();
        await NexArrServiceRegistration.InitializeAsync(app);
    });
