namespace TrainArr.Api.Contracts;

public sealed record TrainingNotificationSettingsResponse(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnAssignmentCreated,
    bool NotifyOnAssignmentCompleted,
    bool NotifyOnQualificationExpiring,
    bool NotifyOnQualificationIssued,
    bool NotifyOnQualificationSuspended,
    bool NotifyOnQualificationRevoked,
    bool NotifyOnQualificationExpired,
    int ExpiringLeadDays,
    int MaxAttempts,
    int RetryIntervalMinutes,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertTrainingNotificationSettingsRequest(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnAssignmentCreated,
    bool NotifyOnAssignmentCompleted,
    bool NotifyOnQualificationExpiring,
    bool NotifyOnQualificationIssued,
    bool NotifyOnQualificationSuspended,
    bool NotifyOnQualificationRevoked,
    bool NotifyOnQualificationExpired,
    int ExpiringLeadDays,
    int MaxAttempts,
    int RetryIntervalMinutes);

public sealed record TrainingNotificationDispatchItem(
    Guid NotificationId,
    string EventKind,
    string DispatchStatus,
    Guid StaffarrPersonId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    int AttemptCount,
    string? WebhookHost,
    int? HttpStatusCode,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? NextRetryAt,
    DateTimeOffset? DispatchedAt);

public sealed record TrainingNotificationDispatchesResponse(
    IReadOnlyList<TrainingNotificationDispatchItem> Items);

public sealed record PendingTrainingNotificationItem(
    Guid NotificationId,
    Guid TenantId,
    string EventKind,
    Guid StaffarrPersonId,
    int AttemptCount,
    DateTimeOffset? NextRetryAt,
    DateTimeOffset CreatedAt);

public sealed record PendingTrainingNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingTrainingNotificationItem> Items);

public sealed record ProcessTrainingNotificationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record TrainingNotificationDispatchResult(
    Guid NotificationId,
    string DispatchStatus);

public sealed record TrainingNotificationDispatchSkip(
    Guid NotificationId,
    string Reason);

public sealed record ProcessTrainingNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int EnqueuedExpiringCount,
    int PendingFound,
    int DispatchedCount,
    int RetriedCount,
    int AbandonedCount,
    int SkippedCount,
    IReadOnlyList<TrainingNotificationDispatchResult> Dispatches,
    IReadOnlyList<TrainingNotificationDispatchSkip> Skipped);

