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
        builder.Services.AddSingleton<StlServiceTokenValidator>();

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddScoped<ComplianceCoreTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<ComplianceCoreAuthorizationService>();
        builder.Services.AddScoped<VocabularyService>();
        builder.Services.AddScoped<ComplianceKeyService>();
        builder.Services.AddScoped<MaterialKeyService>();
        builder.Services.AddScoped<GoverningBodyService>();
        builder.Services.AddScoped<JurisdictionService>();
        builder.Services.AddScoped<RegulatoryProgramService>();
        builder.Services.AddScoped<RulePackService>();
        builder.Services.AddScoped<RegulatoryCitationService>();
        builder.Services.AddScoped<FactDefinitionService>();
        builder.Services.AddScoped<FactRequirementService>();
        builder.Services.AddScoped<RegulatoryMappingService>();
        builder.Services.AddScoped<RuleContentService>();
        builder.Services.AddScoped<ComplianceFindingService>();
        builder.Services.AddScoped<RuleEvaluationService>();
        builder.Services.AddScoped<RulePackBatchEvaluationService>();
        builder.Services.AddScoped<FactSourceService>();
        builder.Services.AddScoped<FactResolveService>();
        builder.Services.AddScoped<InternalRuleEvaluationService>();
        builder.Services.AddScoped<ScheduledRuleEvaluationService>();
        builder.Services.AddScoped<WorkflowGateService>();
        builder.Services.AddScoped<DispatchWorkflowGateSeedService>();
        builder.Services.AddScoped<CsvImportExportService>();
        builder.Services.AddScoped<AuditPackageService>();
        builder.Services.AddScoped<OperatorDashboardService>();
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
