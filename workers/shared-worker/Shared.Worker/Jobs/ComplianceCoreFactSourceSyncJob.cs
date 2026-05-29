using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class ComplianceCoreFactSourceSyncJob(
    ILogger<ComplianceCoreFactSourceSyncJob> logger,
    ComplianceCoreFactSourceSyncClient complianceCoreClient,
    IOptions<ComplianceCoreFactSourceSyncOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Compliance Core fact source sync job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "Compliance Core fact source sync job is enabled but {Setting} is not configured.",
                $"{ComplianceCoreFactSourceSyncOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "Compliance Core fact source sync job started (interval {IntervalMinutes} min, sync interval {SyncIntervalMinutes} min, batch size {BatchSize}).",
            interval.TotalMinutes,
            settings.IntervalMinutes,
            settings.BatchSize);

        using var timer = new PeriodicTimer(interval);
        do
        {
            await RunBatchAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunBatchAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await complianceCoreClient.ProcessBatchAsync(cancellationToken);
            if (result.DueCount > 0 || result.FailedCount > 0)
            {
                logger.LogInformation(
                    "Compliance Core fact source sync: due={Due}, succeeded={Succeeded}, failed={Failed}, skipped={Skipped}",
                    result.DueCount,
                    result.SucceededCount,
                    result.FailedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Compliance Core fact source sync batch failed.");
        }
    }
}
