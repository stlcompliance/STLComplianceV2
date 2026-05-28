using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class TrainArrQualificationRecalculationJob(
    ILogger<TrainArrQualificationRecalculationJob> logger,
    TrainArrQualificationRecalculationClient trainArrClient,
    IOptions<TrainArrQualificationRecalculationOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("TrainArr qualification recalculation job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "TrainArr qualification recalculation job is enabled but {Setting} is not configured.",
                $"{TrainArrQualificationRecalculationOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "TrainArr qualification recalculation job started (interval {IntervalMinutes} min, batch size {BatchSize}, staleness {StalenessHours}h).",
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
            var result = await trainArrClient.ProcessBatchAsync(cancellationToken);
            if (result.CandidatesFound > 0
                || result.RecalculatedCount > 0
                || result.SuspendedCount > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "TrainArr qualification recalculation scan: candidates={Candidates}, recalculated={Recalculated}, suspended={Suspended}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.RecalculatedCount,
                    result.SuspendedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TrainArr qualification recalculation scan failed.");
        }
    }
}
