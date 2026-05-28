using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record StaffArrProcessPersonnelHistoryResult(
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount);

public sealed class StaffArrPersonnelHistoryRollupClient(
    HttpClient httpClient,
    IOptions<StaffArrPersonnelHistoryRollupOptions> options)
{
    public async Task<StaffArrProcessPersonnelHistoryResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/personnel-history/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
            stalenessHours = settings.StalenessHours
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new StaffArrProcessPersonnelHistoryResult(
            payload?.CandidatesFound ?? 0,
            payload?.RefreshedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int CandidatesFound,
        int RefreshedCount,
        int SkippedCount);
}
