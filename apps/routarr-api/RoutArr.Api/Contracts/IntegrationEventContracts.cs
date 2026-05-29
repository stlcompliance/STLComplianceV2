namespace RoutArr.Api.Contracts;

public sealed record IntegrationEventSettingsResponse(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertIntegrationEventSettingsRequest(
    bool IsEnabled,
    int? MaxAttempts,
    int? RetryIntervalMinutes);

public sealed record PendingIntegrationOutboxEventsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingIntegrationOutboxEventItem> Items);

public sealed record PendingIntegrationOutboxEventItem(
    Guid OutboxEventId,
    Guid TenantId,
    string EventKind,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string PayloadJson,
    Guid CorrelationId,
    DateTimeOffset CreatedAt);

public sealed record ProcessIntegrationOutboxEventsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcessIntegrationOutboxEventsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingCount,
    int ProcessedCount,
    int SkippedCount,
    int AbandonedCount,
    IReadOnlyList<IntegrationOutboxEventProcessResult> Results);

public sealed record IntegrationOutboxEventProcessResult(
    Guid OutboxEventId,
    string ProcessingStatus);

public sealed record IntegrationOutboxEventListResponse(
    IReadOnlyList<IntegrationOutboxEventListItem> Items);

public sealed record IntegrationOutboxEventListItem(
    Guid OutboxEventId,
    string EventKind,
    string ProcessingStatus,
    string RelatedEntityType,
    Guid RelatedEntityId,
    int AttemptCount,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);
