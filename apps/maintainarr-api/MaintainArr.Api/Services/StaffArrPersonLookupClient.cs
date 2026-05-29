using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed record StaffArrIntegrationPersonLookupPlacement(
    Guid? PrimaryOrgUnitId,
    string? PrimaryOrgUnitName,
    string? PrimaryOrgUnitType,
    Guid? ManagerPersonId,
    string? ManagerDisplayName);

public sealed record StaffArrIntegrationPersonLookupResponse(
    Guid PersonId,
    Guid? ExternalUserId,
    string GivenName,
    string FamilyName,
    string DisplayName,
    string PrimaryEmail,
    string EmploymentStatus,
    string? JobTitle,
    StaffArrIntegrationPersonLookupPlacement Placement,
    DateTimeOffset LookedUpAt);

public sealed class StaffArrPersonLookupClient(
    HttpClient httpClient,
    IOptions<StaffArrClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<StaffArrIntegrationPersonLookupResponse?> TryLookupAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return null;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/integrations/person-lookup?tenantId={tenantId}&personId={personId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.person_lookup_failed",
                $"StaffArr person lookup failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<StaffArrIntegrationPersonLookupResponse>(cancellationToken);
    }
}
