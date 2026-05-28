using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StaffArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed record SupplyArrIngestStaffarrDemandLinePayload(
    Guid StaffarrDemandLineId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record SupplyArrIngestStaffarrDemandPayload(
    Guid TenantId,
    Guid StaffarrPublicationId,
    Guid StaffarrIncidentId,
    Guid StaffarrPersonId,
    string StaffarrIncidentTitle,
    string Title,
    string? Notes,
    bool CreatePurchaseRequestDraft,
    IReadOnlyList<SupplyArrIngestStaffarrDemandLinePayload> Lines);

public sealed record SupplyArrStaffarrDemandIntakeResult(
    Guid DemandRefId,
    string Status,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    bool IdempotentReplay);

public sealed class SupplyArrDemandClient(
    HttpClient httpClient,
    IOptions<SupplyArrClientOptions> options)
{
    public async Task<SupplyArrStaffarrDemandIntakeResult> PublishDemandAsync(
        SupplyArrIngestStaffarrDemandPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "supplyarr.service_token_missing",
                "StaffArr SupplyArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/staffarr-demand");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "supplyarr.demand_publication_failed",
                $"SupplyArr StaffArr demand intake failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<SupplyArrStaffarrDemandIntakeResult>(cancellationToken))!;
    }
}
