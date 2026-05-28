namespace SupplyArr.Api.Contracts;

public sealed record IntegrationEventSettingsResponse(
    Guid TenantId,
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes,
    DateTimeOffset UpdatedAt);

public sealed record UpsertIntegrationEventSettingsRequest(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes);

public sealed record IntegrationEventItemResponse(
    Guid EventId,
    string Direction,
    string EventKind,
    string IdempotencyKey,
    string? SourceProduct,
    string RelatedEntityType,
    string? RelatedEntityId,
    string ProcessingStatus,
    int AttemptCount,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);

public sealed record IntegrationEventsListResponse(
    IReadOnlyList<IntegrationEventItemResponse> Items);

public sealed record EnqueueIntegrationInboxRequest(
    Guid TenantId,
    string SourceProduct,
    string EventKind,
    string IdempotencyKey,
    string RelatedEntityType,
    string? RelatedEntityId,
    string PayloadJson,
    Guid? CorrelationId);

public sealed record EnqueueIntegrationInboxResponse(
    Guid? EventId,
    bool WasDuplicate);

public sealed record ProcessIntegrationEventsRequest(
    Guid? TenantId,
    int? BatchSize);

public sealed record ProcessIntegrationEventsResponse(
    Guid? TenantId,
    int OutboxProcessedCount,
    int InboxProcessedCount,
    int SkippedCount,
    int AbandonedCount,
    Guid? RunId);

public sealed record PendingIntegrationEventsResponse(
    int OutboxPendingCount,
    int InboxPendingCount,
    IReadOnlyList<IntegrationEventItemResponse> OutboxItems,
    IReadOnlyList<IntegrationEventItemResponse> InboxItems);

public sealed record AbandonIntegrationEventResponse(
    Guid EventId,
    string Direction,
    string ProcessingStatus);
