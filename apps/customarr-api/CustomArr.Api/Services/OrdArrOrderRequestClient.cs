using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomArr.Api.Data;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace CustomArr.Api.Services;

public sealed class OrdArrOrderRequestClient(HttpClient httpClient)
{
    public async Task<CustomArrOrdArrOrderResponse> CreateOrderAsync(
        CustomArrPortalSubmissionResponse submission,
        string bearerToken,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/orders");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Content = JsonContent.Create(new CustomArrOrdArrCreateOrderRequest(
            submission.CustomerRef,
            submission.CustomerName,
            submission.RequestType,
            submission.OwnerPersonId,
            submission.Summary,
            submission.RequestedWindowStart,
            submission.RequestedWindowEnd,
            submission.PromisedWindowStart,
            submission.PromisedWindowEnd,
            submission.FulfillmentProductKeys));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "customarr.ordarr_order_request_failed",
                $"OrdArr order request failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<CustomArrOrdArrOrderResponse>(cancellationToken)
            ?? throw new StlApiException("customarr.ordarr_order_response_empty", "OrdArr returned an empty order response.", 502);
    }
}

public sealed record CustomArrOrdArrCreateOrderRequest(
    StlProductObjectReference CustomerRef,
    string CustomerName,
    string RequestType,
    string OwnerPersonId,
    string Summary,
    DateTimeOffset? RequestedWindowStart,
    DateTimeOffset? RequestedWindowEnd,
    DateTimeOffset? PromisedWindowStart,
    DateTimeOffset? PromisedWindowEnd,
    IReadOnlyList<string>? FulfillmentProductKeys);

public sealed record CustomArrOrdArrOrderResponse(
    string OrderId,
    string OrderNumber,
    string RequestType,
    string LifecycleStatus,
    StlProductObjectReference CustomerRef,
    string CustomerName,
    string OwnerPersonId,
    DateTimeOffset RequestedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RequestedWindowStart,
    DateTimeOffset? RequestedWindowEnd,
    DateTimeOffset? PromisedWindowStart,
    DateTimeOffset? PromisedWindowEnd,
    string HandoffState,
    string CompletionState,
    string FinancialPacketState,
    string Summary);
