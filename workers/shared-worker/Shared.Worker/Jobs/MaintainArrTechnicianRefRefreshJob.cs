using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class MaintainArrTechnicianRefRefreshJob(
    ILogger<MaintainArrTechnicianRefRefreshJob> logger,
    MaintainArrTechnicianRefRefreshClient maintainArrClient,
    IOptions<MaintainArrTechnicianRefRefreshOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("MaintainArr technician ref refresh job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "MaintainArr technician ref refresh job is enabled but {Setting} is not configured.",
                $"{MaintainArrTechnicianRefRefreshOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "MaintainArr technician ref refresh job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
            interval.TotalMinutes,
            settings.BatchSize);

        using var timer = new PeriodicTimer(interval);
        do
        {
            await RunRefreshAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunRefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await maintainArrClient.ProcessRefreshAsync(cancellationToken);
            if (result.CandidatesFound > 0
                || result.RefreshedCount > 0
                || result.SkippedCount > 0
                || result.FailedCount > 0)
            {
                logger.LogInformation(
                    "MaintainArr technician ref refresh: candidates={Candidates}, refreshed={Refreshed}, skipped={Skipped}, failed={Failed}",
                    result.CandidatesFound,
                    result.RefreshedCount,
                    result.SkippedCount,
                    result.FailedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "MaintainArr technician ref refresh failed.");
        }
    }
}
