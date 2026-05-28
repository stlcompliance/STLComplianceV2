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

        builder.Services.Configure<StaffArrPersonExportDeliveryOptions>(
            builder.Configuration.GetSection(StaffArrPersonExportDeliveryOptions.SectionName));

        builder.Services.AddHttpClient<StaffArrPersonExportDeliveryClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<StaffArrPersonExportDeliveryOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.StaffArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddHostedService<StaffArrPersonExportDeliveryJob>();

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
    });
