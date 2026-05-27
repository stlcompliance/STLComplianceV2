using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record StaffArrProcessExpirationsResult(
    int CandidatesFound,
    int ExpiredCount,
    int SkippedCount);

public sealed class StaffArrCertificationExpirationClient(
    HttpClient httpClient,
    IOptions<StaffArrCertificationExpirationOptions> options)
{
    public async Task<StaffArrProcessExpirationsResult> ProcessExpirationsAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/certifications/process-expirations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessExpirationsPayload>(cancellationToken);
        return new StaffArrProcessExpirationsResult(
            payload?.CandidatesFound ?? 0,
            payload?.ExpiredCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessExpirationsPayload(
        int CandidatesFound,
        int ExpiredCount,
        int SkippedCount);
}
