using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class NexArrLaunchDestinationReconciliationJob(
    ILogger<NexArrLaunchDestinationReconciliationJob> logger,
    NexArrLaunchDestinationReconciliationClient nexArrClient,
    IOptions<NexArrLaunchDestinationReconciliationOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("NexArr launch-destination reconciliation job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "NexArr launch-destination reconciliation job is enabled but {Setting} is not configured.",
                $"{NexArrLaunchDestinationReconciliationOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "NexArr launch-destination reconciliation job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            if (result.DriftFoundCount > 0
                || result.GrantedCount > 0
                || result.RevokedCount > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "NexArr launch-destination reconciliation scan: drift={Drift}, granted={Granted}, revoked={Revoked}, skipped={Skipped}",
                    result.DriftFoundCount,
                    result.GrantedCount,
                    result.RevokedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "NexArr launch-destination reconciliation scan failed.");
        }
    }
}
