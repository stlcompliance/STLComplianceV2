using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class SupplyArrDemandProcessingJob(
    ILogger<SupplyArrDemandProcessingJob> logger,
    SupplyArrDemandProcessingClient supplyArrClient,
    IOptions<SupplyArrDemandProcessingOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("SupplyArr demand processing job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "SupplyArr demand processing job is enabled but {Setting} is not configured.",
                $"{SupplyArrDemandProcessingOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "SupplyArr demand processing job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            if (result.CandidatesFound > 0 || result.ProcessedCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "SupplyArr demand processing: candidates={Candidates}, processed={Processed}, prDrafts={PrDrafts}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.ProcessedCount,
                    result.PrDraftsCreatedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "SupplyArr demand processing failed.");
        }
    }
}
