using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class StaffArrPersonnelHistoryRollupJob(
    ILogger<StaffArrPersonnelHistoryRollupJob> logger,
    StaffArrPersonnelHistoryRollupClient staffArrClient,
    IOptions<StaffArrPersonnelHistoryRollupOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("StaffArr personnel history rollup job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "StaffArr personnel history rollup job is enabled but {Setting} is not configured.",
                $"{StaffArrPersonnelHistoryRollupOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "StaffArr personnel history rollup job started (interval {IntervalMinutes} min, batch size {BatchSize}, staleness {StalenessHours} h).",
            interval.TotalMinutes,
            settings.BatchSize,
            settings.StalenessHours);

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
            var result = await staffArrClient.ProcessBatchAsync(cancellationToken);
            if (result.CandidatesFound > 0 || result.RefreshedCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "StaffArr personnel history rollup scan: candidates={Candidates}, refreshed={Refreshed}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.RefreshedCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "StaffArr personnel history rollup scan failed.");
        }
    }
}
