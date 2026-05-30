using ComplianceCore.Api;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Endpoints;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<ComplianceCoreDbContext>(
    new ProductDescriptor("compliancecore", "Compliance Core", 5107),
    args,
    ComplianceCoreServiceRegistration.ConfigureServices,
    ComplianceCoreServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapComplianceCoreAuthEndpoints();
        app.MapStlProductLaunchEndpoints();
        app.MapComplianceCoreSettingsEndpoints();
        app.MapComplianceCoreVocabularyEndpoints();
        app.MapComplianceCoreComplianceKeyEndpoints();
        app.MapComplianceCoreMaterialKeyEndpoints();
        app.MapComplianceCoreSdsEndpoints();
        app.MapComplianceCoreHazComEndpoints();
        app.MapComplianceCoreRuleVersionEndpoints();
        app.MapComplianceCoreRegulatoryRegistryEndpoints();
        app.MapComplianceCoreRulePackEndpoints();
        app.MapComplianceCoreRuleCatalogEndpoints();
        app.MapComplianceCoreCitationFactEndpoints();
        app.MapComplianceCoreRegulatoryMappingEndpoints();
        app.MapComplianceCoreRuleEvaluationEndpoints();
        app.MapComplianceCoreFactSourceEndpoints();
        app.MapComplianceCoreFactSourceSyncWorkerSettingsEndpoints();
        app.MapComplianceCoreFactSourceSyncHealthEndpoints();
        app.MapComplianceCoreInternalFactSourceSyncEndpoints();
        app.MapComplianceCoreInternalFactEndpoints();
        app.MapComplianceCoreProductFactIntegrationEndpoints();
        app.MapComplianceCoreSourceIngestionEndpoints();
        app.MapComplianceCoreRuleChangeMonitoringEndpoints();
        app.MapComplianceCoreInternalRuleChangeMonitoringEndpoints();
        app.MapComplianceCoreRiskScoringEndpoints();
        app.MapComplianceCoreMissingEvidenceWarningEndpoints();
        app.MapComplianceCoreControlEffectivenessEndpoints();
        app.MapComplianceCoreReadinessForecastEndpoints();
        app.MapComplianceCoreM12AnalyticsWorkerSettingsEndpoints();
        app.MapComplianceCoreAuditDeliveryOrchestrationEndpoints();
        app.MapComplianceCoreInternalM12AnalyticsBatchEndpoints();
        app.MapComplianceCoreInternalScheduledEvaluationEndpoints();
        app.MapComplianceCoreFindingEndpoints();
        app.MapComplianceCoreWaiverEndpoints();
        app.MapComplianceCoreWorkflowGateEndpoints();
        app.MapComplianceCoreProductGateEndpoints();
        app.MapComplianceCoreCsvImportExportEndpoints();
        app.MapComplianceCoreAuditPackageEndpoints();
        app.MapComplianceCoreInternalAuditPackageGenerationEndpoints();
        app.MapComplianceCoreInternalWaiverEndpoints();
        app.MapComplianceCoreOperatorDashboardEndpoints();
        app.MapComplianceCoreFindingsReportEndpoints();
        app.MapComplianceCoreOperatorReportEndpoints();
        app.MapComplianceCoreEntityExportEndpoints();
        app.MapComplianceCoreLoadTestJourneySeedEndpoints();

        await SeedVocabularyTypesAsync(app);
    });

static async Task SeedVocabularyTypesAsync(WebApplication app)
{
    var connectionString = StlDatabaseConnection.Resolve(app.Configuration);

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        app.Logger.LogWarning("Skipping vocabulary type seed: no database connection configured.");
        return;
    }

    await using var scope = app.Services.CreateAsyncScope();
    var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
    await vocabularyService.EnsureVocabularyTypesSeededAsync();
}
