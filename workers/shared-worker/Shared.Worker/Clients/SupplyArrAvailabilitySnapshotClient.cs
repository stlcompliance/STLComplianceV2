using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessAvailabilitySnapshotResult(
    int CandidatesFound,
    int CapturedCount,
    int SkippedCount);

public sealed class SupplyArrAvailabilitySnapshotClient(
    HttpClient httpClient,
    IOptions<SupplyArrAvailabilitySnapshotOptions> options)
{
    public async Task<SupplyArrProcessAvailabilitySnapshotResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/availability-snapshots/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
            stalenessHours = settings.StalenessHours,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessAvailabilitySnapshotPayload>(cancellationToken);
        return new SupplyArrProcessAvailabilitySnapshotResult(
            payload?.CandidatesFound ?? 0,
            payload?.CapturedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessAvailabilitySnapshotPayload(
        int CandidatesFound,
        int CapturedCount,
        int SkippedCount);
}
