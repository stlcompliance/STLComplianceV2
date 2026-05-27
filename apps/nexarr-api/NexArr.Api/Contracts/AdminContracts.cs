namespace NexArr.Api.Contracts;

public sealed record CreateTenantRequest(string Slug, string DisplayName);

public sealed record UpdateTenantRequest(string DisplayName);

public sealed record UpdateTenantStatusRequest(string Status);

public sealed record TenantDetailResponse(
    Guid TenantId,
    string Slug,
    string DisplayName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt);

public sealed record CreateProductRequest(
    string ProductKey,
    string DisplayName,
    int SortOrder,
    bool IsActive = true);

public sealed record UpdateProductRequest(
    string DisplayName,
    int SortOrder,
    bool IsActive);

public sealed record ProductDetailResponse(
    string ProductKey,
    string DisplayName,
    int SortOrder,
    bool IsActive);

public sealed record GrantEntitlementRequest(Guid TenantId, string ProductKey);

public sealed record EntitlementDetailResponse(
    Guid EntitlementId,
    Guid TenantId,
    string ProductKey,
    string ProductDisplayName,
    string Status,
    DateTimeOffset GrantedAt,
    DateTimeOffset? RevokedAt);

public sealed record RegisterServiceClientRequest(
    string ClientKey,
    string DisplayName,
    string SourceProductKey,
    IReadOnlyList<string> AllowedProductKeys);

public sealed record ServiceClientResponse(
    Guid ServiceClientId,
    string ClientKey,
    string DisplayName,
    string SourceProductKey,
    IReadOnlyList<string> AllowedProductKeys,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record IssueServiceTokenRequest(
    Guid ServiceClientId,
    Guid? TenantId,
    IReadOnlyList<string>? AllowedProductKeys,
    string? ActionScope,
    int? LifetimeMinutes);

public sealed record ServiceTokenIssueResponse(
    string AccessToken,
    Guid TokenId,
    DateTimeOffset ExpiresAt,
    Guid ServiceClientId,
    Guid? TenantId,
    IReadOnlyList<string> AllowedProductKeys,
    string? ActionScope);

public sealed record ValidateServiceTokenRequest(string Token);

public sealed record ServiceTokenValidationResponse(
    bool IsValid,
    Guid? TokenId,
    Guid? ServiceClientId,
    string? SourceProductKey,
    Guid? TenantId,
    IReadOnlyList<string> AllowedProductKeys,
    string? ActionScope,
    DateTimeOffset? ExpiresAt,
    string? ReasonCode);

public sealed record ServiceTokenSummaryResponse(
    Guid TokenId,
    Guid ServiceClientId,
    string ClientKey,
    Guid? TenantId,
    IReadOnlyList<string> AllowedProductKeys,
    string? ActionScope,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt);
