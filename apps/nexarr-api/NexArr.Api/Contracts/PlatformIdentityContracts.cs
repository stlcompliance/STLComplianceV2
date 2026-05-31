namespace NexArr.Api.Contracts;

public sealed record PlatformIdentityTenantMembershipResponse(
    Guid TenantId,
    string RoleKey,
    bool IsActive);

public sealed record PlatformIdentityResponse(
    Guid PersonId,
    string Email,
    string DisplayName,
    bool IsActive,
    bool CanLogin,
    bool IsPlatformAdmin,
    DateTimeOffset? LastLoginAt,
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
