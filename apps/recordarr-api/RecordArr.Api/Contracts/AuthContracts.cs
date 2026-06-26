namespace RecordArr.Api.Contracts;

public sealed record RedeemHandoffRequest(string HandoffCode);

public sealed record HandoffSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    Guid SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> LaunchableProductKeys,
    string ThemePreference,
    string? CallbackUrl);
