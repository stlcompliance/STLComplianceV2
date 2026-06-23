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
    bool IsMfaEnabled,
    bool RequiresPasswordChange,
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
    string? RoleKey = "employee",
    string? Password = null,
    bool RequiresPasswordChange = false,
    Guid? RequestedByUserId = null);

public sealed record CreatePlatformIdentityResponse(
    bool WasCreated,
    bool MembershipWasCreated,
    PlatformIdentityResponse Identity);

public sealed record SyncPlatformIdentityRequest(
    Guid TenantId,
    string DisplayName,
    string? RoleKey = "employee",
    string? Email = null,
    Guid? RequestedByUserId = null);

public sealed record RequestPlatformIdentityPasswordResetRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    Guid ExternalUserId,
    Guid? RequestedByUserId = null,
    string? Reason = null);

public sealed record RequestPlatformIdentityPasswordResetResponse(
    Guid ExternalUserId,
    string Message);

public sealed record ResetPlatformIdentityMfaRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    Guid ExternalUserId,
    Guid? RequestedByUserId = null,
    string? Reason = null);

public sealed record ResetPlatformIdentityMfaResponse(
    Guid ExternalUserId,
    bool WasMfaEnabled,
    DateTimeOffset UpdatedAt);
