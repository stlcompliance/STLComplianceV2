using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed record SupplyArrReadinessBlockerPayload(
    string ReasonCode,
    string Message,
    string SourceEntityType,
    string SourceEntityId,
    string? RelatedEntityId);

public sealed record SupplyArrPartAvailabilityPayload(
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    decimal? ReorderPoint,
    int ActiveReservationCount,
    int OpenBackorderCount);

public sealed record SupplyArrPartReadinessPayload(
    Guid PartId,
    string PartKey,
    string DisplayName,
    string Status,
    string ReadinessStatus,
    string ReadinessBasis,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<SupplyArrReadinessBlockerPayload> Blockers,
    SupplyArrPartAvailabilityPayload Availability);

public sealed class SupplyArrSupplyReadinessClient(
    HttpClient httpClient,
    IOptions<SupplyArrClientOptions> options)
{
    public async Task<SupplyArrPartReadinessPayload> GetPartReadinessAsync(
        Guid tenantId,
        Guid partId,
        decimal? quantity = null,
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

        var query = $"tenantId={tenantId}&partId={partId}";
        if (quantity is not null)
        {
            query += $"&quantity={quantity.Value}";
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/integrations/part-supply-readiness?{query}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "supplyarr.readiness_lookup_failed",
                $"SupplyArr part readiness lookup failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<SupplyArrPartReadinessPayload>(cancellationToken))!;
    }
}
