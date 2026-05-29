using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record MaintainArrProcessTechnicianRefRefreshResult(
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    int FailedCount);

public sealed class MaintainArrTechnicianRefRefreshClient(
    HttpClient httpClient,
    IOptions<MaintainArrTechnicianRefRefreshOptions> options)
{
    public async Task<MaintainArrProcessTechnicianRefRefreshResult> ProcessRefreshAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/technician-refs/process-refresh");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
            staleAfter = TimeSpan.FromHours(Math.Clamp(settings.StaleAfterHours, 1, 24 * 14))
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessTechnicianRefRefreshPayload>(cancellationToken);
        return new MaintainArrProcessTechnicianRefRefreshResult(
            payload?.CandidatesFound ?? 0,
            payload?.RefreshedCount ?? 0,
            payload?.SkippedCount ?? 0,
            payload?.FailedCount ?? 0);
    }

    private sealed record ProcessTechnicianRefRefreshPayload(
        int CandidatesFound,
        int RefreshedCount,
        int SkippedCount,
        int FailedCount);
}
