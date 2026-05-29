using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record ComplianceCoreProcessExpiredWaiversResult(int ExpiredCount);

public sealed class ComplianceCoreWaiverExpirationClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreWaiverExpirationOptions> options)
{
    public async Task<ComplianceCoreProcessExpiredWaiversResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/waivers/expire-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessExpiredWaiversPayload>(cancellationToken);
        return new ComplianceCoreProcessExpiredWaiversResult(payload?.ExpiredCount ?? 0);
    }

    private sealed record ProcessExpiredWaiversPayload(int ExpiredCount);
}
