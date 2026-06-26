namespace NexArr.Api.Contracts;

public sealed record FieldCompanionRedeemHandoffRequest(string HandoffCode);

public sealed record FieldCompanionSessionResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessExpiresAt,
    DateTimeOffset RefreshExpiresAt,
    Guid SessionId,
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> LaunchableProductKeys,
    string ThemePreference,
    string? CallbackUrl);

public sealed record FieldCompanionMeResponse(
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> LaunchableProductKeys,
    IReadOnlyList<string> FieldProductKeys);
