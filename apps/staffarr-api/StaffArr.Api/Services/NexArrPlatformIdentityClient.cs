using System.Net.Http.Json;
using System.Linq;
using Microsoft.Extensions.Options;
using StaffArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class NexArrPlatformIdentityClient(
    HttpClient httpClient,
    IOptions<NexArrClientOptions> options)
{
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(options.Value.BaseUrl)
        && !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<NexArrPlatformIdentityResult> GetIdentityAsync(
        Guid tenantId,
        Guid externalUserId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured("NexArr platform identity lookup is not configured.");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/internal/platform-identities/{externalUserId}?tenantId={tenantId:D}");
        request.Headers.Authorization = CreateAuthorizationHeader();

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "people.account_lookup_failed",
                $"NexArr account lookup failed with {(int)response.StatusCode}: {body}",
                (int)response.StatusCode);
        }

        var identity = await response.Content.ReadFromJsonAsync<NexArrPlatformIdentityResponse>(cancellationToken: cancellationToken)
            ?? throw new StlApiException(
                "people.account_lookup_failed",
                "NexArr account lookup returned an empty response.",
                502);

        return Map(identity);
    }

    public async Task<NexArrPlatformIdentityResult> CreateIdentityAsync(
        Guid tenantId,
        string email,
        string displayName,
        string password,
        Guid? requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured("NexArr identity provisioning is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/platform-identities");
        request.Headers.Authorization = CreateAuthorizationHeader();
        request.Content = JsonContent.Create(new NexArrPlatformIdentityCreatePayload(
            tenantId,
            email,
            displayName,
            "employee",
            password,
            true,
            requestedByUserId));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "people.login_provision_failed",
                $"NexArr identity provisioning failed with {(int)response.StatusCode}: {body}",
                (int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<NexArrPlatformIdentityCreateResponse>(cancellationToken: cancellationToken)
            ?? throw new StlApiException(
                "people.login_provision_failed",
                "NexArr identity provisioning returned an empty response.",
                502);

        return Map(result.Identity);
    }

    public async Task<NexArrPlatformIdentityResult> SyncIdentityAsync(
        Guid tenantId,
        Guid externalUserId,
        string email,
        string displayName,
        string? roleKey,
        Guid? requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured("NexArr account update is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/internal/platform-identities/{externalUserId}");
        request.Headers.Authorization = CreateAuthorizationHeader();
        request.Content = JsonContent.Create(new NexArrPlatformIdentitySyncPayload(
            tenantId,
            displayName,
            roleKey,
            email,
            requestedByUserId));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "people.account_update_failed",
                $"NexArr account update failed with {(int)response.StatusCode}: {body}",
                (int)response.StatusCode);
        }

        var identity = await response.Content.ReadFromJsonAsync<NexArrPlatformIdentityResponse>(cancellationToken: cancellationToken)
            ?? throw new StlApiException(
                "people.account_update_failed",
                "NexArr account update returned an empty response.",
                502);

        return Map(identity);
    }

    public async Task<string> RequestPasswordResetAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        Guid externalUserId,
        Guid? requestedByUserId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured("NexArr password reset is not configured.");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/internal/platform-identities/{externalUserId}/password-reset");
        request.Headers.Authorization = CreateAuthorizationHeader();
        request.Content = JsonContent.Create(new NexArrPlatformIdentityPasswordResetPayload(
            tenantId,
            staffarrPersonId,
            externalUserId,
            requestedByUserId,
            reason));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "people.password_reset_failed",
                $"NexArr password reset failed with {(int)response.StatusCode}: {body}",
                (int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<NexArrPlatformIdentityPasswordResetResponse>(cancellationToken: cancellationToken)
            ?? throw new StlApiException(
                "people.password_reset_failed",
                "NexArr password reset returned an empty response.",
                502);

        return result.Message;
    }

    public async Task<bool> ResetMfaAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        Guid externalUserId,
        Guid? requestedByUserId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured("NexArr MFA reset is not configured.");

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/internal/platform-identities/{externalUserId}/mfa-reset");
        request.Headers.Authorization = CreateAuthorizationHeader();
        request.Content = JsonContent.Create(new NexArrPlatformIdentityMfaResetPayload(
            tenantId,
            staffarrPersonId,
            externalUserId,
            requestedByUserId,
            reason));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "people.mfa_reset_failed",
                $"NexArr MFA reset failed with {(int)response.StatusCode}: {body}",
                (int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<NexArrPlatformIdentityMfaResetResponse>(cancellationToken: cancellationToken)
            ?? throw new StlApiException(
                "people.mfa_reset_failed",
                "NexArr MFA reset returned an empty response.",
                502);

        return result.WasMfaEnabled;
    }

    private void EnsureConfigured(string message)
    {
        if (!IsConfigured)
        {
            throw new StlApiException("people.nexarr_integration_unavailable", message, 503);
        }
    }

    private System.Net.Http.Headers.AuthenticationHeaderValue CreateAuthorizationHeader() =>
        new("Bearer", options.Value.ServiceToken);

    private static NexArrPlatformIdentityResult Map(NexArrPlatformIdentityResponse identity) =>
        new(
            identity.PersonId,
            identity.Email,
            identity.DisplayName,
            identity.CanLogin,
            identity.IsMfaEnabled,
            identity.RequiresPasswordChange,
            identity.IsActive,
            identity.Status,
            identity.IsPlatformAdmin,
            identity.LastLoginAt,
            identity.LastProductLaunchAt,
            identity.MembershipRoleKey,
            identity.LaunchEligible);

    private sealed record NexArrPlatformIdentityCreatePayload(
        Guid TenantId,
        string Email,
        string DisplayName,
        string? RoleKey,
        string Password,
        bool RequiresPasswordChange,
        Guid? RequestedByUserId);

    private sealed record NexArrPlatformIdentitySyncPayload(
        Guid TenantId,
        string DisplayName,
        string? RoleKey,
        string? Email,
        Guid? RequestedByUserId);

    private sealed record NexArrPlatformIdentityPasswordResetPayload(
        Guid TenantId,
        Guid StaffarrPersonId,
        Guid ExternalUserId,
        Guid? RequestedByUserId,
        string? Reason);

    private sealed record NexArrPlatformIdentityMfaResetPayload(
        Guid TenantId,
        Guid StaffarrPersonId,
        Guid ExternalUserId,
        Guid? RequestedByUserId,
        string? Reason);

    private sealed record NexArrPlatformIdentityCreateResponse(
        bool WasCreated,
        bool MembershipWasCreated,
        NexArrPlatformIdentityResponse Identity);

    private sealed record NexArrPlatformIdentityPasswordResetResponse(
        Guid ExternalUserId,
        string Message);

    private sealed record NexArrPlatformIdentityMfaResetResponse(
        Guid ExternalUserId,
        bool WasMfaEnabled,
        DateTimeOffset UpdatedAt);

    private sealed record NexArrPlatformIdentityResponse(
        Guid PersonId,
        string Email,
        string? SecondaryEmail,
        string? PhoneNumber,
        string? AvatarUrl,
        string DisplayName,
        bool IsActive,
        bool CanLogin,
        bool IsMfaEnabled,
        bool RequiresPasswordChange,
        bool LaunchEligible,
        string Status,
        bool IsPlatformAdmin,
        DateTimeOffset? LastLoginAt,
        DateTimeOffset? LastProductLaunchAt,
        DateTimeOffset CreatedAt,
        DateTimeOffset ModifiedAt,
        IReadOnlyList<NexArrPlatformIdentityTenantMembershipResponse> TenantMemberships)
    {
        public string? MembershipRoleKey =>
            TenantMemberships.FirstOrDefault(x => x.IsActive)?.RoleKey;
    }

    private sealed record NexArrPlatformIdentityTenantMembershipResponse(
        Guid TenantId,
        string RoleKey,
        bool IsActive);
}

public sealed record NexArrPlatformIdentityResult(
    Guid ExternalUserId,
    string Email,
    string DisplayName,
    bool CanLogin,
    bool IsMfaEnabled,
    bool RequiresPasswordChange,
    bool IsActive,
    string Status,
    bool IsPlatformAdmin,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset? LastProductLaunchAt,
    string? MembershipRoleKey,
    bool LaunchEligible);
