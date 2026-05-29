using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class SupplyArrProcurementExceptionEscalationsJob(
    ILogger<SupplyArrProcurementExceptionEscalationsJob> logger,
    SupplyArrProcurementExceptionEscalationsClient supplyArrClient,
    IOptions<SupplyArrProcurementExceptionEscalationsOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("SupplyArr procurement exception escalations job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "SupplyArr procurement exception escalations job is enabled but {Setting} is not configured.",
                $"{SupplyArrProcurementExceptionEscalationsOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "SupplyArr procurement exception escalations job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            if (result.CandidatesFound > 0 || result.EscalatedCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "SupplyArr procurement exception escalations: candidates={Candidates}, escalated={Escalated}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.EscalatedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "SupplyArr procurement exception escalations failed.");
        }
    }
}
