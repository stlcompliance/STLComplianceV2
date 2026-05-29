using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record RoutArrProcessAttachmentRetentionResult(
    int CandidatesFound,
    int PurgedCount,
    long BytesReclaimed,
    int SkippedCount);

public sealed class RoutArrAttachmentRetentionClient(
    HttpClient httpClient,
    IOptions<RoutArrAttachmentRetentionOptions> options)
{
    public async Task<RoutArrProcessAttachmentRetentionResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/attachment-retention/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessAttachmentRetentionPayload>(cancellationToken);
        return new RoutArrProcessAttachmentRetentionResult(
            payload?.CandidatesFound ?? 0,
            payload?.PurgedCount ?? 0,
            payload?.BytesReclaimed ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessAttachmentRetentionPayload(
        int CandidatesFound,
        int PurgedCount,
        long BytesReclaimed,
        int SkippedCount);
}
