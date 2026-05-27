using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class TrainArrQualificationExpirationJob(
    ILogger<TrainArrQualificationExpirationJob> logger,
    TrainArrQualificationExpirationClient trainArrClient,
    IOptions<TrainArrQualificationExpirationOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("TrainArr qualification expiration job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "TrainArr qualification expiration job is enabled but {Setting} is not configured.",
                $"{TrainArrQualificationExpirationOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "TrainArr qualification expiration job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await trainArrClient.ProcessExpirationsAsync(cancellationToken);
            if (result.CandidatesFound > 0 || result.ExpiredCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "TrainArr qualification expiration scan: candidates={Candidates}, expired={Expired}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.ExpiredCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TrainArr qualification expiration scan failed.");
        }
    }
}
