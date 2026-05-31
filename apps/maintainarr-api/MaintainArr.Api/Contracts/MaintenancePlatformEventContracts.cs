namespace MaintainArr.Api.Contracts;

public sealed record MaintenancePlatformEventSettingsResponse(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertMaintenancePlatformEventSettingsRequest(
    bool IsEnabled,
    int? MaxAttempts = null,
    int? RetryIntervalMinutes = null);

public sealed record MaintenancePlatformEventPayload(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string PreviousReadinessStatus,
    string ReadinessStatus,
    string PreviousLifecycleStatus,
    string LifecycleStatus,
    string ReadinessBasis,
    int BlockerCount,
    string? PrimaryBlockerMessage,
    DateTimeOffset OccurredAt,
    string Summary,
    string? TargetEntityType = null,
    Guid? TargetEntityId = null,
    string? EventResult = null,
    Guid? ActorUserId = null);

public sealed record PendingMaintenancePlatformOutboxItem(
    Guid Id,
    Guid TenantId,
    string EventKind,
    Guid RelatedEntityId,
    int AttemptCount,
    DateTimeOffset? NextRetryAt,
    DateTimeOffset CreatedAt);

public sealed record PendingMaintenancePlatformOutboxResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingMaintenancePlatformOutboxItem> Items);

public sealed record ProcessMaintenancePlatformEventsRequest(
    Guid? TenantId = null,
    DateTimeOffset? AsOfUtc = null,
    int? BatchSize = null);

public sealed record MaintenancePlatformEventProcessResult(
    Guid EventId,
    string ProcessingStatus,
    int AttemptCount);

public sealed record MaintenancePlatformEventProcessSkip(
    Guid EventId,
    string Reason);

public sealed record ProcessMaintenancePlatformEventsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingFound,
    int ProcessedCount,
    int RetriedCount,
    int AbandonedCount,
    int SkippedCount,
    IReadOnlyList<MaintenancePlatformEventProcessResult> Results,
    IReadOnlyList<MaintenancePlatformEventProcessSkip> Skipped);

public sealed record MaintenancePlatformOutboxEventItem(
    Guid Id,
    string EventKind,
    string ProcessingStatus,
    Guid RelatedEntityId,
    int AttemptCount,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);

public sealed record MaintenancePlatformOutboxEventsResponse(
    IReadOnlyList<MaintenancePlatformOutboxEventItem> Items);

public sealed record MaintenancePlatformEventProcessingRunItem(
    Guid Id,
    int PendingFound,
    int ProcessedCount,
    int RetriedCount,
    int AbandonedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record MaintenancePlatformEventProcessingRunsResponse(
    IReadOnlyList<MaintenancePlatformEventProcessingRunItem> Items);
