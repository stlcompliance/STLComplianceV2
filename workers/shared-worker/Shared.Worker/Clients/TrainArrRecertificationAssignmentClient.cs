using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record TrainArrProcessRecertificationResult(
    int CandidatesFound,
    int AssignedCount,
    int SkippedCount);

public sealed class TrainArrRecertificationAssignmentClient(
    HttpClient httpClient,
    IOptions<TrainArrRecertificationAssignmentOptions> options)
{
    public async Task<TrainArrProcessRecertificationResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/recertification/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessRecertificationPayload>(cancellationToken);
        return new TrainArrProcessRecertificationResult(
            payload?.CandidatesFound ?? 0,
            payload?.AssignedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessRecertificationPayload(
        int CandidatesFound,
        int AssignedCount,
        int SkippedCount);
}
