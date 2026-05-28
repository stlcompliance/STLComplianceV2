using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessProcurementNotificationsResult(
    int PendingFound,
    int DispatchedCount,
    int SkippedCount);

public sealed class SupplyArrNotificationDispatchClient(
    HttpClient httpClient,
    IOptions<SupplyArrNotificationDispatchOptions> options)
{
    public async Task<SupplyArrProcessProcurementNotificationsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/procurement-notifications/process-batch");
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
        return new SupplyArrProcessProcurementNotificationsResult(
            payload?.PendingFound ?? 0,
            payload?.DispatchedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int PendingFound,
        int DispatchedCount,
        int SkippedCount);
}
