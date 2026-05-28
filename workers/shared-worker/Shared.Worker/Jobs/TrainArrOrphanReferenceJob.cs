using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class TrainArrOrphanReferenceJob(
    ILogger<TrainArrOrphanReferenceJob> logger,
    TrainArrOrphanReferenceClient trainArrClient,
    IOptions<TrainArrOrphanReferenceOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("TrainArr orphan reference job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "TrainArr orphan reference job is enabled but {Setting} is not configured.",
                $"{TrainArrOrphanReferenceOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "TrainArr orphan reference job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await trainArrClient.ProcessBatchAsync(cancellationToken);
            if (result.TenantsScanned > 0
                || result.FindingsDetected > 0
                || result.FindingsResolved > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "TrainArr orphan reference scan: tenants={Tenants}, references={References}, findings={Findings}, resolved={Resolved}, skipped={Skipped}",
                    result.TenantsScanned,
                    result.ReferencesChecked,
                    result.FindingsDetected,
                    result.FindingsResolved,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TrainArr orphan reference scan failed.");
        }
    }
}
