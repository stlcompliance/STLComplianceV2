using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class TrainArrRecertificationAssignmentJob(
    ILogger<TrainArrRecertificationAssignmentJob> logger,
    TrainArrRecertificationAssignmentClient trainArrClient,
    IOptions<TrainArrRecertificationAssignmentOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("TrainArr recertification assignment job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "TrainArr recertification assignment job is enabled but {Setting} is not configured.",
                $"{TrainArrRecertificationAssignmentOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "TrainArr recertification assignment job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            if (result.CandidatesFound > 0 || result.AssignedCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "TrainArr recertification assignment scan: candidates={Candidates}, assigned={Assigned}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.AssignedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TrainArr recertification assignment scan failed.");
        }
    }
}
