using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class ComplianceCoreRuleChangeMonitorJob(
    ILogger<ComplianceCoreRuleChangeMonitorJob> logger,
    ComplianceCoreRuleChangeMonitorClient complianceCoreClient,
    IOptions<ComplianceCoreRuleChangeMonitorOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Compliance Core rule change monitor job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "Compliance Core rule change monitor job is enabled but {Setting} is not configured.",
                $"{ComplianceCoreRuleChangeMonitorOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "Compliance Core rule change monitor job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await complianceCoreClient.ProcessScanAsync(cancellationToken);
            if (result.PacksScannedCount > 0 || result.ChangesDetectedCount > 0)
            {
                logger.LogInformation(
                    "Compliance Core rule change scan: scanned={Scanned}, changes={Changes}",
                    result.PacksScannedCount,
                    result.ChangesDetectedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Compliance Core rule change scan failed.");
        }
    }
}
