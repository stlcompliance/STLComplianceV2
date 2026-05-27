using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class ComplianceCoreScheduledEvaluationJob(
    ILogger<ComplianceCoreScheduledEvaluationJob> logger,
    ComplianceCoreScheduledEvaluationClient complianceCoreClient,
    IOptions<ComplianceCoreScheduledEvaluationOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Compliance Core scheduled evaluation job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "Compliance Core scheduled evaluation job is enabled but {Setting} is not configured.",
                $"{ComplianceCoreScheduledEvaluationOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "Compliance Core scheduled evaluation job started (interval {IntervalMinutes} min, batch size {BatchSize}, interval hours {IntervalHours}).",
            interval.TotalMinutes,
            settings.BatchSize,
            settings.IntervalHours);

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
            var result = await complianceCoreClient.ProcessBatchAsync(cancellationToken);
            if (result.PacksDueCount > 0 || result.EvaluatedCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "Compliance Core scheduled evaluation: due={Due}, evaluated={Evaluated}, skipped={Skipped}, allow={Allow}, warn={Warn}, block={Block}",
                    result.PacksDueCount,
                    result.EvaluatedCount,
                    result.SkippedCount,
                    result.AllowCount,
                    result.WarnCount,
                    result.BlockCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Compliance Core scheduled evaluation scan failed.");
        }
    }
}
