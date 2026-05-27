using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed record MaintainArrDemandStatusCallbackPayload(
    Guid TenantId,
    Guid MaintainarrPublicationId,
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

public sealed class MaintainArrDemandStatusClient(
    HttpClient httpClient,
    IOptions<MaintainArrClientOptions> options)
{
    public async Task PublishStatusAsync(
        MaintainArrDemandStatusCallbackPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "maintainarr.service_token_missing",
                "SupplyArr MaintainArr service token is not configured.",
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
                "maintainarr.demand_status_callback_failed",
                $"MaintainArr demand status callback failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }
}
