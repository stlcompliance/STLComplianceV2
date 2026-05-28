using System.Net.Http.Headers;
using System.Net.Http.Json;
using RoutArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record SupplyArrIngestRoutarrDemandLinePayload(
    Guid RoutarrDemandLineId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record SupplyArrIngestRoutarrDemandPayload(
    Guid TenantId,
    Guid RoutarrPublicationId,
    Guid RoutarrTripId,
    string RoutarrTripNumber,
    string RoutarrVehicleRefKey,
    string Title,
    string? Notes,
    bool CreatePurchaseRequestDraft,
    IReadOnlyList<SupplyArrIngestRoutarrDemandLinePayload> Lines);

public sealed record SupplyArrRoutarrDemandIntakeResult(
    Guid DemandRefId,
    string Status,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    bool IdempotentReplay);

public sealed class SupplyArrDemandClient(
    HttpClient httpClient,
    IOptions<SupplyArrClientOptions> options)
{
    public async Task<SupplyArrRoutarrDemandIntakeResult> PublishDemandAsync(
        SupplyArrIngestRoutarrDemandPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "supplyarr.service_token_missing",
                "RoutArr SupplyArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/routarr-demand");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "supplyarr.demand_publication_failed",
                $"SupplyArr RoutArr demand intake failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<SupplyArrRoutarrDemandIntakeResult>(cancellationToken))!;
    }
}
