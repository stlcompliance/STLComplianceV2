using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class NexArrTenantLifecycleJob(
    ILogger<NexArrTenantLifecycleJob> logger,
    NexArrTenantLifecycleClient nexArrClient,
    IOptions<NexArrTenantLifecycleOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("NexArr tenant lifecycle job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "NexArr tenant lifecycle job is enabled but {Setting} is not configured.",
                $"{NexArrTenantLifecycleOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "NexArr tenant lifecycle job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            if (result.PendingCount > 0
                || result.SuspendedCount > 0
                || result.ReactivatedCount > 0
                || result.SessionsRevokedCount > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "NexArr tenant lifecycle scan: pending={Pending}, suspended={Suspended}, reactivated={Reactivated}, sessionsRevoked={SessionsRevoked}, skipped={Skipped}",
                    result.PendingCount,
                    result.SuspendedCount,
                    result.ReactivatedCount,
                    result.SessionsRevokedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "NexArr tenant lifecycle scan failed.");
        }
    }
}
