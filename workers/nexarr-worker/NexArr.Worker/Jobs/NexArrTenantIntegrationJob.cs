using Microsoft.Extensions.Options;
using NexArr.Worker.Clients;
using NexArr.Worker.Options;

namespace NexArr.Worker.Jobs;

public sealed class NexArrTenantIntegrationJob(
    ILogger<NexArrTenantIntegrationJob> logger,
    NexArrTenantIntegrationClient nexArrClient,
    IOptions<NexArrTenantIntegrationOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("NexArr tenant integration sync job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "NexArr tenant integration sync job is enabled but {Setting} is not configured.",
                $"{NexArrTenantIntegrationOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "NexArr tenant integration sync job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
                || result.SucceededCount > 0
                || result.FailedCount > 0
                || result.NeedsReviewCount > 0
                || result.SourceUnavailableCount > 0
                || result.DeadLetterCount > 0)
            {
                logger.LogInformation(
                    "NexArr tenant integration scan: candidates={Candidates}, succeeded={Succeeded}, failed={Failed}, needsReview={NeedsReview}, sourceUnavailable={SourceUnavailable}, deadLetter={DeadLetter}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.SucceededCount,
                    result.FailedCount,
                    result.NeedsReviewCount,
                    result.SourceUnavailableCount,
                    result.DeadLetterCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "NexArr tenant integration scan failed.");
        }
    }
}
