using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record TrainArrProcessOrphanReferenceResult(
    int TenantsScanned,
    int ReferencesChecked,
    int FindingsDetected,
    int FindingsResolved,
    int SkippedCount);

public sealed class TrainArrOrphanReferenceClient(
    HttpClient httpClient,
    IOptions<TrainArrOrphanReferenceOptions> options)
{
    public async Task<TrainArrProcessOrphanReferenceResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/orphan-references/process-batch");
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
        var payload = await response.Content.ReadFromJsonAsync<ProcessOrphanReferencePayload>(cancellationToken);
        return new TrainArrProcessOrphanReferenceResult(
            payload?.TenantsScanned ?? 0,
            payload?.ReferencesChecked ?? 0,
            payload?.FindingsDetected ?? 0,
            payload?.FindingsResolved ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessOrphanReferencePayload(
        int TenantsScanned,
        int ReferencesChecked,
        int FindingsDetected,
        int FindingsResolved,
        int SkippedCount);
}
