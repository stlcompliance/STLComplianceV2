using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class MaintainArrAssetStatusRollupJob(
    ILogger<MaintainArrAssetStatusRollupJob> logger,
    MaintainArrAssetStatusRollupClient maintainArrClient,
    IOptions<MaintainArrAssetStatusRollupOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("MaintainArr asset status rollup job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "MaintainArr asset status rollup job is enabled but {Setting} is not configured.",
                $"{MaintainArrAssetStatusRollupOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "MaintainArr asset status rollup job started (interval {IntervalMinutes} min, batch size {BatchSize}, staleness {StalenessHours}h).",
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
            var result = await maintainArrClient.ProcessBatchAsync(cancellationToken);
            if (result.CandidatesFound > 0 || result.RefreshedCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "MaintainArr asset status rollup: candidates={Candidates}, refreshed={Refreshed}, skipped={Skipped}, scopes={Scopes}",
                    result.CandidatesFound,
                    result.RefreshedCount,
                    result.SkippedCount,
                    result.ScopeRollupsRefreshed);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "MaintainArr asset status rollup scan failed.");
        }
    }
}
