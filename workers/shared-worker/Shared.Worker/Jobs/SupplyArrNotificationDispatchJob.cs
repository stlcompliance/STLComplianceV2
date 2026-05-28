using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class SupplyArrNotificationDispatchJob(
    ILogger<SupplyArrNotificationDispatchJob> logger,
    SupplyArrNotificationDispatchClient supplyArrClient,
    IOptions<SupplyArrNotificationDispatchOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("SupplyArr notification dispatch job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "SupplyArr notification dispatch job is enabled but {Setting} is not configured.",
                $"{SupplyArrNotificationDispatchOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "SupplyArr notification dispatch job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await supplyArrClient.ProcessBatchAsync(cancellationToken);
            if (result.PendingFound > 0
                || result.DispatchedCount > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "SupplyArr notification dispatch scan: pending={Pending}, dispatched={Dispatched}, skipped={Skipped}",
                    result.PendingFound,
                    result.DispatchedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "SupplyArr notification dispatch scan failed.");
        }
    }
}
