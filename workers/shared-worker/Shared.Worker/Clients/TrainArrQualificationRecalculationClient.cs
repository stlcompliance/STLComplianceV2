using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record TrainArrProcessQualificationRecalculationResult(
    int CandidatesFound,
    int RecalculatedCount,
    int SuspendedCount,
    int SkippedCount);

public sealed class TrainArrQualificationRecalculationClient(
    HttpClient httpClient,
    IOptions<TrainArrQualificationRecalculationOptions> options)
{
    public async Task<TrainArrProcessQualificationRecalculationResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/qualification-recalculation/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
            stalenessHours = settings.StalenessHours
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessRecalculationPayload>(cancellationToken);
        return new TrainArrProcessQualificationRecalculationResult(
            payload?.CandidatesFound ?? 0,
            payload?.RecalculatedCount ?? 0,
            payload?.SuspendedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessRecalculationPayload(
        int CandidatesFound,
        int RecalculatedCount,
        int SuspendedCount,
        int SkippedCount);
}
