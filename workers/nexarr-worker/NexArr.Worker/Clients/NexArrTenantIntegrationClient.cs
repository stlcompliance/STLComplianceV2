using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NexArr.Worker.Options;

namespace NexArr.Worker.Clients;

public sealed record NexArrProcessTenantIntegrationResult(
    int CandidatesFound,
    int SucceededCount,
    int FailedCount,
    int NeedsReviewCount,
    int SourceUnavailableCount,
    int DeadLetterCount,
    int SkippedCount);

public sealed class NexArrTenantIntegrationClient(
    HttpClient httpClient,
    IOptions<NexArrTenantIntegrationOptions> options)
{
    public async Task<NexArrProcessTenantIntegrationResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/internal/integrations/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new NexArrProcessTenantIntegrationResult(
            payload?.CandidatesFound ?? 0,
            payload?.SucceededCount ?? 0,
            payload?.FailedCount ?? 0,
            payload?.NeedsReviewCount ?? 0,
            payload?.SourceUnavailableCount ?? 0,
            payload?.DeadLetterCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int CandidatesFound,
        int SucceededCount,
        int FailedCount,
        int NeedsReviewCount,
        int SourceUnavailableCount,
        int DeadLetterCount,
        int SkippedCount);
}
