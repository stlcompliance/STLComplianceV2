using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class NexArrServiceTokenCleanupJob(
    ILogger<NexArrServiceTokenCleanupJob> logger,
    NexArrServiceTokenCleanupClient nexArrClient,
    IOptions<NexArrServiceTokenCleanupOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("NexArr service token cleanup job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "NexArr service token cleanup job is enabled but {Setting} is not configured.",
                $"{NexArrServiceTokenCleanupOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "NexArr service token cleanup job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await nexArrClient.ProcessBatchAsync(cancellationToken);
            if (result.CandidatesFound > 0
                || result.PurgedCount > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "NexArr service token cleanup scan: candidates={Candidates}, purged={Purged}, expired={Expired}, revoked={Revoked}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.PurgedCount,
                    result.ExpiredPurgeCount,
                    result.RevokedPurgeCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "NexArr service token cleanup scan failed.");
        }
    }
}
