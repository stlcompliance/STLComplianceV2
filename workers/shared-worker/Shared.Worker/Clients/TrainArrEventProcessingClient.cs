using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record TrainArrProcessTrainingEventsResult(
    int PendingFound,
    int ProcessedCount,
    int RetriedCount,
    int AbandonedCount,
    int SkippedCount);

public sealed class TrainArrEventProcessingClient(
    HttpClient httpClient,
    IOptions<TrainArrEventProcessingOptions> options)
{
    public async Task<TrainArrProcessTrainingEventsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/training-events/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessEventsPayload>(cancellationToken);
        return new TrainArrProcessTrainingEventsResult(
            payload?.PendingFound ?? 0,
            payload?.ProcessedCount ?? 0,
            payload?.RetriedCount ?? 0,
            payload?.AbandonedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessEventsPayload(
        int PendingFound,
        int ProcessedCount,
        int RetriedCount,
        int AbandonedCount,
        int SkippedCount);
}
