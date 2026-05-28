using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed record TrainArrDemandStatusCallbackPayload(
    Guid TenantId,
    Guid TrainarrPublicationId,
    Guid SupplyarrDemandRefId,
    Guid SupplyarrCallbackPublicationId,
    string EventType,
    string ProcurementStatus,
    Guid? SupplyarrPurchaseRequestId,
    Guid? SupplyarrPurchaseOrderId,
    Guid? SupplyarrReceivingReceiptId,
    decimal? QuantityReceivedDelta,
    string? Message,
    DateTimeOffset OccurredAt);

public sealed class TrainArrDemandStatusClient(
    HttpClient httpClient,
    IOptions<TrainArrClientOptions> options)
{
    public async Task PublishStatusAsync(
        TrainArrDemandStatusCallbackPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "trainarr.service_token_missing",
                "SupplyArr TrainArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/supplyarr-demand-status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "trainarr.demand_status_callback_failed",
                $"TrainArr demand status callback failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }
}
