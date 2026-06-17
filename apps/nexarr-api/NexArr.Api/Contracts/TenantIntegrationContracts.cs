namespace NexArr.Api.Contracts;

public sealed record TenantIntegrationRouteResponse(
    string RouteKey,
    string Method,
    string Path,
    string Description);

public sealed record TenantIntegrationBrandResponse(
    string Mark,
    string AccentColor,
    string BackgroundColor,
    string TextColor,
    string WebsiteUrl,
    string AssetSourceUrl,
    string AssetSourceLabel,
    string UsageNote);

public sealed record TenantIntegrationProviderResponse(
    string ProviderKey,
    string DisplayName,
    string Category,
    TenantIntegrationBrandResponse Brand,
    string ConnectorFamily,
    string AuthType,
    string DefaultDirection,
    bool SupportsWriteback,
    bool RequiresManualMapping,
    IReadOnlyList<string> OwningProducts,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<TenantIntegrationRouteResponse> Routes);

public sealed record TenantIntegrationCatalogResponse(
    IReadOnlyList<TenantIntegrationProviderResponse> Providers);

public sealed record TenantIntegrationCredentialSummaryResponse(
    Guid CredentialId,
    string CredentialKind,
    string RedactedLabel,
    string EncryptionKeyId,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? LastValidatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TenantIntegrationHealthResponse(
    string Status,
    DateTimeOffset? CheckedAt,
    double? LatencyMs,
    string? ErrorCategory,
    string? ErrorMessage);

public sealed record TenantIntegrationSyncRunResponse(
    Guid SyncRunId,
    Guid TenantId,
    Guid ConnectionId,
    string ProviderKey,
    string Status,
    string Direction,
    string TriggeredBy,
    int AttemptCount,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? NextRetryAt,
    int SnapshotCount,
    int MappingCount,
    string? ErrorCategory,
    string? ErrorMessage,
    string DestinationProductsJson,
    string ResultSummaryJson);

public sealed record TenantIntegrationConnectionResponse(
    Guid ConnectionId,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string ProviderKey,
    string ProviderDisplayName,
    string Category,
    TenantIntegrationBrandResponse Brand,
    string Status,
    string SyncDirection,
    bool WritebacksEnabled,
    bool ManualMappingRequired,
    string ConfigurationJson,
    DateTimeOffset? LastSuccessfulSyncAt,
    DateTimeOffset? LastFailedSyncAt,
    string? LastErrorCategory,
    string? LastErrorMessage,
    TenantIntegrationCredentialSummaryResponse? Credential,
    TenantIntegrationHealthResponse? Health,
    TenantIntegrationSyncRunResponse? LatestSyncRun,
    IReadOnlyList<TenantIntegrationRouteResponse> Routes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertTenantIntegrationConnectionRequest(
    string? Status,
    string? SyncDirection,
    bool? WritebacksEnabled,
    bool? ManualMappingRequired,
    string? ConfigurationJson);

public sealed record UpsertTenantIntegrationCredentialRequest(
    string CredentialKind,
    string SecretLabel,
    IReadOnlyDictionary<string, string> Payload,
    DateTimeOffset? ExpiresAt);

public sealed record TriggerTenantIntegrationSyncRequest(
    string? IdempotencyKey,
    bool Force = false);

public sealed record TestTenantIntegrationConnectionResponse(
    Guid ConnectionId,
    string ProviderKey,
    string Status,
    string? ErrorCategory,
    string? ErrorMessage,
    double? LatencyMs,
    DateTimeOffset CheckedAt);

public sealed record TenantIntegrationMappingTemplateResponse(
    Guid MappingTemplateId,
    Guid TenantId,
    Guid ConnectionId,
    string ProviderKey,
    string TemplateName,
    string SourceEntityType,
    string TargetProductKey,
    string TargetEntityType,
    string MappingJson,
    bool IsActive,
    DateTimeOffset UpdatedAt);

public sealed record UpsertTenantIntegrationMappingTemplateRequest(
    string TemplateName,
    string SourceEntityType,
    string TargetProductKey,
    string TargetEntityType,
    string MappingJson,
    bool IsActive = true);

public sealed record TenantIntegrationExternalMappingResponse(
    Guid MappingId,
    Guid TenantId,
    Guid ConnectionId,
    string ProviderKey,
    string OwningProductKey,
    string StlEntityType,
    string StlEntityId,
    string ExternalEntityType,
    string ExternalId,
    string MappingStatus,
    string SyncDirection,
    DateTimeOffset? LastVerifiedAt,
    DateTimeOffset? LastSyncAt,
    string? LastError);

public sealed record UpsertTenantIntegrationExternalMappingRequest(
    string OwningProductKey,
    string StlEntityType,
    string StlEntityId,
    string ExternalEntityType,
    string ExternalId,
    string MappingStatus,
    string SyncDirection);

public sealed record TenantIntegrationIntakeAttemptResponse(
    Guid IntakeAttemptId,
    Guid? TenantId,
    Guid? ConnectionId,
    string ProviderKey,
    string IntakeKind,
    string IdempotencyKey,
    string Status,
    string SourceRoute,
    string PayloadHash,
    string? FileName,
    string? ContentType,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? ProcessedAt,
    string? ErrorCategory,
    string? ErrorMessage);

public sealed record ProcessTenantIntegrationSyncRequest(
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcessTenantIntegrationSyncResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int SucceededCount,
    int FailedCount,
    int NeedsReviewCount,
    int SourceUnavailableCount,
    int DeadLetterCount,
    int SkippedCount,
    IReadOnlyList<Guid> ProcessedSyncRunIds);
