using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record MaintainArrProcessPmDueScanResult(
    int CandidatesFound,
    int MarkedDueCount,
    int MarkedOverdueCount,
    int SkippedCount,
    int WorkOrdersCreatedCount,
    int WorkOrdersLinkedCount);

public sealed class MaintainArrPmDueScanClient(
    HttpClient httpClient,
    IOptions<MaintainArrPmDueScanOptions> options)
{
    public async Task<MaintainArrProcessPmDueScanResult> ProcessDueScanAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/pm/process-due-scan");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
            overdueGraceDays = settings.OverdueGraceDays
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessPmDueScanPayload>(cancellationToken);
        return new MaintainArrProcessPmDueScanResult(
            payload?.CandidatesFound ?? 0,
            payload?.MarkedDueCount ?? 0,
            payload?.MarkedOverdueCount ?? 0,
            payload?.SkippedCount ?? 0,
            payload?.WorkOrdersCreatedCount ?? 0,
            payload?.WorkOrdersLinkedCount ?? 0);
    }

    private sealed record ProcessPmDueScanPayload(
        int CandidatesFound,
        int MarkedDueCount,
        int MarkedOverdueCount,
        int SkippedCount,
        int WorkOrdersCreatedCount,
        int WorkOrdersLinkedCount);
}
