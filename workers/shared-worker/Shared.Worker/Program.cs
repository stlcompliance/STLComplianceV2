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

        builder.Services.Configure<TrainArrOrphanReferenceOptions>(
            builder.Configuration.GetSection(TrainArrOrphanReferenceOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrOrphanReferenceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrOrphanReferenceOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<TrainArrOrphanReferenceJob>();

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

        builder.Services.Configure<TrainArrAssignmentDueRemindersOptions>(
            builder.Configuration.GetSection(TrainArrAssignmentDueRemindersOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrAssignmentDueRemindersClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrAssignmentDueRemindersOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrAssignmentDueRemindersJob>();

        builder.Services.Configure<TrainArrAssignmentEscalationOptions>(
            builder.Configuration.GetSection(TrainArrAssignmentEscalationOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrAssignmentEscalationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrAssignmentEscalationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<TrainArrAssignmentEscalationJob>();

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

        builder.Services.Configure<ComplianceCoreRuleChangeMonitorOptions>(
            builder.Configuration.GetSection(ComplianceCoreRuleChangeMonitorOptions.SectionName));

        builder.Services.AddHttpClient<ComplianceCoreRuleChangeMonitorClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ComplianceCoreRuleChangeMonitorOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.ComplianceCoreBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<ComplianceCoreRuleChangeMonitorJob>();

        builder.Services.Configure<ComplianceCoreWaiverExpirationOptions>(
            builder.Configuration.GetSection(ComplianceCoreWaiverExpirationOptions.SectionName));

        builder.Services.AddHttpClient<ComplianceCoreWaiverExpirationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ComplianceCoreWaiverExpirationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.ComplianceCoreBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<ComplianceCoreWaiverExpirationJob>();

        builder.Services.Configure<ComplianceCoreFactSourceSyncOptions>(
            builder.Configuration.GetSection(ComplianceCoreFactSourceSyncOptions.SectionName));

        builder.Services.AddHttpClient<ComplianceCoreFactSourceSyncClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ComplianceCoreFactSourceSyncOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.ComplianceCoreBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<ComplianceCoreFactSourceSyncJob>();

        builder.Services.Configure<ComplianceCoreM12AnalyticsBatchOptions>(
            builder.Configuration.GetSection(ComplianceCoreM12AnalyticsBatchOptions.SectionName));

        builder.Services.AddHttpClient<ComplianceCoreM12AnalyticsBatchClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ComplianceCoreM12AnalyticsBatchOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.ComplianceCoreBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddHostedService<ComplianceCoreM12AnalyticsBatchJob>();

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

        builder.Services.Configure<MaintainArrTechnicianRefRefreshOptions>(
            builder.Configuration.GetSection(MaintainArrTechnicianRefRefreshOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrTechnicianRefRefreshClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrTechnicianRefRefreshOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrTechnicianRefRefreshJob>();

        builder.Services.Configure<RoutArrNotificationDispatchOptions>(
            builder.Configuration.GetSection(RoutArrNotificationDispatchOptions.SectionName));

        builder.Services.AddHttpClient<RoutArrNotificationDispatchClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RoutArrNotificationDispatchOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.RoutArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<RoutArrNotificationDispatchJob>();

        builder.Services.Configure<RoutArrTripCompletionRollupOptions>(
            builder.Configuration.GetSection(RoutArrTripCompletionRollupOptions.SectionName));

        builder.Services.AddHttpClient<RoutArrTripCompletionRollupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RoutArrTripCompletionRollupOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.RoutArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<RoutArrTripCompletionRollupJob>();

        builder.Services.Configure<RoutArrAttachmentRetentionOptions>(
            builder.Configuration.GetSection(RoutArrAttachmentRetentionOptions.SectionName));

        builder.Services.AddHttpClient<RoutArrAttachmentRetentionClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RoutArrAttachmentRetentionOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.RoutArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<RoutArrAttachmentRetentionJob>();

        builder.Services.Configure<RoutArrIntegrationEventsOptions>(
            builder.Configuration.GetSection(RoutArrIntegrationEventsOptions.SectionName));

        builder.Services.AddHttpClient<RoutArrIntegrationEventsClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RoutArrIntegrationEventsOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.RoutArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<RoutArrIntegrationEventsJob>();

        builder.Services.Configure<SupplyArrReorderEvaluationOptions>(
            builder.Configuration.GetSection(SupplyArrReorderEvaluationOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrReorderEvaluationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrReorderEvaluationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrReorderEvaluationJob>();

        builder.Services.Configure<SupplyArrPriceSnapshotOptions>(
            builder.Configuration.GetSection(SupplyArrPriceSnapshotOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrPriceSnapshotClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrPriceSnapshotOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrPriceSnapshotJob>();

        builder.Services.Configure<SupplyArrLeadTimeSnapshotOptions>(
            builder.Configuration.GetSection(SupplyArrLeadTimeSnapshotOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrLeadTimeSnapshotClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrLeadTimeSnapshotOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrLeadTimeSnapshotJob>();

        builder.Services.Configure<SupplyArrAvailabilitySnapshotOptions>(
            builder.Configuration.GetSection(SupplyArrAvailabilitySnapshotOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrAvailabilitySnapshotClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrAvailabilitySnapshotOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrAvailabilitySnapshotJob>();

        builder.Services.Configure<SupplyArrProcurementCoordinationOptions>(
            builder.Configuration.GetSection(SupplyArrProcurementCoordinationOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrProcurementCoordinationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrProcurementCoordinationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrProcurementCoordinationJob>();

        builder.Services.Configure<SupplyArrApprovalRemindersOptions>(
            builder.Configuration.GetSection(SupplyArrApprovalRemindersOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrApprovalRemindersClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrApprovalRemindersOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrApprovalRemindersJob>();

        builder.Services.Configure<SupplyArrProcurementExceptionEscalationsOptions>(
            builder.Configuration.GetSection(SupplyArrProcurementExceptionEscalationsOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrProcurementExceptionEscalationsClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrProcurementExceptionEscalationsOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrProcurementExceptionEscalationsJob>();

        builder.Services.Configure<SupplyArrDemandProcessingOptions>(
            builder.Configuration.GetSection(SupplyArrDemandProcessingOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrDemandProcessingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrDemandProcessingOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrDemandProcessingJob>();

        builder.Services.Configure<SupplyArrIntegrationEventsOptions>(
            builder.Configuration.GetSection(SupplyArrIntegrationEventsOptions.SectionName));

        builder.Services.AddHttpClient<SupplyArrIntegrationEventsClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SupplyArrIntegrationEventsOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.SupplyArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<SupplyArrIntegrationEventsJob>();

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

        builder.Services.Configure<RoutArrAuditPackageGenerationOptions>(
            builder.Configuration.GetSection(RoutArrAuditPackageGenerationOptions.SectionName));

        builder.Services.AddHttpClient<RoutArrAuditPackageGenerationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RoutArrAuditPackageGenerationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.RoutArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddHostedService<RoutArrAuditPackageGenerationJob>();

        builder.Services.Configure<TrainArrAuditPackageGenerationOptions>(
            builder.Configuration.GetSection(TrainArrAuditPackageGenerationOptions.SectionName));

        builder.Services.AddHttpClient<TrainArrAuditPackageGenerationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<TrainArrAuditPackageGenerationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.TrainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddHostedService<TrainArrAuditPackageGenerationJob>();

        builder.Services.Configure<NexArrServiceTokenCleanupOptions>(
            builder.Configuration.GetSection(NexArrServiceTokenCleanupOptions.SectionName));

        builder.Services.AddHttpClient<NexArrServiceTokenCleanupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NexArrServiceTokenCleanupOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.NexArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<NexArrServiceTokenCleanupJob>();

        builder.Services.Configure<NexArrEntitlementReconciliationOptions>(
            builder.Configuration.GetSection(NexArrEntitlementReconciliationOptions.SectionName));

        builder.Services.AddHttpClient<NexArrEntitlementReconciliationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NexArrEntitlementReconciliationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.NexArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<NexArrEntitlementReconciliationJob>();

        builder.Services.Configure<NexArrTenantLifecycleOptions>(
            builder.Configuration.GetSection(NexArrTenantLifecycleOptions.SectionName));

        builder.Services.AddHttpClient<NexArrTenantLifecycleClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NexArrTenantLifecycleOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.NexArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<NexArrTenantLifecycleJob>();

        builder.Services.Configure<MaintainArrDefectEscalationOptions>(
            builder.Configuration.GetSection(MaintainArrDefectEscalationOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrDefectEscalationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrDefectEscalationOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrDefectEscalationJob>();

        builder.Services.Configure<MaintainArrAssetStatusRollupOptions>(
            builder.Configuration.GetSection(MaintainArrAssetStatusRollupOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrAssetStatusRollupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrAssetStatusRollupOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrAssetStatusRollupJob>();

        builder.Services.Configure<MaintainArrMaintenanceHistoryRollupOptions>(
            builder.Configuration.GetSection(MaintainArrMaintenanceHistoryRollupOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrMaintenanceHistoryRollupClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrMaintenanceHistoryRollupOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrMaintenanceHistoryRollupJob>();

        builder.Services.Configure<MaintainArrDowntimeSyncOptions>(
            builder.Configuration.GetSection(MaintainArrDowntimeSyncOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrDowntimeSyncClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrDowntimeSyncOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrDowntimeSyncJob>();

        builder.Services.Configure<MaintainArrPlatformEventProcessingOptions>(
            builder.Configuration.GetSection(MaintainArrPlatformEventProcessingOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrPlatformEventProcessingClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrPlatformEventProcessingOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrPlatformEventProcessingJob>();
    });
