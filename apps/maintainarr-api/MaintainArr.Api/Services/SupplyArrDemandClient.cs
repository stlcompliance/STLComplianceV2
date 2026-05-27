using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed record SupplyArrIngestDemandLinePayload(
    Guid MaintainarrDemandLineId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record SupplyArrIngestDemandPayload(
    Guid TenantId,
    Guid MaintainarrPublicationId,
    Guid MaintainarrWorkOrderId,
    string MaintainarrWorkOrderNumber,
    Guid MaintainarrAssetId,
    string Title,
    string? Notes,
    bool CreatePurchaseRequestDraft,
    IReadOnlyList<SupplyArrIngestDemandLinePayload> Lines);

public sealed record SupplyArrDemandIntakeResult(
    Guid DemandRefId,
    string Status,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    bool IdempotentReplay);

public sealed class SupplyArrDemandClient(
    HttpClient httpClient,
    IOptions<SupplyArrClientOptions> options)
{
    public async Task<SupplyArrDemandIntakeResult> PublishDemandAsync(
        SupplyArrIngestDemandPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "supplyarr.service_token_missing",
                "MaintainArr SupplyArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/maintainarr-demand");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "supplyarr.demand_publication_failed",
                $"SupplyArr demand intake failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<SupplyArrDemandIntakeResult>(cancellationToken))!;
    }
}
