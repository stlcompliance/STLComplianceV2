using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class TrainArrStaffarrPublicationRetryJob(
    ILogger<TrainArrStaffarrPublicationRetryJob> logger,
    TrainArrStaffarrPublicationRetryClient trainArrClient,
    IOptions<TrainArrStaffarrPublicationRetryOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("TrainArr StaffArr publication retry job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "TrainArr StaffArr publication retry job is enabled but {Setting} is not configured.",
                $"{TrainArrStaffarrPublicationRetryOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "TrainArr StaffArr publication retry job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await trainArrClient.ProcessBatchAsync(cancellationToken);
            if (result.PendingFound > 0
                || result.DeliveredCount > 0
                || result.RetriedCount > 0
                || result.AbandonedCount > 0)
            {
                logger.LogInformation(
                    "TrainArr StaffArr publication retry scan: pending={Pending}, delivered={Delivered}, retried={Retried}, abandoned={Abandoned}, skipped={Skipped}",
                    result.PendingFound,
                    result.DeliveredCount,
                    result.RetriedCount,
                    result.AbandonedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TrainArr StaffArr publication retry scan failed.");
        }
    }
}
