using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed record RoutArrCreateShipmentLinePayload(
    Guid SupplyarrShipmentLineId,
    Guid PartId,
    string PartDisplayName,
    decimal Quantity);

public sealed record RoutArrCreateShipmentPayload(
    Guid TenantId,
    Guid SupplyarrShipmentId,
    string ShipmentKey,
    string DestinationName,
    string DestinationAddressSnapshot,
    IReadOnlyList<RoutArrCreateShipmentLinePayload> Lines);

public sealed record RoutArrCreateShipmentResponse(
    Guid ShipmentIntentId,
    Guid? RouteId,
    string Status);

public sealed class RoutArrShipmentClient(
    HttpClient httpClient,
    IOptions<RoutArrClientOptions> options)
{
    public async Task<RoutArrCreateShipmentResponse> CreateShipmentAsync(
        RoutArrCreateShipmentPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ShipmentServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            serviceToken = options.Value.ServiceToken;
        }

        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "routarr.shipment_service_token_missing",
                "SupplyArr RoutArr shipment service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/supplyarr-shipments");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "routarr.shipment_create_failed",
                $"RoutArr shipment intent create failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<RoutArrCreateShipmentResponse>(cancellationToken))!;
    }
}
