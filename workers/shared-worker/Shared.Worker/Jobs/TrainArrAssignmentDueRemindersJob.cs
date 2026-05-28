using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class TrainArrAssignmentDueRemindersJob(
    ILogger<TrainArrAssignmentDueRemindersJob> logger,
    TrainArrAssignmentDueRemindersClient trainArrClient,
    IOptions<TrainArrAssignmentDueRemindersOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("TrainArr assignment due reminders job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "TrainArr assignment due reminders job is enabled but {Setting} is not configured.",
                $"{TrainArrAssignmentDueRemindersOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "TrainArr assignment due reminders job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            if (result.CandidatesFound > 0 || result.RemindersSentCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "TrainArr assignment due reminders: candidates={Candidates}, sent={Sent}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.RemindersSentCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TrainArr assignment due reminders scan failed.");
        }
    }
}
