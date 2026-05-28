using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record TrainArrProcessRulePackImpactResult(
    int CandidatesFound,
    int AssessedCount,
    int AttentionRequiredCount,
    int SkippedCount);

public sealed class TrainArrRulePackImpactClient(
    HttpClient httpClient,
    IOptions<TrainArrRulePackImpactOptions> options)
{
    public async Task<TrainArrProcessRulePackImpactResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/rule-pack-impact/process-batch");
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
        var payload = await response.Content.ReadFromJsonAsync<ProcessRulePackImpactPayload>(cancellationToken);
        return new TrainArrProcessRulePackImpactResult(
            payload?.CandidatesFound ?? 0,
            payload?.AssessedCount ?? 0,
            payload?.AttentionRequiredCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessRulePackImpactPayload(
        int CandidatesFound,
        int AssessedCount,
        int AttentionRequiredCount,
        int SkippedCount);
}
