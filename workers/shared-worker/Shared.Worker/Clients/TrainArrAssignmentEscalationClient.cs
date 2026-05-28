using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record TrainArrProcessAssignmentEscalationsResult(
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount);

public sealed class TrainArrAssignmentEscalationClient(
    HttpClient httpClient,
    IOptions<TrainArrAssignmentEscalationOptions> options)
{
    public async Task<TrainArrProcessAssignmentEscalationsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/assignment-escalations/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessAssignmentEscalationsPayload>(cancellationToken);
        return new TrainArrProcessAssignmentEscalationsResult(
            payload?.CandidatesFound ?? 0,
            payload?.EscalatedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessAssignmentEscalationsPayload(
        int CandidatesFound,
        int EscalatedCount,
        int SkippedCount);
}
