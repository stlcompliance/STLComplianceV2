using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record NexArrSmartImportProcessResult(bool Processed, Guid? BatchId, string? Status);

public sealed class NexArrSmartImportClient(
    HttpClient httpClient,
    IOptions<NexArrSmartImportOptions> options)
{
    public async Task<NexArrSmartImportProcessResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/smart-import/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new NexArrSmartImportProcessResult(false, null, null);
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessBatchPayload>(cancellationToken);
        return new NexArrSmartImportProcessResult(true, payload?.Batch.BatchId, payload?.Batch.Status);
    }

    private sealed record ProcessBatchPayload(ProcessBatchSummary Batch);

    private sealed record ProcessBatchSummary(Guid BatchId, string Status);
}
