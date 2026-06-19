using ComplianceCore.Api.Options;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.RateLimiting;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.SmartImport;
using System.Threading.RateLimiting;

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
        builder.Services.AddScoped<QuestionnaireService>();
        builder.Services.AddScoped<RulePackService>();
        builder.Services.AddScoped<RegulatoryCitationService>();
        builder.Services.AddScoped<FactDefinitionService>();
        builder.Services.AddScoped<FactRequirementService>();
        builder.Services.AddScoped<RegulatoryMappingService>();
        builder.Services.AddScoped<RuleContentService>();
        builder.Services.AddScoped<RuleCatalogService>();
        builder.Services.AddScoped<RuleTestCaseService>();
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
        builder.Services.AddScoped<ProductIntegrationHealthReportService>();
        builder.Services.AddScoped<AuditReadinessReportService>();
        builder.Services.AddScoped<RemediationQueueReportService>();
        builder.Services.AddScoped<ProductFactMirrorService>();
        builder.Services.AddScoped<ProductFactIngestionService>();
        builder.Services.AddScoped<SourceIngestionService>();
        builder.Services.AddScoped<RuleChangeMonitoringService>();
        builder.Services.AddScoped<RuleChangeImpactReportService>();
        builder.Services.AddScoped<EvaluationHistoryExplorerService>();
        builder.Services.AddScoped<RiskScoringService>();
        builder.Services.AddScoped<MissingEvidenceWarningService>();
        builder.Services.AddScoped<EvidenceCompletenessReportService>();
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
        builder.Services.AddScoped<CsvImportExportService>();
        builder.Services.AddScoped<StagedImportService>();
        builder.Services.AddScoped<TheoreticalSituationService>();
        builder.Services.AddScoped<AuditPackageService>();
        builder.Services.AddScoped<AuditPackageGenerationService>();
        builder.Services.AddScoped<OperatorDashboardService>();
        builder.Services.AddScoped<FindingsReportService>();
        builder.Services.AddScoped<OperatorReportService>();
        builder.Services.AddScoped<MissingEvidenceReportService>();
        builder.Services.AddScoped<WaiverReportService>();
        builder.Services.AddScoped<ExceptionExemptionReportService>();
        builder.Services.AddScoped<RegulatoryDomainCoverageReportService>();
        builder.Services.AddScoped<HazmatTableCoverageReportService>();
        builder.Services.AddScoped<Title49CoverageExplorerService>();
        builder.Services.AddScoped<Title49CitationCoverageReportService>();
        builder.Services.AddScoped<CitationReviewReportService>();
        builder.Services.AddScoped<Title49CalculatorService>();
        builder.Services.AddScoped<ComplianceCoreEntityBulkExportService>();
        builder.Services.AddScoped<IComplianceCoreAuditService, ComplianceCoreAuditService>();
        builder.Services.AddScoped<ISmartImportDestinationCommitHandler, ComplianceCoreSmartImportCommitHandler>();

        var authPermitLimit = builder.Configuration.GetValue("Auth:LoginRateLimitPermitLimit", 100);
        var authWindowSeconds = builder.Configuration.GetValue("Auth:LoginRateLimitWindowSeconds", 60);
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(
                "ComplianceCoreAuthThrottle",
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authPermitLimit,
                        Window = TimeSpan.FromSeconds(authWindowSeconds),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));
        });

        var frontendOrigin = builder.Configuration["Cors:ComplianceCoreFrontendOrigin"] ?? "http://localhost:5177";

        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "ComplianceCoreFrontend",
            frontendOrigin,
            "http://127.0.0.1:5177");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseRateLimiter();
        app.UseCors("ComplianceCoreFrontend");
    }
}
