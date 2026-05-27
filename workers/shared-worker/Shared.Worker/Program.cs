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

        builder.Services.Configure<MaintainArrPmDueScanOptions>(
            builder.Configuration.GetSection(MaintainArrPmDueScanOptions.SectionName));

        builder.Services.AddHttpClient<MaintainArrPmDueScanClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MaintainArrPmDueScanOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.MaintainArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<MaintainArrPmDueScanJob>();

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
