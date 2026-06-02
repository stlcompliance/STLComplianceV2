using ComplianceCore.Api.Options;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace ComplianceCore.Api;

public static class ComplianceCoreServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));
        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.Configure<ProductApiIntegrationOptions>(builder.Configuration.GetSection(ProductApiIntegrationOptions.SectionName));
        builder.Services.Configure<StaffArrClientOptions>(builder.Configuration.GetSection(StaffArrClientOptions.SectionName));
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.AddHttpClient(nameof(ProductFactApiFetcher));
        builder.Services.AddHttpClient<StaffArrSiteLookupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddScoped<ComplianceCoreTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<ComplianceCoreAuthorizationService>();
        builder.Services.AddScoped<VocabularyService>();
        builder.Services.AddScoped<ComplianceKeyService>();
        builder.Services.AddScoped<MaterialKeyService>();
        builder.Services.AddScoped<SdsReferenceService>();
        builder.Services.AddScoped<StaffArrSiteReferenceService>();
        builder.Services.AddScoped<HazComReferenceService>();
        builder.Services.AddScoped<RuleVersionService>();
        builder.Services.AddScoped<GoverningBodyService>();
        builder.Services.AddScoped<JurisdictionService>();
        builder.Services.AddScoped<RegulatoryProgramService>();
        builder.Services.AddScoped<RulePackService>();
        builder.Services.AddScoped<RegulatoryCitationService>();
        builder.Services.AddScoped<FactDefinitionService>();
        builder.Services.AddScoped<FactRequirementService>();
        builder.Services.AddScoped<RegulatoryMappingService>();
        builder.Services.AddScoped<RuleContentService>();
        builder.Services.AddScoped<RuleCatalogService>();
        builder.Services.AddScoped<ComplianceFindingService>();
        builder.Services.AddScoped<ComplianceWaiverService>();
        builder.Services.AddScoped<ComplianceExceptionExemptionService>();
        builder.Services.AddScoped<RuleEvaluationService>();
        builder.Services.AddScoped<RulePackBatchEvaluationService>();
        builder.Services.AddScoped<FactSourceService>();
        builder.Services.AddScoped<FactSourceSyncCacheService>();
        builder.Services.AddScoped<ProductFactApiFetcher>();
        builder.Services.AddScoped<FactSourceSyncWorkerSettingsService>();
        builder.Services.AddScoped<FactSourceSyncWorkerService>();
        builder.Services.AddScoped<FactSourceSyncHealthService>();
        builder.Services.AddScoped<ProductFactMirrorService>();
        builder.Services.AddScoped<ProductFactIngestionService>();
        builder.Services.AddScoped<SourceIngestionService>();
        builder.Services.AddScoped<RuleChangeMonitoringService>();
        builder.Services.AddScoped<RiskScoringService>();
        builder.Services.AddScoped<MissingEvidenceWarningService>();
        builder.Services.AddScoped<ControlEffectivenessService>();
        builder.Services.AddScoped<ReadinessForecastService>();
        builder.Services.AddScoped<M12AnalyticsWorkerSettingsService>();
        builder.Services.AddScoped<M12AnalyticsBatchWorkerService>();
        builder.Services.AddScoped<AuditDeliveryOrchestrationService>();
        builder.Services.AddScoped<FactResolveService>();
        builder.Services.AddScoped<InternalRuleEvaluationService>();
        builder.Services.AddScoped<ScheduledRuleEvaluationService>();
        builder.Services.AddScoped<WorkflowGateService>();
        builder.Services.AddScoped<ProductGateEvaluationService>();
        builder.Services.AddScoped<ProductGateResponseService>();
        builder.Services.AddScoped<ProductGateEventService>();
        builder.Services.AddScoped<AuditRequirementService>();
        builder.Services.AddScoped<DispatchWorkflowGateSeedService>();
        builder.Services.AddScoped<LoadTestJourneySeedService>();
        builder.Services.AddScoped<CsvImportExportService>();
        builder.Services.AddScoped<StagedImportService>();
        builder.Services.AddScoped<TheoreticalSituationService>();
        builder.Services.AddScoped<AuditPackageService>();
        builder.Services.AddScoped<AuditPackageGenerationService>();
        builder.Services.AddScoped<OperatorDashboardService>();
        builder.Services.AddScoped<FindingsReportService>();
        builder.Services.AddScoped<OperatorReportService>();
        builder.Services.AddScoped<MissingEvidenceReportService>();
        builder.Services.AddScoped<ComplianceCoreEntityBulkExportService>();
        builder.Services.AddScoped<IComplianceCoreAuditService, ComplianceCoreAuditService>();

        var frontendOrigin = builder.Configuration["Cors:ComplianceCoreFrontendOrigin"] ?? "http://localhost:5177";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ComplianceCoreFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("ComplianceCoreFrontend");
    }
}
