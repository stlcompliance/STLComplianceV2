using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record ComplianceCoreProcessFactSourceSyncsResult(
    int DueCount,
    int SucceededCount,
    int FailedCount,
    int SkippedCount);

public sealed class ComplianceCoreFactSourceSyncClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreFactSourceSyncOptions> options)
{
    public async Task<ComplianceCoreProcessFactSourceSyncsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/fact-source-sync/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            intervalMinutes = settings.IntervalMinutes,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessFactSourceSyncsPayload>(cancellationToken);
        return new ComplianceCoreProcessFactSourceSyncsResult(
            payload?.DueCount ?? 0,
            payload?.SucceededCount ?? 0,
            payload?.FailedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessFactSourceSyncsPayload(
        int DueCount,
        int SucceededCount,
        int FailedCount,
        int SkippedCount);
}
