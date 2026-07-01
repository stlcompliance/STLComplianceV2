using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record SupplyArrSupplierOrderReadResponse(
    Guid SupplierOrderId,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    Guid SupplierId,
    string SupplierNameSnapshot,
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

public sealed class SupplyArrSupplierOrderClient(
    HttpClient httpClient,
    IOptions<SupplyArrClientOptions> options)
{
    public async Task<SupplyArrSupplierOrderReadResponse> GetSupplierOrderAsync(
        Guid tenantId,
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "supplyarr.supplier_order_service_token_missing",
                "RoutArr SupplyArr supplier-order service token is not configured.",
                500);
        }

        var url = $"api/v1/integrations/supplier-orders/{supplierOrderId}?tenantId={tenantId:D}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "supplyarr.supplier_order_read_failed",
                $"SupplyArr supplier-order read failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<SupplyArrSupplierOrderReadResponse>(cancellationToken))!;
    }
}
