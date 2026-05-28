namespace MaintainArr.Api.Contracts;

public sealed record MaintenanceNotificationSettingsResponse(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnWorkOrderCreated,
    bool NotifyOnPmScheduleDue,
    bool NotifyOnPmScheduleOverdue,
    bool NotifyOnDefectEscalated,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertMaintenanceNotificationSettingsRequest(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnWorkOrderCreated,
    bool NotifyOnPmScheduleDue,
    bool NotifyOnPmScheduleOverdue,
    bool NotifyOnDefectEscalated);

public sealed record MaintenanceNotificationDispatchItem(
    Guid NotificationId,
    string EventKind,
    string DispatchStatus,
    Guid AssetId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string? WebhookHost,
    int? HttpStatusCode,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DispatchedAt);

public sealed record MaintenanceNotificationDispatchesResponse(
    IReadOnlyList<MaintenanceNotificationDispatchItem> Items);

public sealed record PendingMaintenanceNotificationItem(
    Guid NotificationId,
    Guid TenantId,
    string EventKind,
    Guid AssetId,
    DateTimeOffset CreatedAt);

public sealed record PendingMaintenanceNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingMaintenanceNotificationItem> Items);

public sealed record ProcessMaintenanceNotificationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record MaintenanceNotificationDispatchResult(
    Guid NotificationId,
    string DispatchStatus);

public sealed record MaintenanceNotificationDispatchSkip(
    Guid NotificationId,
    string Reason);

public sealed record ProcessMaintenanceNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int EnqueuedPmDueCount,
    int PendingFound,
    int DispatchedCount,
    int SkippedCount,
    IReadOnlyList<MaintenanceNotificationDispatchResult> Dispatches,
    IReadOnlyList<MaintenanceNotificationDispatchSkip> Skipped);
