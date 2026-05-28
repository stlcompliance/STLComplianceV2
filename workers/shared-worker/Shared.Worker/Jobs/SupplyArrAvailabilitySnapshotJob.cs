using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class SupplyArrAvailabilitySnapshotJob(
    ILogger<SupplyArrAvailabilitySnapshotJob> logger,
    SupplyArrAvailabilitySnapshotClient supplyArrClient,
    IOptions<SupplyArrAvailabilitySnapshotOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("SupplyArr availability snapshot job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "SupplyArr availability snapshot job is enabled but {Setting} is not configured.",
                $"{SupplyArrAvailabilitySnapshotOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "SupplyArr availability snapshot job started (interval {IntervalMinutes} min, batch size {BatchSize}, staleness {StalenessHours}h).",
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
                    "SupplyArr availability snapshot capture: candidates={Candidates}, captured={Captured}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.CapturedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "SupplyArr availability snapshot capture failed.");
        }
    }
}
