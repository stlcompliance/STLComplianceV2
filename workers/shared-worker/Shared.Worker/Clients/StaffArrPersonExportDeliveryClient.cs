using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record StaffArrProcessPersonExportDeliveriesResult(
    int CandidatesFound,
    int DeliveredCount,
    int SkippedCount);

public sealed class StaffArrPersonExportDeliveryClient(
    HttpClient httpClient,
    IOptions<StaffArrPersonExportDeliveryOptions> options)
{
    public async Task<StaffArrProcessPersonExportDeliveriesResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/person-export-deliveries/process-batch");
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
        return new StaffArrProcessPersonExportDeliveriesResult(
            payload?.CandidatesFound ?? 0,
            payload?.DeliveredCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessBatchPayload(
        int CandidatesFound,
        int DeliveredCount,
        int SkippedCount);
}
