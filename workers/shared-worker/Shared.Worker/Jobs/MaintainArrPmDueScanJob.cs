using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class MaintainArrPmDueScanJob(
    ILogger<MaintainArrPmDueScanJob> logger,
    MaintainArrPmDueScanClient maintainArrClient,
    IOptions<MaintainArrPmDueScanOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("MaintainArr PM due scan job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "MaintainArr PM due scan job is enabled but {Setting} is not configured.",
                $"{MaintainArrPmDueScanOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "MaintainArr PM due scan job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await maintainArrClient.ProcessDueScanAsync(cancellationToken);
            if (result.CandidatesFound > 0
                || result.MarkedDueCount > 0
                || result.MarkedOverdueCount > 0
                || result.SkippedCount > 0
                || result.WorkOrdersCreatedCount > 0
                || result.WorkOrdersLinkedCount > 0)
            {
                logger.LogInformation(
                    "MaintainArr PM due scan: candidates={Candidates}, due={Due}, overdue={Overdue}, skipped={Skipped}, workOrdersCreated={WorkOrdersCreated}, workOrdersLinked={WorkOrdersLinked}",
                    result.CandidatesFound,
                    result.MarkedDueCount,
                    result.MarkedOverdueCount,
                    result.SkippedCount,
                    result.WorkOrdersCreatedCount,
                    result.WorkOrdersLinkedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "MaintainArr PM due scan failed.");
        }
    }
}
