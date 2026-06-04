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
        app.MapComplianceCoreRuleTestCaseEndpoints();
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
        app.MapComplianceCoreExceptionExemptionEndpoints();
        app.MapComplianceCoreWorkflowGateEndpoints();
        app.MapComplianceCoreProductGateEndpoints();
        app.MapComplianceCoreProductGateResponseEndpoints();
        app.MapComplianceCoreAuditRequirementEndpoints();
        app.MapComplianceCoreCsvImportExportEndpoints();
        app.MapComplianceCoreStagedImportEndpoints();
        app.MapComplianceCoreTheoreticalSituationEndpoints();
        app.MapComplianceCoreAuditPackageEndpoints();
        app.MapComplianceCoreInternalAuditPackageGenerationEndpoints();
        app.MapComplianceCoreInternalWaiverEndpoints();
        app.MapComplianceCoreOperatorDashboardEndpoints();
        app.MapComplianceCoreReportIndexEndpoints();
        app.MapComplianceCoreFindingsReportEndpoints();
        app.MapComplianceCoreOperatorReportEndpoints();
        app.MapComplianceCoreMissingEvidenceReportEndpoints();
        app.MapComplianceCoreEvidenceCompletenessReportEndpoints();
        app.MapComplianceCoreWaiverReportEndpoints();
        app.MapComplianceCoreExceptionExemptionReportEndpoints();
        app.MapComplianceCoreProductIntegrationHealthReportEndpoints();
        app.MapComplianceCoreAuditReadinessReportEndpoints();
        app.MapComplianceCoreRemediationQueueReportEndpoints();
        app.MapComplianceCoreRegulatoryDomainCoverageReportEndpoints();
        app.MapComplianceCoreHazmatTableCoverageReportEndpoints();
        app.MapComplianceCoreTitle49CoverageExplorerEndpoints();
        app.MapComplianceCoreTitle49CitationCoverageReportEndpoints();
        app.MapComplianceCoreCitationReviewReportEndpoints();
        app.MapComplianceCoreRuleChangeImpactReportEndpoints();
        app.MapComplianceCoreEvaluationHistoryExplorerEndpoints();
        app.MapComplianceCoreCalculatorEndpoints();
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
