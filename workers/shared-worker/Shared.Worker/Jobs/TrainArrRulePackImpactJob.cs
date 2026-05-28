using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class TrainArrRulePackImpactJob(
    ILogger<TrainArrRulePackImpactJob> logger,
    TrainArrRulePackImpactClient trainArrClient,
    IOptions<TrainArrRulePackImpactOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("TrainArr rule pack impact job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "TrainArr rule pack impact job is enabled but {Setting} is not configured.",
                $"{TrainArrRulePackImpactOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "TrainArr rule pack impact job started (interval {IntervalMinutes} min, batch size {BatchSize}, staleness {StalenessHours}h).",
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
                || result.AssessedCount > 0
                || result.AttentionRequiredCount > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "TrainArr rule pack impact scan: candidates={Candidates}, assessed={Assessed}, attention={Attention}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.AssessedCount,
                    result.AttentionRequiredCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TrainArr rule pack impact scan failed.");
        }
    }
}
