using TrainArr.Api.Options;

using TrainArr.Api.Services;

using STLCompliance.Shared.Auth;

using STLCompliance.Shared.Integration;



namespace TrainArr.Api;



public static class TrainArrServiceRegistration

{

    public static void ConfigureServices(WebApplicationBuilder builder)

    {

        builder.Services.Configure<StaffArrClientOptions>(builder.Configuration.GetSection(StaffArrClientOptions.SectionName));

        builder.Services.Configure<ComplianceCoreClientOptions>(builder.Configuration.GetSection(ComplianceCoreClientOptions.SectionName));

        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));

        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));
        builder.Services.Configure<SupplyArrClientOptions>(builder.Configuration.GetSection(SupplyArrClientOptions.SectionName));

        builder.Services.Configure<EvidenceStorageOptions>(builder.Configuration.GetSection(EvidenceStorageOptions.SectionName));

        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));

        builder.Services.AddSingleton<StlServiceTokenValidator>();

        builder.Services.AddHttpClient<StaffArrTrainingBlockerClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddHttpClient<StaffArrTrainingAcknowledgementClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddHttpClient<StaffArrCertificationGrantClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddHttpClient<StaffArrCertificationLifecycleClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddHttpClient<StaffArrPersonLookupClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddHttpClient<StaffArrSiteLookupClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StaffArrClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddHttpClient<ComplianceCoreRuleEvaluationClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddHttpClient<ComplianceCoreCitationClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddHttpClient<ComplianceCoreRulePackClient>((sp, client) =>

        {

            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ComplianceCoreClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");

        });

        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);

        builder.Services.AddHttpClient<SupplyArrDemandClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupplyArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddScoped<TrainArrTokenService>();

        builder.Services.AddScoped<HandoffAuthService>();

        builder.Services.AddScoped<MeService>();

        builder.Services.AddScoped<TrainArrAuthorizationService>();

        builder.Services.AddScoped<CertificationPublicationService>();

        builder.Services.AddScoped<QualificationIssueService>();

        builder.Services.AddScoped<QualificationExpirationService>();

        builder.Services.AddScoped<QualificationCheckService>();

        builder.Services.AddScoped<LoadTestJourneySeedService>();
        builder.Services.AddScoped<RecertificationSettingsService>();
        builder.Services.AddScoped<RecertificationAssignmentService>();
        builder.Services.AddScoped<QualificationRecalculationSettingsService>();
        builder.Services.AddScoped<QualificationRecalculationService>();
        builder.Services.AddScoped<RulePackImpactSettingsService>();
        builder.Services.AddScoped<RulePackImpactWorkerService>();
        builder.Services.AddScoped<EvidenceRetentionSettingsService>();
        builder.Services.AddScoped<EvidenceRetentionWorkerService>();
        builder.Services.AddScoped<OrphanReferenceSettingsService>();
        builder.Services.AddScoped<OrphanReferenceWorkerService>();

        builder.Services.AddScoped<IntegrationSettingsService>();
        builder.Services.AddScoped<IntegrationProbeService>();

        builder.Services.AddHttpClient(IntegrationProbeService.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        builder.Services.AddScoped<StaffArrSiteReferenceService>();
        builder.Services.AddScoped<StaffarrPublicationSettingsService>();
        builder.Services.AddScoped<StaffarrPublicationRetryService>();

        builder.Services.AddScoped<EventProcessingSettingsService>();
        builder.Services.AddScoped<TrainingEventProcessingService>();
        builder.Services.AddScoped<TrainingEventEnqueueService>();
        builder.Services.AddScoped<PersonTrainingHistoryService>();

        builder.Services.AddScoped<TrainingNotificationSettingsService>();
        builder.Services.AddScoped<TrainingNotificationEnqueueService>();
        builder.Services.AddScoped<TrainingNotificationDispatchService>();

        builder.Services.AddScoped<AssignmentDueReminderSettingsService>();
        builder.Services.AddScoped<AssignmentDueReminderWorkerService>();
        builder.Services.AddScoped<AssignmentEscalationSettingsService>();
        builder.Services.AddScoped<AssignmentEscalationWorkerService>();

        builder.Services.AddHttpClient(TrainingNotificationDispatchService.WebhookHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        builder.Services.AddScoped<StaffarrIncidentRemediationService>();

        builder.Services.AddScoped<StaffarrIncidentRemediationQueryService>();

        builder.Services.AddScoped<TrainingDefinitionService>();

        builder.Services.AddScoped<TrainingDefinitionStepService>();

        builder.Services.AddScoped<TrainingDefinitionCompletionRuleService>();
        builder.Services.AddScoped<TrainingDefinitionStepBranchService>();

        builder.Services.AddScoped<TrainingCompletionEvaluationService>();

        builder.Services.AddScoped<TrainingProgramVersionService>();

        builder.Services.AddScoped<TrainingProgramService>();

        builder.Services.AddScoped<TrainingMatrixService>();

        builder.Services.AddScoped<TrainingApplicabilityProfileService>();

        builder.Services.AddScoped<TrainingRequirementService>();

        builder.Services.AddScoped<TrainingAcknowledgementPublicationService>();

        builder.Services.AddScoped<TrainingAssignmentService>();
        builder.Services.AddScoped<TrainingAssignmentMaterialDemandService>();
        builder.Services.AddScoped<TrainingAssignmentMaterialDemandStatusIngestionService>();
        builder.Services.AddScoped<FieldInboxService>();

        builder.Services.AddSingleton<TrainArrEvidenceStorageService>();

        builder.Services.AddScoped<TrainingEvidenceService>();

        builder.Services.AddScoped<TrainingEvaluationService>();

        builder.Services.AddScoped<TrainingSignoffService>();

        builder.Services.AddScoped<TrainingCitationService>();

        builder.Services.AddScoped<TrainingRulePackRequirementService>();

        builder.Services.AddScoped<RulePackImpactService>();

        builder.Services.AddScoped<ITrainArrAuditService, TrainArrAuditService>();

        builder.Services.AddScoped<AuditPackageService>();

        builder.Services.AddScoped<AuditPackageGenerationService>();

        builder.Services.AddScoped<AssignmentReportService>();
        builder.Services.AddScoped<QualificationReportService>();
        builder.Services.AddScoped<ComplianceReportService>();
        builder.Services.AddScoped<ReadinessAlertReportService>();
        builder.Services.AddScoped<TrainArrCommandCenterService>();
        builder.Services.AddScoped<PersonalTrainingDashboardService>();
        builder.Services.AddScoped<TrainArrEntityBulkExportService>();

        var frontendOrigin = builder.Configuration["Cors:TrainArrFrontendOrigin"] ?? "http://localhost:5176";

        builder.Services.AddCors(options =>

        {

            options.AddPolicy("TrainArrFrontend", policy =>

            {

                policy.WithOrigins(frontendOrigin)

                    .AllowAnyHeader()

                    .AllowAnyMethod();

            });

        });

    }



    public static void ConfigurePipeline(WebApplication app)

    {

        app.UseCors("TrainArrFrontend");

    }

}


