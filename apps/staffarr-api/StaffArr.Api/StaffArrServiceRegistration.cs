using StaffArr.Api.Options;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.SmartImport;

namespace StaffArr.Api;

public static class StaffArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));
        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));
        builder.Services.Configure<TrainArrClientOptions>(builder.Configuration.GetSection(TrainArrClientOptions.SectionName));
        builder.Services.Configure<SupplyArrClientOptions>(builder.Configuration.GetSection(SupplyArrClientOptions.SectionName));
        builder.Services.Configure<MaintainArrClientOptions>(builder.Configuration.GetSection(MaintainArrClientOptions.SectionName));
        builder.Services.Configure<ComplianceCoreClientOptions>(builder.Configuration.GetSection(ComplianceCoreClientOptions.SectionName));

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddScoped<StaffArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<MePortalService>();
        builder.Services.AddScoped<MyTeamService>();
        builder.Services.AddScoped<PersonnelUpdateRequestService>();
        builder.Services.AddScoped<StaffArrAuthorizationService>();
        builder.Services.AddScoped<PersonProvisioningService>();
        builder.Services.AddScoped<StaffArrTenantSettingsService>();
        builder.Services.AddScoped<EmploymentApplicationService>();
        builder.Services.AddScoped<RecruitingService>();
        builder.Services.AddScoped<PeopleService>();
        builder.Services.AddScoped<PersonAccountAccessService>();
        builder.Services.AddScoped<PersonLookupService>();
        builder.Services.AddScoped<PeopleBulkImportService>();
        builder.Services.AddScoped<PeopleExportService>();
        builder.Services.AddScoped<PersonExportPresetService>();
        builder.Services.AddScoped<PersonExportScheduleService>();
        builder.Services.AddScoped<PersonExportDeliveryNotificationService>();
        builder.Services.AddScoped<PersonExportDeliveryService>();

        builder.Services.AddHttpClient(PersonExportDeliveryNotificationService.WebhookHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        builder.Services.AddScoped<ManagerHierarchyService>();
        builder.Services.AddScoped<OrgUnitService>();
        builder.Services.AddScoped<OrgUnitAssignmentService>();
        builder.Services.AddScoped<RoleManagementService>();
        builder.Services.AddScoped<ProductPermissionCatalogService>();
        builder.Services.AddScoped<InternalLocationService>();
        builder.Services.AddScoped<CertificationService>();
        builder.Services.AddScoped<ReadinessOverrideService>();
        builder.Services.AddScoped<ReadinessService>();
        builder.Services.AddScoped<ReadinessRollupService>();
        builder.Services.AddScoped<PersonnelHistoryService>();
        builder.Services.AddScoped<PermissionProjectionService>();
        builder.Services.AddScoped<IntegrationPermissionCheckService>();
        builder.Services.AddScoped<StaffArrEventFeedService>();
        builder.Services.AddScoped<ProcurementApprovalAuthorityService>();
        builder.Services.AddScoped<TrainingBlockerIngestionService>();
        builder.Services.AddScoped<TrainingAcknowledgementIngestionService>();
        builder.Services.AddScoped<TrainingAcknowledgementService>();
        builder.Services.AddScoped<CertificationGrantIngestionService>();
        builder.Services.AddScoped<CertificationLifecycleIngestionService>();
        builder.Services.AddScoped<CertificationExpirationService>();
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddHttpClient<TrainArrIncidentRemediationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TrainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<TrainArrPersonTrainingHistoryClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TrainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<TrainarrPersonTrainingHistoryService>();
        builder.Services.AddScoped<WorkforceOnboardingJourneyService>();
        builder.Services.AddScoped<PersonOffboardingService>();
        builder.Services.AddHttpClient<NexArrPlatformIdentityClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NexArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<NexArrLoginDisableClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NexArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<NexArrLoginEnableClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NexArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<SupplyArrDemandClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<MaintainArrTechnicianRefSyncClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MaintainArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddHttpClient<ComplianceCorePersonReadinessGateClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<StaffArrMaintainArrTechnicianRefSyncService>();
        builder.Services.AddScoped<IncidentService>();
        builder.Services.AddScoped<IncidentSupplyDemandService>();
        builder.Services.AddScoped<IncidentSupplyDemandStatusIngestionService>();
        builder.Services.AddScoped<PerformanceService>();
        builder.Services.AddScoped<BenefitsCompensationService>();
        builder.Services.AddScoped<FieldInboxService>();
        builder.Services.AddScoped<IncidentRoutingService>();
        builder.Services.AddScoped<PersonTimelineService>();
        builder.Services.AddScoped<PersonnelNoteService>();
        builder.Services.AddScoped<PersonnelDocumentService>();
        builder.Services.Configure<DocumentStorageOptions>(builder.Configuration.GetSection(DocumentStorageOptions.SectionName));
        builder.Services.AddSingleton<StaffArrDocumentStorageService>();
        builder.Services.AddScoped<AuditTimelineService>();
        builder.Services.AddScoped<AuditPackageService>();
        builder.Services.AddScoped<AuditPackageGenerationService>();
        builder.Services.AddScoped<StaffArrEntityBulkExportService>();
        builder.Services.AddScoped<StaffArrWorkerAdminService>();
        builder.Services.AddScoped<TimekeepingService>();
        builder.Services.AddScoped<IStaffArrAuditService, StaffArrAuditService>();
        builder.Services.AddScoped<ISmartImportDestinationCommitHandler, StaffArrSmartImportCommitHandler>();

        var frontendOrigin = builder.Configuration["Cors:StaffArrFrontendOrigin"] ?? "http://localhost:5175";
        var publicSiteOrigin = builder.Configuration["Cors:PublicSiteOrigin"] ?? "http://localhost:5173";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "StaffArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5175",
            publicSiteOrigin,
            "http://127.0.0.1:5173");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("StaffArrFrontend");
    }
}
