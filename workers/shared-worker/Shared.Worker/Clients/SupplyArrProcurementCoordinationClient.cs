using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessProcurementCoordinationResult(
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount);

public sealed class SupplyArrProcurementCoordinationClient(
    HttpClient httpClient,
    IOptions<SupplyArrProcurementCoordinationOptions> options)
{
    public async Task<SupplyArrProcessProcurementCoordinationResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/procurement-coordination/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
            stalenessHours = settings.StalenessHours,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessProcurementCoordinationPayload>(cancellationToken);
        return new SupplyArrProcessProcurementCoordinationResult(
            payload?.CandidatesFound ?? 0,
            payload?.RefreshedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessProcurementCoordinationPayload(
        int CandidatesFound,
        int RefreshedCount,
        int SkippedCount);
}
