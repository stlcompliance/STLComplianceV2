using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class RoutArrIntegrationEventsJob(
    ILogger<RoutArrIntegrationEventsJob> logger,
    RoutArrIntegrationEventsClient routArrClient,
    IOptions<RoutArrIntegrationEventsOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("RoutArr integration events job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "RoutArr integration events job is enabled but {Setting} is not configured.",
                $"{RoutArrIntegrationEventsOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "RoutArr integration events job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await routArrClient.ProcessBatchAsync(cancellationToken);
            if (result.ProcessedCount > 0 || result.SkippedCount > 0 || result.AbandonedCount > 0)
            {
                logger.LogInformation(
                    "RoutArr integration events: pending={Pending}, processed={Processed}, skipped={Skipped}, abandoned={Abandoned}",
                    result.PendingCount,
                    result.ProcessedCount,
                    result.SkippedCount,
                    result.AbandonedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "RoutArr integration events processing failed.");
        }
    }
}
