using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class MaintainArrDowntimeSyncJob(
    ILogger<MaintainArrDowntimeSyncJob> logger,
    MaintainArrDowntimeSyncClient maintainArrClient,
    IOptions<MaintainArrDowntimeSyncOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("MaintainArr downtime sync job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "MaintainArr downtime sync job is enabled but {Setting} is not configured.",
                $"{MaintainArrDowntimeSyncOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "MaintainArr downtime sync job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            if (result.AssetsScanned > 0 || result.EventsOpened > 0 || result.EventsClosed > 0)
            {
                logger.LogInformation(
                    "MaintainArr downtime sync: scanned={Scanned}, opened={Opened}, closed={Closed}, snapshots={Snapshots}",
                    result.AssetsScanned,
                    result.EventsOpened,
                    result.EventsClosed,
                    result.SnapshotsRefreshed);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "MaintainArr downtime sync scan failed.");
        }
    }
}
