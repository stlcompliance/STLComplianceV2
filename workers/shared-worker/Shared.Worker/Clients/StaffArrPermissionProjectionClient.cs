using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record StaffArrProcessPermissionProjectionsResult(
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount);

public sealed class StaffArrPermissionProjectionClient(
    HttpClient httpClient,
    IOptions<StaffArrPermissionProjectionOptions> options)
{
    public async Task<StaffArrProcessPermissionProjectionsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/permission-projections/process-batch");
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
        return new StaffArrProcessPermissionProjectionsResult(
            payload?.CandidatesFound ?? 0,
            payload?.RefreshedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int CandidatesFound,
        int RefreshedCount,
        int SkippedCount);
}
