using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NexArr.Worker.Options;

namespace NexArr.Worker.Clients;

public sealed record NexArrProcessPlatformOutboxPublisherResult(
    int CandidatesFound,
    int PublishedCount,
    int FailedCount,
    int DeadLetterCount,
    int SkippedCount);

public sealed class NexArrPlatformOutboxPublisherClient(
    HttpClient httpClient,
    IOptions<NexArrPlatformOutboxPublisherOptions> options)
{
    public async Task<NexArrProcessPlatformOutboxPublisherResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/internal/platform-outbox/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new NexArrProcessPlatformOutboxPublisherResult(
            payload?.CandidatesFound ?? 0,
            payload?.PublishedCount ?? 0,
            payload?.FailedCount ?? 0,
            payload?.DeadLetterCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int CandidatesFound,
        int PublishedCount,
        int FailedCount,
        int DeadLetterCount,
        int SkippedCount);
}
