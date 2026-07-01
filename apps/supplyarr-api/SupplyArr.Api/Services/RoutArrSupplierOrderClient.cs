using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class RoutArrSupplierOrderClient(
    HttpClient httpClient,
    IOptions<RoutArrClientOptions> options)
{
    public async Task PublishSupplierOrderEventAsync(
        SupplyArrSupplierOrderEventEnvelope payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.SupplierOrderEventServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            serviceToken = options.Value.ServiceToken;
        }

        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "routarr.supplier_order_service_token_missing",
                "SupplyArr RoutArr supplier-order service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/supplyarr-supplier-order-events");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "routarr.supplier_order_event_failed",
                $"RoutArr supplier-order event publish failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }
}
