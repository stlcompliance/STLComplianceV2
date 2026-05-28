using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record NexArrProcessEntitlementReconciliationResult(
    int DriftFoundCount,
    int GrantedCount,
    int RevokedCount,
    int SkippedCount);

public sealed class NexArrEntitlementReconciliationClient(
    HttpClient httpClient,
    IOptions<NexArrEntitlementReconciliationOptions> options)
{
    public async Task<NexArrProcessEntitlementReconciliationResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/internal/entitlement-reconciliation/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new NexArrProcessEntitlementReconciliationResult(
            payload?.DriftFoundCount ?? 0,
            payload?.GrantedCount ?? 0,
            payload?.RevokedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int DriftFoundCount,
        int GrantedCount,
        int RevokedCount,
        int SkippedCount);
}
