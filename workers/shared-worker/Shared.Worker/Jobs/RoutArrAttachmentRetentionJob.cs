using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class RoutArrAttachmentRetentionJob(
    ILogger<RoutArrAttachmentRetentionJob> logger,
    RoutArrAttachmentRetentionClient routArrClient,
    IOptions<RoutArrAttachmentRetentionOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("RoutArr attachment retention job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "RoutArr attachment retention job is enabled but {Setting} is not configured.",
                $"{RoutArrAttachmentRetentionOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "RoutArr attachment retention job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await routArrClient.ProcessBatchAsync(cancellationToken);
            if (result.CandidatesFound > 0
                || result.PurgedCount > 0
                || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "RoutArr attachment retention scan: candidates={Candidates}, purged={Purged}, bytes={Bytes}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.PurgedCount,
                    result.BytesReclaimed,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "RoutArr attachment retention scan failed.");
        }
    }
}
