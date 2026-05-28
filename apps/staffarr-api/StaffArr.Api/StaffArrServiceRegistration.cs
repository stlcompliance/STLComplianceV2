using StaffArr.Api.Options;
using StaffArr.Api.Options;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace StaffArr.Api;

public static class StaffArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));
        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));
        builder.Services.Configure<TrainArrClientOptions>(builder.Configuration.GetSection(TrainArrClientOptions.SectionName));
        builder.Services.Configure<SupplyArrClientOptions>(builder.Configuration.GetSection(SupplyArrClientOptions.SectionName));

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddScoped<StaffArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<StaffArrAuthorizationService>();
        builder.Services.AddScoped<PersonProvisioningService>();
        builder.Services.AddScoped<PeopleService>();
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
        builder.Services.AddScoped<RoleTemplateService>();
        builder.Services.AddScoped<CertificationService>();
        builder.Services.AddScoped<ReadinessOverrideService>();
        builder.Services.AddScoped<ReadinessService>();
        builder.Services.AddScoped<ReadinessRollupService>();
        builder.Services.AddScoped<PersonnelHistoryService>();
        builder.Services.AddScoped<PermissionProjectionService>();
        builder.Services.AddScoped<ProcurementApprovalAuthorityService>();
        builder.Services.AddScoped<TrainingBlockerIngestionService>();
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
        builder.Services.AddHttpClient<SupplyArrDemandClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        builder.Services.AddScoped<IncidentService>();
        builder.Services.AddScoped<IncidentSupplyDemandService>();
        builder.Services.AddScoped<IncidentSupplyDemandStatusIngestionService>();
        builder.Services.AddScoped<FieldInboxService>();
        builder.Services.AddScoped<IncidentRoutingService>();
        builder.Services.AddScoped<PersonTimelineService>();
        builder.Services.AddScoped<PersonnelNoteService>();
        builder.Services.AddScoped<PersonnelDocumentService>();
        builder.Services.Configure<DocumentStorageOptions>(builder.Configuration.GetSection(DocumentStorageOptions.SectionName));
        builder.Services.AddSingleton<StaffArrDocumentStorageService>();
        builder.Services.AddScoped<AuditPackageService>();
        builder.Services.AddScoped<AuditPackageGenerationService>();
        builder.Services.AddScoped<IStaffArrAuditService, StaffArrAuditService>();

        var frontendOrigin = builder.Configuration["Cors:StaffArrFrontendOrigin"] ?? "http://localhost:5175";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("StaffArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("StaffArrFrontend");
    }
}
