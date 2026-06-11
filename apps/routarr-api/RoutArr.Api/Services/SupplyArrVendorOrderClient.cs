using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record SupplyArrVendorOrderReadResponse(
    Guid VendorOrderId,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    Guid VendorId,
    string VendorNameSnapshot,
    string? PickupLocationNameSnapshot,
    string PickupAddressSnapshot,
    string? DeliveryLocationNameSnapshot,
    string? DeliveryAddressSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    decimal QuantityReady,
    decimal QuantityRemaining,
    string QuantityUom,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? PickupInstructions,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed class SupplyArrVendorOrderClient(
    HttpClient httpClient,
    IOptions<SupplyArrClientOptions> options)
{
    public async Task<SupplyArrVendorOrderReadResponse> GetVendorOrderAsync(
        Guid tenantId,
        Guid vendorOrderId,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "supplyarr.vendor_order_service_token_missing",
                "RoutArr SupplyArr vendor-order service token is not configured.",
                500);
        }

        var url = $"api/v1/integrations/vendor-orders/{vendorOrderId}?tenantId={tenantId:D}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "supplyarr.vendor_order_read_failed",
                $"SupplyArr vendor-order read failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<SupplyArrVendorOrderReadResponse>(cancellationToken))!;
    }
}
