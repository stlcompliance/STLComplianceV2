using System.Net.Http.Headers;
using System.Net.Http.Json;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.Shared.Integration;

public static class StaffArrSiteIntegrationScopes
{
    public const string SitesRead = "staffarr.sites.read";
}

public sealed record StaffArrSiteLookupResponse(
    Guid OrgUnitId,
    string Name,
    string? Code,
    Guid? ParentOrgUnitId,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed class StaffArrSiteLookupClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<StaffArrSiteLookupResponse>> ListAsync(
        Guid tenantId,
        string serviceToken,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        using var request = BuildRequest(
            HttpMethod.Get,
            $"api/v1/integrations/sites?tenantId={tenantId:D}{(includeArchived ? "&includeArchived=true" : string.Empty)}",
            serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowFailureAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<StaffArrSiteLookupResponse>>(cancellationToken)
            ?? [];
    }

    public async Task<StaffArrSiteLookupResponse?> GetAsync(
        Guid tenantId,
        Guid orgUnitId,
        string serviceToken,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        using var request = BuildRequest(
            HttpMethod.Get,
            $"api/v1/integrations/sites/{orgUnitId:D}?tenantId={tenantId:D}{(includeArchived ? "&includeArchived=true" : string.Empty)}",
            serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowFailureAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<StaffArrSiteLookupResponse>(cancellationToken);
    }

    private static HttpRequestMessage BuildRequest(HttpMethod method, string path, string serviceToken)
    {
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "StaffArr service token is not configured.",
                500);
        }

        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        return request;
    }

    private static async Task ThrowFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new StlApiException(
            "staffarr.sites.lookup_failed",
            $"StaffArr site lookup failed ({(int)response.StatusCode}): {body}",
            (int)response.StatusCode);
    }
}
