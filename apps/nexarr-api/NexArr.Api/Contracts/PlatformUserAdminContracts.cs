namespace NexArr.Api.Contracts;

public sealed record PlatformUserEnableResponse(
    Guid UserId,
    bool WasAlreadyEnabled);

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
    DateTimeOffset ModifiedAt);
