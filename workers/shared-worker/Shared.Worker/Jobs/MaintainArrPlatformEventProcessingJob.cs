using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class MaintainArrPlatformEventProcessingJob(
    ILogger<MaintainArrPlatformEventProcessingJob> logger,
    MaintainArrPlatformEventProcessingClient maintainArrClient,
    IOptions<MaintainArrPlatformEventProcessingOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("MaintainArr platform event processing job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "MaintainArr platform event processing job is enabled but {Setting} is not configured.",
                $"{MaintainArrPlatformEventProcessingOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "MaintainArr platform event processing job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
            interval.TotalMinutes,
            settings.BatchSize);

        using var timer = new PeriodicTimer(interval);
        do
        {
            await RunScanAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunScanAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await maintainArrClient.ProcessBatchAsync(cancellationToken);
            if (result.PendingFound > 0
                || result.ProcessedCount > 0
                || result.RetriedCount > 0
                || result.AbandonedCount > 0)
            {
                logger.LogInformation(
                    "MaintainArr platform event processing scan: pending={Pending}, processed={Processed}, retried={Retried}, abandoned={Abandoned}, skipped={Skipped}",
                    result.PendingFound,
                    result.ProcessedCount,
                    result.RetriedCount,
                    result.AbandonedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "MaintainArr platform event processing scan failed.");
        }
    }
}
