using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessReorderEvaluationResult(
    int CandidatesFound,
    int SuggestionsCount,
    int SkippedOpenPurchaseRequestCount,
    int DraftPurchaseRequestsCreated);

public sealed class SupplyArrReorderEvaluationClient(
    HttpClient httpClient,
    IOptions<SupplyArrReorderEvaluationOptions> options)
{
    public async Task<SupplyArrProcessReorderEvaluationResult> ProcessEvaluationAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/reorder/process-evaluation");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
            createDraftPurchaseRequests = settings.CreateDraftPurchaseRequests
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessReorderEvaluationPayload>(cancellationToken);
        return new SupplyArrProcessReorderEvaluationResult(
            payload?.CandidatesFound ?? 0,
            payload?.SuggestionsCount ?? 0,
            payload?.SkippedOpenPurchaseRequestCount ?? 0,
            payload?.DraftPurchaseRequestsCreated ?? 0);
    }

    private sealed record ProcessReorderEvaluationPayload(
        int CandidatesFound,
        int SuggestionsCount,
        int SkippedOpenPurchaseRequestCount,
        int DraftPurchaseRequestsCreated);
}
