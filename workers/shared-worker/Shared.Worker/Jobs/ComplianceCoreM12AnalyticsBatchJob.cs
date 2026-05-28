using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class ComplianceCoreM12AnalyticsBatchJob(
    ILogger<ComplianceCoreM12AnalyticsBatchJob> logger,
    ComplianceCoreM12AnalyticsBatchClient complianceCoreClient,
    IOptions<ComplianceCoreM12AnalyticsBatchOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Compliance Core M12 analytics batch job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "Compliance Core M12 analytics batch job is enabled but {Setting} is not configured.",
                $"{ComplianceCoreM12AnalyticsBatchOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "Compliance Core M12 analytics batch job started (interval {IntervalMinutes} min, batch size {BatchSize}, interval hours {IntervalHours}).",
            interval.TotalMinutes,
            settings.BatchSize,
            settings.IntervalHours);

        using var timer = new PeriodicTimer(interval);
        do
        {
            await RunBatchAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunBatchAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await complianceCoreClient.ProcessBatchAsync(cancellationToken);
            if (result.TenantsDueCount > 0 || result.ProcessedCount > 0)
            {
                logger.LogInformation(
                    "Compliance Core M12 analytics batch: due={Due}, processed={Processed}, skipped={Skipped}",
                    result.TenantsDueCount,
                    result.ProcessedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Compliance Core M12 analytics batch failed.");
        }
    }
}
