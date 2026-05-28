using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record NexArrProcessServiceTokenCleanupResult(
    int CandidatesFound,
    int PurgedCount,
    int ExpiredPurgeCount,
    int RevokedPurgeCount,
    int SkippedCount);

public sealed class NexArrServiceTokenCleanupClient(
    HttpClient httpClient,
    IOptions<NexArrServiceTokenCleanupOptions> options)
{
    public async Task<NexArrProcessServiceTokenCleanupResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/internal/service-token-cleanup/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new NexArrProcessServiceTokenCleanupResult(
            payload?.CandidatesFound ?? 0,
            payload?.PurgedCount ?? 0,
            payload?.ExpiredPurgeCount ?? 0,
            payload?.RevokedPurgeCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int CandidatesFound,
        int PurgedCount,
        int ExpiredPurgeCount,
        int RevokedPurgeCount,
        int SkippedCount);
}
