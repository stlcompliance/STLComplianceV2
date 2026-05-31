namespace RoutArr.Api.Contracts;

public sealed record DispatchNotificationSettingsResponse(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnTripAssigned,
    bool NotifyOnTripDispatched,
    bool NotifyOnTripAccepted,
    bool NotifyOnTripInProgress,
    bool NotifyOnTripCompleted,
    bool NotifyOnTripCancelled,
    bool NotifyOnDriverAssignmentChanged,
    bool NotifyOnRouteCancelled,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertDispatchNotificationSettingsRequest(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnTripAssigned,
    bool NotifyOnTripDispatched,
    bool NotifyOnTripInProgress,
    bool NotifyOnTripCompleted,
    bool NotifyOnTripCancelled,
    bool NotifyOnTripAccepted = true,
    bool NotifyOnDriverAssignmentChanged = true,
    bool NotifyOnRouteCancelled = true,
    bool ClearNotificationWebhookOnDisable = false);

public sealed record DispatchNotificationDispatchItem(
    Guid NotificationId,
    string EventKind,
    string DispatchStatus,
    Guid? TripId,
    Guid? RouteId,
    string? DriverPersonId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string? WebhookHost,
    int? HttpStatusCode,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DispatchedAt);

public sealed record DispatchNotificationDispatchesResponse(
    IReadOnlyList<DispatchNotificationDispatchItem> Items);

public sealed record PendingDispatchNotificationItem(
    Guid NotificationId,
    Guid TenantId,
    string EventKind,
    Guid? TripId,
    Guid? RouteId,
    DateTimeOffset CreatedAt);

public sealed record PendingDispatchNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingDispatchNotificationItem> Items);

public sealed record ProcessDispatchNotificationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record DispatchNotificationDispatchResult(
    Guid NotificationId,
    string DispatchStatus);

public sealed record DispatchNotificationDispatchSkip(
    Guid NotificationId,
    string Reason);

public sealed record ProcessDispatchNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingFound,
    int DispatchedCount,
    int SkippedCount,
    IReadOnlyList<DispatchNotificationDispatchResult> Dispatches,
    IReadOnlyList<DispatchNotificationDispatchSkip> Skipped);
