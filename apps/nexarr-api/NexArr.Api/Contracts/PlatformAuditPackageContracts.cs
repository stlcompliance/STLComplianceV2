namespace NexArr.Api.Contracts;

public sealed record PlatformAuditPackageManifestResponse(
    string PackageVersion,
    IReadOnlyList<PlatformAuditPackageSectionDescriptor> Sections);

public sealed record PlatformAuditPackageSectionDescriptor(
    string Key,
    string FileName,
    string Label,
    string Description);

public sealed record PlatformAuditPackageExportResponse(
    Guid PackageId,
    Guid? ScopeTenantId,
    DateTimeOffset GeneratedAt,
    PlatformAuditPackageDateRangeResponse? DateRange,
    PlatformAuditPackageCountsResponse Counts,
    IReadOnlyList<PlatformAuditEventExportItem> AuditEvents,
    IReadOnlyList<PlatformAuditPackageTenantItem> Tenants,
    IReadOnlyList<PlatformAuditPackageEntitlementItem> TenantEntitlements,
    IReadOnlyList<PlatformAuditPackageProductItem> ProductCatalog,
    IReadOnlyList<PlatformAuditPackageUserItem> PlatformUsers,
    IReadOnlyList<PlatformAuditPackageServiceClientItem> ServiceClients,
    IReadOnlyList<PlatformAuditPackageServiceTokenItem> ServiceTokens,
    IReadOnlyList<PlatformAuditPackageLaunchProfileItem> LaunchProfiles,
    IReadOnlyList<PlatformAuditPackageCallbackAllowlistItem> CallbackAllowlist);

public sealed record PlatformAuditPackageDateRangeResponse(
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record PlatformAuditPackageCountsResponse(
    int AuditEvents,
    int Tenants,
    int TenantEntitlements,
    int ProductCatalog,
    int PlatformUsers,
    int ServiceClients,
    int ServiceTokens,
    int LaunchProfiles,
    int CallbackAllowlist);

public sealed record PlatformAuditEventExportItem(
    Guid AuditEventId,
    Guid? TenantId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record PlatformAuditPackageTenantItem(
    Guid TenantId,
    string Slug,
    string DisplayName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt);

public sealed record PlatformAuditPackageEntitlementItem(
    Guid EntitlementId,
    Guid TenantId,
    string ProductKey,
    string Status,
    DateTimeOffset GrantedAt,
    DateTimeOffset? RevokedAt);

public sealed record PlatformAuditPackageProductItem(
    string ProductKey,
    string DisplayName,
    bool IsActive,
    int SortOrder);

public sealed record PlatformAuditPackageUserItem(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsActive,
    bool IsPlatformAdmin,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt);

public sealed record PlatformAuditPackageServiceClientItem(
    Guid ServiceClientId,
    string ClientKey,
    string DisplayName,
    string SourceProductKey,
    string AllowedProductKeys,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt);

public sealed record PlatformAuditPackageServiceTokenItem(
    Guid ServiceTokenId,
    Guid ServiceClientId,
    Guid? TenantId,
    string Jti,
    string? ActionScope,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt);

public sealed record PlatformAuditPackageLaunchProfileItem(
    string ProductKey,
    string BaseUrl,
    string LaunchPath,
    bool IsActive);

public sealed record PlatformAuditPackageCallbackAllowlistItem(
    Guid EntryId,
    string ProductKey,
    Guid? TenantId,
    string UrlPattern,
    string PatternType,
    bool IsActive);
