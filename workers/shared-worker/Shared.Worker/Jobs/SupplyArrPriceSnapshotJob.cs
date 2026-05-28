using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class SupplyArrPriceSnapshotJob(
    ILogger<SupplyArrPriceSnapshotJob> logger,
    SupplyArrPriceSnapshotClient supplyArrClient,
    IOptions<SupplyArrPriceSnapshotOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("SupplyArr price snapshot job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "SupplyArr price snapshot job is enabled but {Setting} is not configured.",
                $"{SupplyArrPriceSnapshotOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "SupplyArr price snapshot job started (interval {IntervalMinutes} min, batch size {BatchSize}, staleness {StalenessHours}h).",
            interval.TotalMinutes,
            settings.BatchSize,
            settings.StalenessHours);

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
            if (result.CandidatesFound > 0 || result.CapturedCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "SupplyArr price snapshot capture: candidates={Candidates}, captured={Captured}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.CapturedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "SupplyArr price snapshot capture failed.");
        }
    }
}
