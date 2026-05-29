using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record RoutArrProcessIntegrationEventsResult(
    int PendingCount,
    int ProcessedCount,
    int SkippedCount,
    int AbandonedCount);

public sealed class RoutArrIntegrationEventsClient(
    HttpClient httpClient,
    IOptions<RoutArrIntegrationEventsOptions> options)
{
    public async Task<RoutArrProcessIntegrationEventsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/integration-events/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessIntegrationEventsPayload>(cancellationToken);
        return new RoutArrProcessIntegrationEventsResult(
            payload?.PendingCount ?? 0,
            payload?.ProcessedCount ?? 0,
            payload?.SkippedCount ?? 0,
            payload?.AbandonedCount ?? 0);
    }

    private sealed record ProcessIntegrationEventsPayload(
        int PendingCount,
        int ProcessedCount,
        int SkippedCount,
        int AbandonedCount);
}
