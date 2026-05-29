using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessProcurementExceptionEscalationsResult(
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount);

public sealed class SupplyArrProcurementExceptionEscalationsClient(
    HttpClient httpClient,
    IOptions<SupplyArrProcurementExceptionEscalationsOptions> options)
{
    public async Task<SupplyArrProcessProcurementExceptionEscalationsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/internal/procurement-exception-escalations/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessProcurementExceptionEscalationsPayload>(
            cancellationToken);

        return new SupplyArrProcessProcurementExceptionEscalationsResult(
            payload?.CandidatesFound ?? 0,
            payload?.EscalatedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessProcurementExceptionEscalationsPayload(
        int CandidatesFound,
        int EscalatedCount,
        int SkippedCount);
}
