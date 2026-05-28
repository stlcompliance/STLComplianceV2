using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class SupplyArrIntegrationEventsJob(
    ILogger<SupplyArrIntegrationEventsJob> logger,
    SupplyArrIntegrationEventsClient supplyArrClient,
    IOptions<SupplyArrIntegrationEventsOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("SupplyArr integration events job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "SupplyArr integration events job is enabled but {Setting} is not configured.",
                $"{SupplyArrIntegrationEventsOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "SupplyArr integration events job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await supplyArrClient.ProcessBatchAsync(cancellationToken);
            if (result.OutboxProcessedCount > 0
                || result.InboxProcessedCount > 0
                || result.SkippedCount > 0
                || result.AbandonedCount > 0)
            {
                logger.LogInformation(
                    "SupplyArr integration events: outbox={Outbox}, inbox={Inbox}, skipped={Skipped}, abandoned={Abandoned}",
                    result.OutboxProcessedCount,
                    result.InboxProcessedCount,
                    result.SkippedCount,
                    result.AbandonedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "SupplyArr integration events processing failed.");
        }
    }
}
