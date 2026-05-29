namespace NexArr.Api.Contracts;

public sealed record ProductManifestCallbackAllowlistResponse(
    Guid EntryId,
    Guid? TenantId,
    string UrlPattern,
    string PatternType,
    bool IsActive);

public sealed record ProductManifestDataPlaneProfileResponse(
    Guid ProfileId,
    Guid TenantId,
    string DeploymentMode,
    string TrustStatus,
    string? DataEndpointUrl);

public sealed record ProductManifestResponse(
    string ProductKey,
    string DisplayName,
    string ProductCategory,
    string ProductOwner,
    string ProductStatus,
    bool IsActive,
    string EnvironmentKey,
    string CanonicalCallbackPath,
    string? LaunchBaseUrl,
    string? LaunchPath,
    string? LaunchUrl,
    string ApiBaseUrl,
    string HealthUrl,
    string ServiceAudience,
    string MarketingUrl,
    string DocumentationUrl,
    string SupportUrl,
    string EntitlementDependencyRules,
    string ProductDependencyMetadata,
    DateTimeOffset? LaunchProfileModifiedAt,
    IReadOnlyList<ProductManifestCallbackAllowlistResponse> CallbackAllowlist,
    IReadOnlyList<ProductManifestDataPlaneProfileResponse> DataPlaneProfiles);
