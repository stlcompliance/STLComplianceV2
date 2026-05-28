using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record TrainArrProcessStaffarrPublicationRetryResult(
    int PendingFound,
    int DeliveredCount,
    int RetriedCount,
    int AbandonedCount,
    int SkippedCount);

public sealed class TrainArrStaffarrPublicationRetryClient(
    HttpClient httpClient,
    IOptions<TrainArrStaffarrPublicationRetryOptions> options)
{
    public async Task<TrainArrProcessStaffarrPublicationRetryResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/staffarr-publication-retries/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessRetryPayload>(cancellationToken);
        return new TrainArrProcessStaffarrPublicationRetryResult(
            payload?.PendingFound ?? 0,
            payload?.DeliveredCount ?? 0,
            payload?.RetriedCount ?? 0,
            payload?.AbandonedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessRetryPayload(
        int PendingFound,
        int DeliveredCount,
        int RetriedCount,
        int AbandonedCount,
        int SkippedCount);
}
