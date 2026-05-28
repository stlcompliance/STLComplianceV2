using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessDemandProcessingResult(
    int CandidatesFound,
    int ProcessedCount,
    int PrDraftsCreatedCount,
    int SkippedCount);

public sealed class SupplyArrDemandProcessingClient(
    HttpClient httpClient,
    IOptions<SupplyArrDemandProcessingOptions> options)
{
    public async Task<SupplyArrProcessDemandProcessingResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/demand-processing/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
            stalenessHours = settings.StalenessHours,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessDemandProcessingPayload>(cancellationToken);
        return new SupplyArrProcessDemandProcessingResult(
            payload?.CandidatesFound ?? 0,
            payload?.ProcessedCount ?? 0,
            payload?.PrDraftsCreatedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessDemandProcessingPayload(
        int CandidatesFound,
        int ProcessedCount,
        int PrDraftsCreatedCount,
        int SkippedCount);
}
