using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class NexArrSmartImportJob(
    ILogger<NexArrSmartImportJob> logger,
    NexArrSmartImportClient nexArrClient,
    IOptions<NexArrSmartImportOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("NexArr Smart Import job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "NexArr Smart Import job is enabled but {Setting} is not configured.",
                $"{NexArrSmartImportOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "NexArr Smart Import job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            if (result.Processed)
            {
                logger.LogInformation(
                    "NexArr Smart Import processed batch {BatchId} with status {Status}.",
                    result.BatchId,
                    result.Status);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "NexArr Smart Import scan failed.");
        }
    }
}
