namespace NexArr.Api.Contracts;

public sealed record PlatformIdentityTenantMembershipResponse(
    Guid TenantId,
    string RoleKey,
    bool IsActive);

public sealed record PlatformIdentityResponse(
    Guid PersonId,
    string Email,
    string? SecondaryEmail,
    string? PhoneNumber,
    string? AvatarUrl,
    string DisplayName,
    bool IsActive,
    bool CanLogin,
    bool LaunchEligible,
    string Status,
    bool IsPlatformAdmin,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset? LastProductLaunchAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt,
    IReadOnlyList<PlatformIdentityTenantMembershipResponse> TenantMemberships);

public sealed record CreatePlatformIdentityRequest(
    Guid TenantId,
    string Email,
    string DisplayName,
    string? RoleKey = "employee");

public sealed record CreatePlatformIdentityResponse(
    bool WasCreated,
    bool MembershipWasCreated,
    PlatformIdentityResponse Identity);

public sealed record SyncPlatformIdentityRequest(
    Guid TenantId,
    string DisplayName,
    string? RoleKey = "employee");
