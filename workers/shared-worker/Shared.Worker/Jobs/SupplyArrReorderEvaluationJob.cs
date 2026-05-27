using Microsoft.Extensions.Options;
using Shared.Worker.Clients;
using Shared.Worker.Options;

namespace Shared.Worker.Jobs;

public sealed class SupplyArrReorderEvaluationJob(
    ILogger<SupplyArrReorderEvaluationJob> logger,
    SupplyArrReorderEvaluationClient supplyArrClient,
    IOptions<SupplyArrReorderEvaluationOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("SupplyArr reorder evaluation job is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ServiceToken))
        {
            logger.LogWarning(
                "SupplyArr reorder evaluation job is enabled but {Setting} is not configured.",
                $"{SupplyArrReorderEvaluationOptions.SectionName}:ServiceToken");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Clamp(settings.ScanIntervalMinutes, 1, 24 * 60));
        logger.LogInformation(
            "SupplyArr reorder evaluation job started (interval {IntervalMinutes} min, batch size {BatchSize}).",
            interval.TotalMinutes,
            settings.BatchSize);

        using var timer = new PeriodicTimer(interval);
        do
        {
            await RunEvaluationAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunEvaluationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await supplyArrClient.ProcessEvaluationAsync(cancellationToken);
            if (result.CandidatesFound > 0
                || result.SuggestionsCount > 0
                || result.SkippedOpenPurchaseRequestCount > 0
                || result.DraftPurchaseRequestsCreated > 0)
            {
                logger.LogInformation(
                    "SupplyArr reorder evaluation: candidates={Candidates}, suggestions={Suggestions}, skippedOpenPr={SkippedOpenPr}, draftPrsCreated={DraftPrsCreated}",
                    result.CandidatesFound,
                    result.SuggestionsCount,
                    result.SkippedOpenPurchaseRequestCount,
                    result.DraftPurchaseRequestsCreated);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "SupplyArr reorder evaluation failed.");
        }
    }
}
