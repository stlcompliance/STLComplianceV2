namespace NexArr.Api.Contracts;

public sealed record PlatformOutboxPayload(
    int SchemaVersion,
    Guid? TenantId,
    Guid? ActorPersonId,
    string TargetType,
    string TargetId,
    string Summary,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record UpsertPlatformOutboxPublisherSettingsRequest(
    bool IsEnabled,
    int MaxRetryAttempts,
    int RetryIntervalMinutes);

public sealed record PlatformOutboxPublisherSettingsResponse(
    bool IsEnabled,
    int MaxRetryAttempts,
    int RetryIntervalMinutes,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessPlatformOutboxPublisherRequest(
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record PendingPlatformOutboxEventItem(
    Guid EventId,
    string EventType,
    Guid? TenantId,
    string ProcessingStatus,
    int AttemptCount,
    DateTimeOffset OccurredAt,
    DateTimeOffset? NextRetryAt);

public sealed record PendingPlatformOutboxPublisherResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingPlatformOutboxEventItem> Items);

public sealed record PlatformOutboxPublishSkip(
    Guid EventId,
    string Reason);

public sealed record ProcessPlatformOutboxPublisherResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int PublishedCount,
    int FailedCount,
    int DeadLetterCount,
    int SkippedCount,
    IReadOnlyList<Guid> PublishedEventIds,
    IReadOnlyList<PlatformOutboxPublishSkip> Skipped);

public sealed record PlatformOutboxPublisherRunItem(
    Guid RunId,
    string Outcome,
    int PublishedCount,
    int FailedCount,
    int DeadLetterCount,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record PlatformOutboxPublisherRunsResponse(
    IReadOnlyList<PlatformOutboxPublisherRunItem> Items);

public sealed record PlatformOutboxPublisherStatusResponse(
    DateTimeOffset AsOfUtc,
    bool IsEnabled,
    int PendingCount,
    int DeadLetterCount,
    PlatformOutboxPublisherRunItem? LatestRun);

public sealed record PlatformOutboxEventItemResponse(
    Guid EventId,
    string EventType,
    Guid? TenantId,
    string ProcessingStatus,
    int AttemptCount,
    string? ErrorMessage,
    DateTimeOffset OccurredAt,
    DateTimeOffset? PublishedAt);

public sealed record PlatformOutboxEventsListResponse(
    IReadOnlyList<PlatformOutboxEventItemResponse> Items);

public sealed record TriggerPlatformOutboxPublisherOrchestrationResponse(
    DateTimeOffset AsOfUtc,
    int PublishedCount,
    int FailedCount,
    int DeadLetterCount,
    int SkippedCount);
