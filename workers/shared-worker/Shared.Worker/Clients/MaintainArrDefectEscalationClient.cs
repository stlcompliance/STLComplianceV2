using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record MaintainArrProcessDefectEscalationsResult(
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount);

public sealed class MaintainArrDefectEscalationClient(
    HttpClient httpClient,
    IOptions<MaintainArrDefectEscalationOptions> options)
{
    public async Task<MaintainArrProcessDefectEscalationsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/defect-escalation/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessDefectEscalationsPayload>(cancellationToken);
        return new MaintainArrProcessDefectEscalationsResult(
            payload?.CandidatesFound ?? 0,
            payload?.EscalatedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessDefectEscalationsPayload(
        int CandidatesFound,
        int EscalatedCount,
        int SkippedCount);
}
