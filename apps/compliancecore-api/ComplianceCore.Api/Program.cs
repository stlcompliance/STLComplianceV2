using ComplianceCore.Api;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Endpoints;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;

await StlApiHost.RunAsync<ComplianceCoreDbContext>(
    new ProductDescriptor("compliancecore", "Compliance Core", 5107),
    args,
    ComplianceCoreServiceRegistration.ConfigureServices,
    ComplianceCoreServiceRegistration.ConfigurePipeline,
    async app =>
    {
        app.MapComplianceCoreAuthEndpoints();
        app.MapComplianceCoreVocabularyEndpoints();
        app.MapComplianceCoreComplianceKeyEndpoints();
        app.MapComplianceCoreMaterialKeyEndpoints();
        app.MapComplianceCoreRegulatoryRegistryEndpoints();
        app.MapComplianceCoreRulePackEndpoints();
        app.MapComplianceCoreCitationFactEndpoints();
        app.MapComplianceCoreRegulatoryMappingEndpoints();
        app.MapComplianceCoreRuleEvaluationEndpoints();
        app.MapComplianceCoreFactSourceEndpoints();
        app.MapComplianceCoreInternalFactEndpoints();
        app.MapComplianceCoreInternalScheduledEvaluationEndpoints();
        app.MapComplianceCoreFindingEndpoints();
        app.MapComplianceCoreWorkflowGateEndpoints();
        app.MapComplianceCoreCsvImportExportEndpoints();
        app.MapComplianceCoreAuditPackageEndpoints();
        app.MapComplianceCoreInternalAuditPackageGenerationEndpoints();
        app.MapComplianceCoreOperatorDashboardEndpoints();
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
