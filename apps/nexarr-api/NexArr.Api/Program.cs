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
        app.MapSettingsEndpoints();
        app.MapTenantEndpoints();
        app.MapProductEndpoints();
        app.MapRetiredEntitlementCompatibilityEndpoints();
        app.MapServiceTokenEndpoints();
        app.MapServiceTokenDiscoveryEndpoints();
        app.MapInternalIntegrationTokenEndpoints();
        app.MapLaunchEndpoints();
        app.MapAuditEndpoints();
        app.MapPlatformAdminEndpoints();
        app.MapReferenceDataEndpoints();
        app.MapPlatformAuditPackageEndpoints();
        app.MapNexArrInternalPlatformAuditPackageGenerationEndpoints();
        app.MapPlatformSessionSettingsEndpoints();
        app.MapPlatformServiceTokenCleanupEndpoints();
        app.MapNexArrInternalServiceTokenCleanupEndpoints();
        app.MapPlatformOutboxPublisherEndpoints();
        app.MapNexArrInternalPlatformOutboxPublisherEndpoints();
        app.MapPlatformLaunchDestinationReconciliationEndpoints();
        app.MapNexArrInternalLaunchDestinationReconciliationEndpoints();
        app.MapPlatformTenantLifecycleEndpoints();
        app.MapNexArrInternalTenantLifecycleEndpoints();
        app.MapNexArrInternalPersonLoginDisableEndpoints();
        app.MapNexArrInternalPersonLoginEnableEndpoints();
        app.MapNexArrInternalPlatformIdentityEndpoints();
        app.MapPlatformLifecycleOverviewEndpoints();
        app.MapPlatformWorkerHealthOrchestrationEndpoints();
        app.MapTenantIntegrationEndpoints();
        app.MapHybridDataPlaneEndpoints();
        app.MapPlatformHealthEndpoints();
        app.MapAiAssistanceEndpoints();
        app.MapSmartImportEndpoints();
        app.MapNexArrInternalSmartImportEndpoints();
        app.MapFieldCompanionEndpoints();
        app.MapFieldCompanionNotificationEndpoints();
        app.MapFieldCompanionPushEndpoints();
        app.MapFieldCompanionOfflineEndpoints();
        app.MapFieldCompanionClockEndpoints();
        app.MapFieldCompanionFieldEvidenceEndpoints();
        app.MapFieldCompanionFieldDvirEndpoints();
        app.MapFieldCompanionFieldInspectionEndpoints();
        app.MapFieldCompanionFieldWorkOrderEndpoints();
        app.MapFieldCompanionFieldReceivingEndpoints();
        app.MapFieldCompanionFieldSubmissionEndpoints();
        app.MapFieldCompanionScanEndpoints();
        app.MapInternalFieldCompanionNotificationEndpoints();
        await NexArrServiceRegistration.InitializeAsync(app);
    });
