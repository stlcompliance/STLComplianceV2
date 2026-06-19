using System.Net.Http.Json;
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

    public async Task<NexArrPlatformIdentityResult> CreateIdentityAsync(
        Guid tenantId,
        string email,
        string displayName,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new StlApiException(
                "people.login_provision_unavailable",
                "NexArr identity provisioning is not configured.",
                503);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/platform-identities");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.Value.ServiceToken);
        request.Content = JsonContent.Create(new NexArrPlatformIdentityCreatePayload(
            tenantId,
            email,
            displayName,
            "employee",
            password,
            true));

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

        return new NexArrPlatformIdentityResult(
            result.Identity.PersonId,
            result.WasCreated,
            result.MembershipWasCreated,
            result.Identity.CanLogin,
            result.Identity.RequiresPasswordChange);
    }

    private sealed record NexArrPlatformIdentityCreatePayload(
        Guid TenantId,
        string Email,
        string DisplayName,
        string? RoleKey,
        string Password,
        bool RequiresPasswordChange);

    private sealed record NexArrPlatformIdentityCreateResponse(
        bool WasCreated,
        bool MembershipWasCreated,
        NexArrPlatformIdentityResponse Identity);

    private sealed record NexArrPlatformIdentityResponse(
        Guid PersonId,
        string Email,
        string? SecondaryEmail,
        string? PhoneNumber,
        string? AvatarUrl,
        string DisplayName,
        bool IsActive,
        bool CanLogin,
        bool RequiresPasswordChange,
        bool LaunchEligible,
        string Status,
        bool IsPlatformAdmin,
        DateTimeOffset? LastLoginAt,
        DateTimeOffset? LastProductLaunchAt,
        DateTimeOffset CreatedAt,
        DateTimeOffset ModifiedAt,
        IReadOnlyList<NexArrPlatformIdentityTenantMembershipResponse> TenantMemberships);

    private sealed record NexArrPlatformIdentityTenantMembershipResponse(
        Guid TenantId,
        string RoleKey,
        bool IsActive);
}

public sealed record NexArrPlatformIdentityResult(
    Guid ExternalUserId,
    bool WasCreated,
    bool MembershipWasCreated,
    bool CanLogin,
    bool RequiresPasswordChange);
