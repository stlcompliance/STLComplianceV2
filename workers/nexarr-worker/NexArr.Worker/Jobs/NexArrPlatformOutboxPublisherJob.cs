using Microsoft.Extensions.Options;
using NexArr.Worker.Clients;
using NexArr.Worker.Options;

namespace NexArr.Worker.Jobs;

public sealed class NexArrPlatformOutboxPublisherJob(
    ILogger<NexArrPlatformOutboxPublisherJob> logger,
    NexArrPlatformOutboxPublisherClient nexArrClient,
    IOptions<NexArrPlatformOutboxPublisherOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("NexArr platform outbox publisher job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "NexArr platform outbox publisher job is enabled but {Setting} is not configured.",
                $"{NexArrPlatformOutboxPublisherOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "NexArr platform outbox publisher job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
                || result.PublishedCount > 0
                || result.FailedCount > 0
                || result.DeadLetterCount > 0)
            {
                logger.LogInformation(
                    "NexArr platform outbox scan: candidates={Candidates}, published={Published}, failed={Failed}, deadLetter={DeadLetter}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.PublishedCount,
                    result.FailedCount,
                    result.DeadLetterCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "NexArr platform outbox scan failed.");
        }
    }
}
