namespace NexArr.Api.Contracts;

public sealed record UpsertServiceTokenCleanupSettingsRequest(
    bool IsEnabled,
    int RetentionDaysAfterExpiry,
    int RetentionDaysAfterRevoke);

public sealed record ServiceTokenCleanupSettingsResponse(
    bool IsEnabled,
    int RetentionDaysAfterExpiry,
    int RetentionDaysAfterRevoke,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessServiceTokenCleanupRequest(
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record PendingServiceTokenCleanupItem(
    Guid TokenId,
    Guid ServiceClientId,
    string ClientKey,
    Guid? TenantId,
    string CleanupReason,
    DateTimeOffset EffectiveAt);

public sealed record PendingServiceTokenCleanupResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingServiceTokenCleanupItem> Items);

public sealed record ServiceTokenCleanupPurgeSkip(
    Guid TokenId,
    string Reason);

public sealed record ProcessServiceTokenCleanupResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int PurgedCount,
    int ExpiredPurgeCount,
    int RevokedPurgeCount,
    int SkippedCount,
    IReadOnlyList<Guid> PurgedTokenIds,
    IReadOnlyList<ServiceTokenCleanupPurgeSkip> Skipped);

public sealed record ServiceTokenCleanupRunItem(
    Guid RunId,
    string Outcome,
    int PurgedCount,
    int ExpiredPurgeCount,
    int RevokedPurgeCount,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record ServiceTokenCleanupRunsResponse(
    IReadOnlyList<ServiceTokenCleanupRunItem> Items);
