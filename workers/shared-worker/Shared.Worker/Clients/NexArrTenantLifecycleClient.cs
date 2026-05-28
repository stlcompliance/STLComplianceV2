using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record NexArrProcessTenantLifecycleResult(
    int PendingCount,
    int SuspendedCount,
    int ReactivatedCount,
    int SessionsRevokedCount,
    int SkippedCount);

public sealed class NexArrTenantLifecycleClient(
    HttpClient httpClient,
    IOptions<NexArrTenantLifecycleOptions> options)
{
    public async Task<NexArrProcessTenantLifecycleResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/internal/tenant-lifecycle/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new NexArrProcessTenantLifecycleResult(
            payload?.PendingCount ?? 0,
            payload?.SuspendedCount ?? 0,
            payload?.ReactivatedCount ?? 0,
            payload?.SessionsRevokedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int PendingCount,
        int SuspendedCount,
        int ReactivatedCount,
        int SessionsRevokedCount,
        int SkippedCount);
}
