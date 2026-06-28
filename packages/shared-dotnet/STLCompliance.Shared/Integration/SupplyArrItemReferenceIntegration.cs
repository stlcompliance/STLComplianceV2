using System.Net.Http.Headers;
using System.Net.Http.Json;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.Shared.Integration;

public static class SupplyArrItemReferenceIntegrationScopes
{
    public const string ItemReferencesRead = "supplyarr.item_references.read";
}

public sealed record SupplyArrItemReferenceLookupResponse(
    Guid TenantId,
    string PartKey,
    string DisplayName,
    string UnitOfMeasure,
    string CategoryKey,
    string Status,
    bool RequiresSerialLotTracking,
    DateTimeOffset UpdatedAt);

public sealed class SupplyArrItemReferenceLookupClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<SupplyArrItemReferenceLookupResponse>> ListAsync(
        Guid tenantId,
        string serviceToken,
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        using var request = BuildRequest(BuildListPath(tenantId, query), serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowFailureAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<SupplyArrItemReferenceLookupResponse>>(cancellationToken)
            ?? [];
    }

    private static string BuildListPath(Guid tenantId, string? query)
    {
        var parameters = new List<string>
        {
            $"tenantId={tenantId:D}"
        };

        if (!string.IsNullOrWhiteSpace(query))
        {
            parameters.Add($"query={Uri.EscapeDataString(query.Trim())}");
        }

        return $"api/v1/integrations/item-references?{string.Join("&", parameters)}";
    }

    private static HttpRequestMessage BuildRequest(string path, string serviceToken)
    {
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "supplyarr.service_token_missing",
                "SupplyArr service token is not configured.",
                500);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        return request;
    }

    private static async Task ThrowFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new StlApiException(
            "supplyarr.item_references.lookup_failed",
            $"SupplyArr item reference lookup failed ({(int)response.StatusCode}): {body}",
            (int)response.StatusCode);
    }
}
