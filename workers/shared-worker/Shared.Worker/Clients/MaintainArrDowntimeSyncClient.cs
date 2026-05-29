using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record MaintainArrProcessAssetDowntimeSyncResult(
    int AssetsScanned,
    int EventsOpened,
    int EventsClosed,
    int SnapshotsRefreshed);

public sealed class MaintainArrDowntimeSyncClient(
    HttpClient httpClient,
    IOptions<MaintainArrDowntimeSyncOptions> options)
{
    public async Task<MaintainArrProcessAssetDowntimeSyncResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/downtime-sync/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new MaintainArrProcessAssetDowntimeSyncResult(
            payload?.AssetsScanned ?? 0,
            payload?.EventsOpened ?? 0,
            payload?.EventsClosed ?? 0,
            payload?.SnapshotsRefreshed ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int AssetsScanned,
        int EventsOpened,
        int EventsClosed,
        int SnapshotsRefreshed);
}
