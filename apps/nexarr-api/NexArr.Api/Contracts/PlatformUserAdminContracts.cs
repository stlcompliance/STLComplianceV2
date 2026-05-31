namespace NexArr.Api.Contracts;

public sealed record PlatformUserEnableResponse(
    Guid UserId,
    bool WasAlreadyEnabled);

public sealed record PlatformUserDisableResponse(
    Guid UserId,
    bool WasAlreadyDisabled);

public sealed record PlatformUserLockResponse(
    Guid UserId,
    bool WasAlreadyLocked,
    DateTimeOffset? LockedUntil);

public sealed record PlatformUserUnlockResponse(
    Guid UserId,
    bool WasAlreadyUnlocked);

public sealed record CreatePlatformUserRequest(
    string Email,
    string DisplayName,
    string Password,
    bool IsPlatformAdmin = false,
    bool IsActive = true,
    bool RequireEmailVerification = false);

public sealed record InvitePlatformUserRequest(
    string Email,
    string DisplayName,
    bool IsPlatformAdmin = false,
    bool IsActive = true);

public sealed record UpdatePlatformUserRequest(
    string Email,
    string DisplayName,
    bool IsPlatformAdmin);

public sealed record PlatformUserDetailResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsActive,
    bool IsPlatformAdmin,
    int FailedLoginCount,
    DateTimeOffset? LockedUntil,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt,
    DateTimeOffset? LastLoginAt = null,
    DateTimeOffset? LastProductLaunchAt = null,
    bool CanLogin = true,
    string Status = "active",
    bool IsMfaEnabled = false);

public sealed record AdminResetUserPasswordRequest(string NewPassword);

public sealed record AdminResetUserPasswordResponse(
    Guid UserId,
    DateTimeOffset PasswordChangedAt);

public sealed record PlatformUserListItemResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsActive,
    bool IsPlatformAdmin,
    int FailedLoginCount,
    DateTimeOffset? LockedUntil,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt,
    DateTimeOffset? LastLoginAt = null,
    DateTimeOffset? LastProductLaunchAt = null,
    bool CanLogin = true,
    string Status = "active",
    bool IsMfaEnabled = false);

public sealed record SetPlatformUserMfaRequest(bool IsEnabled);

public sealed record PlatformUserMfaResponse(
    Guid UserId,
    bool IsMfaEnabled,
    bool WasAlreadySet,
    DateTimeOffset ModifiedAt);

public sealed record PlatformUsersListResponse(
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<PlatformUserListItemResponse> Items);

public sealed record PlatformUserSessionItemResponse(
    Guid SessionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    string? UserAgent,
    string? IpAddress,
    Guid? ActiveTenantId,
    bool IsCurrent,
    bool IsActive,
    bool IsRemembered);

public sealed record PlatformUserSessionsResponse(
    Guid UserId,
    IReadOnlyList<PlatformUserSessionItemResponse> Sessions);

public sealed record PlatformUserSessionRevokeResponse(
    Guid UserId,
    Guid SessionId,
    bool WasAlreadyRevoked);

public sealed record PlatformUserTenantMembershipItemResponse(
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string RoleKey,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record PlatformUserTenantMembershipsResponse(
    Guid UserId,
    IReadOnlyList<PlatformUserTenantMembershipItemResponse> Items);

public sealed record AssignPlatformUserTenantMembershipRequest(
    Guid TenantId,
    string RoleKey = "tenant_user");

public sealed record AssignPlatformUserTenantMembershipResponse(
    Guid UserId,
    Guid TenantId,
    bool WasReactivated);

public sealed record RemovePlatformUserTenantMembershipResponse(
    Guid UserId,
    Guid TenantId,
    bool WasAlreadyRemoved);

public sealed record PlatformUserRoleItemResponse(
    string RoleKey,
    bool IsAssigned,
    Guid? TenantId = null);

public sealed record PlatformUserRolesResponse(
    Guid UserId,
    IReadOnlyList<PlatformUserRoleItemResponse> Items);

public sealed record AssignPlatformUserRoleRequest(
    string RoleKey,
    Guid? TenantId = null);

public sealed record AssignPlatformUserRoleResponse(
    Guid UserId,
    string RoleKey,
    bool WasAlreadyAssigned,
    Guid? TenantId = null);

public sealed record RemovePlatformUserRoleResponse(
    Guid UserId,
    string RoleKey,
    bool WasAlreadyRemoved,
    Guid? TenantId = null);

public sealed record PlatformUserExternalIdentityProviderMappingItemResponse(
    Guid MappingId,
    Guid UserId,
    string ProviderKey,
    string ExternalSubject,
    string? ExternalEmail,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt);

public sealed record PlatformUserExternalIdentityProviderMappingsResponse(
    Guid UserId,
    IReadOnlyList<PlatformUserExternalIdentityProviderMappingItemResponse> Items);

public sealed record UpsertPlatformUserExternalIdentityProviderMappingRequest(
    string ProviderKey,
    string ExternalSubject,
    string? ExternalEmail = null);

public sealed record UpsertPlatformUserExternalIdentityProviderMappingResponse(
    Guid MappingId,
    Guid UserId,
    string ProviderKey,
    string ExternalSubject,
    string? ExternalEmail,
    bool WasUpdated);

public sealed record RemovePlatformUserExternalIdentityProviderMappingResponse(
    Guid UserId,
    Guid MappingId,
    bool WasAlreadyRemoved);
