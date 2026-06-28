namespace TrainArr.Api.Contracts;



public sealed record RedeemHandoffRequest(string HandoffCode);



public sealed record HandoffSessionResponse(

    string AccessToken,

    DateTimeOffset ExpiresAt,

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



public sealed record TrainArrSessionBootstrapResponse(

    Guid UserId,

    Guid PersonId,

    Guid TenantId,

    Guid SessionId,

    string TenantRoleKey,

    bool IsPlatformAdmin,

    string ProductKey,

    IReadOnlyList<string> LaunchableProductKeys);



public sealed record TrainArrMeResponse(

    Guid UserId,

    Guid PersonId,

    string Email,

    string DisplayName,

    Guid TenantId,

    string TenantRoleKey,

    bool IsPlatformAdmin,

    string ProductKey,

    IReadOnlyList<string> LaunchableProductKeys);


