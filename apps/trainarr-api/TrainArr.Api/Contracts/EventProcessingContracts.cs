namespace TrainArr.Api.Contracts;

public sealed record EventProcessingSettingsResponse(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertEventProcessingSettingsRequest(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes);

public sealed record TrainingDomainEventItem(
    Guid EventId,
    string EventKind,
    string ProcessingStatus,
    Guid StaffarrPersonId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    int AttemptCount,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);

public sealed record TrainingDomainEventsResponse(
    IReadOnlyList<TrainingDomainEventItem> Items);

public sealed record PendingTrainingDomainEventItem(
    Guid EventId,
    Guid TenantId,
    string EventKind,
    Guid StaffarrPersonId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    int AttemptCount,
    DateTimeOffset? NextRetryAt,
    DateTimeOffset CreatedAt);

public sealed record PendingTrainingDomainEventsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingTrainingDomainEventItem> Items);

public sealed record ProcessTrainingDomainEventsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record TrainingDomainEventProcessResult(
    Guid EventId,
    string ProcessingStatus,
    int AttemptCount);

public sealed record TrainingDomainEventProcessSkip(
    Guid EventId,
    string Reason);

public sealed record ProcessTrainingDomainEventsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingFound,
    int ProcessedCount,
    int RetriedCount,
    int AbandonedCount,
    int SkippedCount,
    IReadOnlyList<TrainingDomainEventProcessResult> Results,
    IReadOnlyList<TrainingDomainEventProcessSkip> Skipped);

public sealed record PersonTrainingHistoryEntryItem(
    Guid EntryId,
    string EventKind,
    string Summary,
    string RelatedEntityType,
    Guid RelatedEntityId,
    DateTimeOffset OccurredAt);

public sealed record PersonTrainingHistoryResponse(
    Guid StaffarrPersonId,
    int TotalCount,
    IReadOnlyList<PersonTrainingHistoryEntryItem> Items);

public sealed record TrainingDomainEventPayload(
    Guid StaffarrPersonId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string Summary,
    DateTimeOffset OccurredAt,
    string? QualificationKey = null,
    string? QualificationName = null,
    string? TrainingDefinitionName = null,
    Guid? TrainingAssignmentId = null,
    Guid? QualificationIssueId = null);
