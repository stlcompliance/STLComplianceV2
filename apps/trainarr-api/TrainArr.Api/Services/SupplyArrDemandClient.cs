using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed record SupplyArrIngestTrainarrDemandLinePayload(
    Guid TrainarrDemandLineId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record SupplyArrIngestTrainarrDemandPayload(
    Guid TenantId,
    Guid TrainarrPublicationId,
    Guid TrainarrAssignmentId,
    string TrainarrAssignmentRefKey,
    Guid StaffarrPersonId,
    string Title,
    string? Notes,
    bool CreatePurchaseRequestDraft,
    IReadOnlyList<SupplyArrIngestTrainarrDemandLinePayload> Lines);

public sealed record SupplyArrTrainarrDemandIntakeResult(
    Guid DemandRefId,
    string Status,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    bool IdempotentReplay);

public sealed class SupplyArrDemandClient(
    HttpClient httpClient,
    IOptions<SupplyArrClientOptions> options)
{
    public async Task<SupplyArrTrainarrDemandIntakeResult> PublishDemandAsync(
        SupplyArrIngestTrainarrDemandPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "supplyarr.service_token_missing",
                "TrainArr SupplyArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/trainarr-demand");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "supplyarr.demand_publication_failed",
                $"SupplyArr TrainArr demand intake failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<SupplyArrTrainarrDemandIntakeResult>(cancellationToken))!;
    }
}
