using System.Net.Http.Headers;
using System.Net.Http.Json;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.Shared.Integration;

public static class StaffArrLocationIntegrationScopes
{
    public const string LocationsRead = "staffarr.locations.read";
}

public sealed record StaffArrLocationLookupResponse(
    Guid LocationId,
    Guid TenantId,
    string LocationNumber,
    string Name,
    string LocationType,
    Guid? ParentLocationId,
    Guid? SiteOrgUnitId,
    string SiteNameSnapshot,
    string ParentPathSnapshot,
    string Status,
    string AllowedProductUsage = "all",
    string? Description = null,
    DateTimeOffset? ArchivedAt = null,
    Guid? ArchivedByUserId = null,
    string? ArchiveReason = null);

public sealed class StaffArrLocationLookupClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<StaffArrLocationLookupResponse>> ListAsync(
        Guid tenantId,
        string serviceToken,
        Guid? siteOrgUnitId = null,
        string? search = null,
        string? type = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        using var request = BuildRequest(
            BuildListPath(tenantId, siteOrgUnitId, search, type, includeArchived),
            serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowFailureAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<StaffArrLocationLookupResponse>>(cancellationToken)
            ?? [];
    }

    private static string BuildListPath(
        Guid tenantId,
        Guid? siteOrgUnitId,
        string? search,
        string? type,
        bool includeArchived)
    {
        var query = new List<string>
        {
            $"tenantId={tenantId:D}"
        };

        if (siteOrgUnitId is Guid siteId)
        {
            query.Add($"siteOrgUnitId={siteId:D}");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query.Add($"type={Uri.EscapeDataString(type.Trim())}");
        }

        if (includeArchived)
        {
            query.Add("includeArchived=true");
        }

        return $"api/v1/integrations/locations?{string.Join("&", query)}";
    }

    private static HttpRequestMessage BuildRequest(string path, string serviceToken)
    {
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "StaffArr service token is not configured.",
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
            "staffarr.locations.lookup_failed",
            $"StaffArr location lookup failed ({(int)response.StatusCode}): {body}",
            (int)response.StatusCode);
    }
}
