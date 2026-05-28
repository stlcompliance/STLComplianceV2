using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record MaintainArrProcessMaintenanceNotificationsResult(
    int EnqueuedPmDueCount,
    int PendingFound,
    int DispatchedCount,
    int SkippedCount);

public sealed class MaintainArrNotificationDispatchClient(
    HttpClient httpClient,
    IOptions<MaintainArrNotificationDispatchOptions> options)
{
    public async Task<MaintainArrProcessMaintenanceNotificationsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/internal/maintenance-notifications/process-batch");
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
        return new MaintainArrProcessMaintenanceNotificationsResult(
            payload?.EnqueuedPmDueCount ?? 0,
            payload?.PendingFound ?? 0,
            payload?.DispatchedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int EnqueuedPmDueCount,
        int PendingFound,
        int DispatchedCount,
        int SkippedCount);
}
