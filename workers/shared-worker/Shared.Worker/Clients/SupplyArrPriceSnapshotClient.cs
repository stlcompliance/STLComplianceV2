using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessPriceSnapshotResult(
    int CandidatesFound,
    int CapturedCount,
    int SkippedCount);

public sealed class SupplyArrPriceSnapshotClient(
    HttpClient httpClient,
    IOptions<SupplyArrPriceSnapshotOptions> options)
{
    public async Task<SupplyArrProcessPriceSnapshotResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/price-snapshots/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
            stalenessHours = settings.StalenessHours,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessPriceSnapshotPayload>(cancellationToken);
        return new SupplyArrProcessPriceSnapshotResult(
            payload?.CandidatesFound ?? 0,
            payload?.CapturedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessPriceSnapshotPayload(
        int CandidatesFound,
        int CapturedCount,
        int SkippedCount);
}
