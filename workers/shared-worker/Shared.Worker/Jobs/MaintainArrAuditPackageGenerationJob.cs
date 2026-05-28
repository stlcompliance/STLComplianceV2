using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class MaintainArrAuditPackageGenerationJob(
    ILogger<MaintainArrAuditPackageGenerationJob> logger,
    MaintainArrAuditPackageGenerationClient MaintainArrClient,
    IOptions<MaintainArrAuditPackageGenerationOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Compliance Core audit package generation job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "Compliance Core audit package generation job is enabled but {Setting} is not configured.",
                $"{MaintainArrAuditPackageGenerationOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "Compliance Core audit package generation job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await MaintainArrClient.ProcessBatchAsync(cancellationToken);
            if (result.CandidatesFound > 0
                || result.CompletedCount > 0
                || result.FailedCount > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "Compliance Core audit package generation scan: candidates={Candidates}, completed={Completed}, failed={Failed}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.CompletedCount,
                    result.FailedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Compliance Core audit package generation scan failed.");
        }
    }
}
