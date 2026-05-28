using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessIntegrationEventsResult(
    int OutboxProcessedCount,
    int InboxProcessedCount,
    int SkippedCount,
    int AbandonedCount);

public sealed class SupplyArrIntegrationEventsClient(
    HttpClient httpClient,
    IOptions<SupplyArrIntegrationEventsOptions> options)
{
    public async Task<SupplyArrProcessIntegrationEventsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/integration-events/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessIntegrationEventsPayload>(cancellationToken);
        return new SupplyArrProcessIntegrationEventsResult(
            payload?.OutboxProcessedCount ?? 0,
            payload?.InboxProcessedCount ?? 0,
            payload?.SkippedCount ?? 0,
            payload?.AbandonedCount ?? 0);
    }

    private sealed record ProcessIntegrationEventsPayload(
        int OutboxProcessedCount,
        int InboxProcessedCount,
        int SkippedCount,
        int AbandonedCount);
}
