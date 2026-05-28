using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed record StaffArrProcurementApprovalAuthorityGrant(
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue,
    string RoleKey,
    string RoleName);

public sealed record StaffArrProcurementApprovalAuthorityPayload(
    Guid PersonId,
    Guid? ExternalUserId,
    DateTimeOffset ComputedAt,
    bool CanSubmitPurchaseRequests,
    bool CanApprovePurchaseRequests,
    bool CanIssuePurchaseOrders,
    decimal? MaxSubmitAmount,
    decimal? MaxApproveAmount,
    decimal? MaxIssueAmount,
    IReadOnlyList<Guid> OrgUnitScopeIds,
    IReadOnlyList<StaffArrProcurementApprovalAuthorityGrant> Grants);

public sealed class StaffArrProcurementApprovalAuthorityClient(
    HttpClient httpClient,
    IOptions<StaffArrClientOptions> options)
{
    public async Task<StaffArrProcurementApprovalAuthorityPayload> GetAuthorityAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "SupplyArr StaffArr service token is not configured.",
                500);
        }

        var query = $"?tenantId={tenantId}&personId={personId}";
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/integrations/procurement-approval-authority{query}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.procurement_approval_authority_failed",
                $"StaffArr procurement approval authority lookup failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await ReadPayloadAsync(response, cancellationToken);
    }

    public async Task<StaffArrProcurementApprovalAuthorityPayload> GetAuthorityByExternalUserIdAsync(
        Guid tenantId,
        Guid externalUserId,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "SupplyArr StaffArr service token is not configured.",
                500);
        }

        var query = $"?tenantId={tenantId}&externalUserId={externalUserId}";
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/integrations/procurement-approval-authority{query}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.procurement_approval_authority_failed",
                $"StaffArr procurement approval authority lookup failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await ReadPayloadAsync(response, cancellationToken);
    }

    private static async Task<StaffArrProcurementApprovalAuthorityPayload> ReadPayloadAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<StaffArrProcurementApprovalAuthorityPayload>(cancellationToken);
        return payload
            ?? throw new StlApiException(
                "staffarr.procurement_approval_authority_invalid",
                "StaffArr procurement approval authority response was empty.",
                502);
    }
}
