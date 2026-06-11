using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class RoutArrVendorOrderClient(
    HttpClient httpClient,
    IOptions<RoutArrClientOptions> options)
{
    public async Task PublishVendorOrderEventAsync(
        SupplyArrVendorOrderEventEnvelope payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.VendorOrderEventServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            serviceToken = options.Value.ServiceToken;
        }

        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "routarr.vendor_order_service_token_missing",
                "SupplyArr RoutArr vendor-order service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/supplyarr-vendor-order-events");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "routarr.vendor_order_event_failed",
                $"RoutArr vendor-order event publish failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }
}
