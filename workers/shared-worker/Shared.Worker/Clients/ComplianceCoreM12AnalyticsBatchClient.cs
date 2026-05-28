using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record ComplianceCoreProcessM12AnalyticsBatchResult(
    int TenantsDueCount,
    int ProcessedCount,
    int SkippedCount);

public sealed class ComplianceCoreM12AnalyticsBatchClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreM12AnalyticsBatchOptions> options)
{
    public async Task<ComplianceCoreProcessM12AnalyticsBatchResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/m12-analytics-batches/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            intervalHours = settings.IntervalHours,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessM12AnalyticsBatchPayload>(cancellationToken);
        return new ComplianceCoreProcessM12AnalyticsBatchResult(
            payload?.TenantsDueCount ?? 0,
            payload?.ProcessedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessM12AnalyticsBatchPayload(
        int TenantsDueCount,
        int ProcessedCount,
        int SkippedCount);
}
