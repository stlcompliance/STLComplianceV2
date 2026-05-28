using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record RoutArrProcessAuditPackageGenerationJobsResult(
    int CandidatesFound,
    int CompletedCount,
    int FailedCount,
    int SkippedCount);

public sealed class RoutArrAuditPackageGenerationClient(
    HttpClient httpClient,
    IOptions<RoutArrAuditPackageGenerationOptions> options)
{
    public async Task<RoutArrProcessAuditPackageGenerationJobsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/audit-package-jobs/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new RoutArrProcessAuditPackageGenerationJobsResult(
            payload?.CandidatesFound ?? 0,
            payload?.CompletedCount ?? 0,
            payload?.FailedCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int CandidatesFound,
        int CompletedCount,
        int FailedCount,
        int SkippedCount);
}
