using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class StaffArrCertificationExpirationJob(
    ILogger<StaffArrCertificationExpirationJob> logger,
    StaffArrCertificationExpirationClient staffArrClient,
    IOptions<StaffArrCertificationExpirationOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("StaffArr certification expiration job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "StaffArr certification expiration job is enabled but {Setting} is not configured.",
                $"{StaffArrCertificationExpirationOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "StaffArr certification expiration job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
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
            var result = await staffArrClient.ProcessExpirationsAsync(cancellationToken);
            if (result.CandidatesFound > 0 || result.ExpiredCount > 0 || result.SkippedCount > 0)
            {
                logger.LogInformation(
                    "StaffArr certification expiration scan: candidates={Candidates}, expired={Expired}, skipped={Skipped}",
                    result.CandidatesFound,
                    result.ExpiredCount,
                    result.SkippedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "StaffArr certification expiration scan failed.");
        }
    }
}
