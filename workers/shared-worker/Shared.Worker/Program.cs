using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Jobs;
using Shared.Worker.Options;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Http;
using STLCompliance.Shared.Workers;

await StlWorkerHost.RunAsync(
    new ProductDescriptor("shared-worker", "STL Shared Worker", 0),
    args,
    builder =>
    {
        builder.Services.Configure<TrainArrQualificationExpirationOptions>(
            builder.Configuration.GetSection(TrainArrQualificationExpirationOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrQualificationExpirationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrQualificationExpirationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrQualificationExpirationJob>();

        builder.Services.Configure<TrainArrRecertificationAssignmentOptions>(
            builder.Configuration.GetSection(TrainArrRecertificationAssignmentOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrRecertificationAssignmentClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrRecertificationAssignmentOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrRecertificationAssignmentJob>();

        builder.Services.Configure<TrainArrQualificationRecalculationOptions>(
            builder.Configuration.GetSection(TrainArrQualificationRecalculationOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrQualificationRecalculationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrQualificationRecalculationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrQualificationRecalculationJob>();

        builder.Services.Configure<TrainArrRulePackImpactOptions>(
            builder.Configuration.GetSection(TrainArrRulePackImpactOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrRulePackImpactClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrRulePackImpactOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrRulePackImpactJob>();

        builder.Services.Configure<TrainArrEvidenceRetentionOptions>(
            builder.Configuration.GetSection(TrainArrEvidenceRetentionOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrEvidenceRetentionClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrEvidenceRetentionOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<TrainArrEvidenceRetentionJob>();

        builder.Services.Configure<TrainArrStaffarrPublicationRetryOptions>(
            builder.Configuration.GetSection(TrainArrStaffarrPublicationRetryOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrStaffarrPublicationRetryClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrStaffarrPublicationRetryOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrStaffarrPublicationRetryJob>();

        builder.Services.Configure<TrainArrEventProcessingOptions>(
            builder.Configuration.GetSection(TrainArrEventProcessingOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrEventProcessingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrEventProcessingOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrEventProcessingJob>();

        builder.Services.Configure<TrainArrNotificationDispatchOptions>(
            builder.Configuration.GetSection(TrainArrNotificationDispatchOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrNotificationDispatchClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrNotificationDispatchOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrNotificationDispatchJob>();

        builder.Services.Configure<StaffArrCertificationExpirationOptions>(
            builder.Configuration.GetSection(StaffArrCertificationExpirationOptions.SectionName));

        builder.Services.AddHttpClient<StaffArrCertificationExpirationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<StaffArrCertificationExpirationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.StaffArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<StaffArrCertificationExpirationJob>();

        builder.Services.Configure<ComplianceCoreScheduledEvaluationOptions>(
            builder.Configuration.GetSection(ComplianceCoreScheduledEvaluationOptions.SectionName));

        builder.Services.AddHttpClient<ComplianceCoreScheduledEvaluationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ComplianceCoreScheduledEvaluationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.ComplianceCoreBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<ComplianceCoreScheduledEvaluationJob>();

        builder.Services.Configure<ComplianceCoreAuditPackageGenerationOptions>(
            builder.Configuration.GetSection(ComplianceCoreAuditPackageGenerationOptions.SectionName));

        builder.Services.AddHttpClient<ComplianceCoreAuditPackageGenerationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ComplianceCoreAuditPackageGenerationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.ComplianceCoreBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddHostedService<ComplianceCoreAuditPackageGenerationJob>();

        builder.Services.Configure<StaffArrReadinessRollupOptions>(
            builder.Configuration.GetSection(StaffArrReadinessRollupOptions.SectionName));

        builder.Services.AddHttpClient<StaffArrReadinessRollupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<StaffArrReadinessRollupOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.StaffArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<StaffArrReadinessRollupJob>();

        builder.Services.Configure<StaffArrPermissionProjectionOptions>(
            builder.Configuration.GetSection(StaffArrPermissionProjectionOptions.SectionName));

        builder.Services.AddHttpClient<StaffArrPermissionProjectionClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<StaffArrPermissionProjectionOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.StaffArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<StaffArrPermissionProjectionJob>();

        builder.Services.Configure<StaffArrPersonnelHistoryRollupOptions>(
            builder.Configuration.GetSection(StaffArrPersonnelHistoryRollupOptions.SectionName));

        builder.Services.AddHttpClient<StaffArrPersonnelHistoryRollupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<StaffArrPersonnelHistoryRollupOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.StaffArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<StaffArrPersonnelHistoryRollupJob>();

        builder.Services.Configure<StaffArrPersonExportDeliveryOptions>(
            builder.Configuration.GetSection(StaffArrPersonExportDeliveryOptions.SectionName));

        builder.Services.AddHttpClient<StaffArrPersonExportDeliveryClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<StaffArrPersonExportDeliveryOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.StaffArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<StaffArrPersonExportDeliveryJob>();

        builder.Services.Configure<StaffArrAuditPackageGenerationOptions>(
            builder.Configuration.GetSection(StaffArrAuditPackageGenerationOptions.SectionName));

        builder.Services.AddHttpClient<StaffArrAuditPackageGenerationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<StaffArrAuditPackageGenerationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.StaffArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddHostedService<StaffArrAuditPackageGenerationJob>();

        builder.Services.Configure<MaintainArrPmDueScanOptions>(
            builder.Configuration.GetSection(MaintainArrPmDueScanOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrPmDueScanClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrPmDueScanOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrPmDueScanJob>();

        builder.Services.Configure<MaintainArrNotificationDispatchOptions>(
            builder.Configuration.GetSection(MaintainArrNotificationDispatchOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrNotificationDispatchClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrNotificationDispatchOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrNotificationDispatchJob>();

        builder.Services.Configure<RoutArrNotificationDispatchOptions>(
            builder.Configuration.GetSection(RoutArrNotificationDispatchOptions.SectionName));

        builder.Services.AddHttpClient<RoutArrNotificationDispatchClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RoutArrNotificationDispatchOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.RoutArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<RoutArrNotificationDispatchJob>();

        builder.Services.Configure<SupplyArrReorderEvaluationOptions>(
            builder.Configuration.GetSection(SupplyArrReorderEvaluationOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrReorderEvaluationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrReorderEvaluationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrReorderEvaluationJob>();

        builder.Services.Configure<SupplyArrNotificationDispatchOptions>(
            builder.Configuration.GetSection(SupplyArrNotificationDispatchOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrNotificationDispatchClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrNotificationDispatchOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrNotificationDispatchJob>();

        builder.Services.Configure<NexArrCompanionNotificationDispatchOptions>(
            builder.Configuration.GetSection(NexArrCompanionNotificationDispatchOptions.SectionName));

        builder.Services.AddHttpClient<NexArrCompanionNotificationDispatchClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NexArrCompanionNotificationDispatchOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.NexArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<NexArrCompanionNotificationDispatchJob>();

        builder.Services.Configure<NexArrPlatformAuditPackageGenerationOptions>(
            builder.Configuration.GetSection(NexArrPlatformAuditPackageGenerationOptions.SectionName));

        builder.Services.AddHttpClient<NexArrPlatformAuditPackageGenerationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NexArrPlatformAuditPackageGenerationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.NexArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddHostedService<NexArrPlatformAuditPackageGenerationJob>();

        builder.Services.Configure<MaintainArrAuditPackageGenerationOptions>(
            builder.Configuration.GetSection(MaintainArrAuditPackageGenerationOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrAuditPackageGenerationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrAuditPackageGenerationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddHostedService<MaintainArrAuditPackageGenerationJob>();
    });
