namespace NexArr.Api.Contracts;

public sealed record FieldCompanionNotificationSettingsResponse(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnHandoffRedeemed,
    bool NotifyOnFieldInboxRefreshed,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertFieldCompanionNotificationSettingsRequest(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnHandoffRedeemed,
    bool NotifyOnFieldInboxRefreshed);

public sealed record FieldCompanionNotificationDispatchItem(
    Guid NotificationId,
    string EventKind,
    string DispatchStatus,
    Guid? ActorUserId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string? WebhookHost,
    int? HttpStatusCode,
    string? ErrorMessage,
    int? PushDeliveredCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DispatchedAt);

public sealed record FieldCompanionNotificationDispatchesResponse(
    IReadOnlyList<FieldCompanionNotificationDispatchItem> Items);

public sealed record PendingFieldCompanionNotificationItem(
    Guid NotificationId,
    Guid TenantId,
    string EventKind,
    Guid? ActorUserId,
    DateTimeOffset CreatedAt);

public sealed record PendingFieldCompanionNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingFieldCompanionNotificationItem> Items);

public sealed record ProcessFieldCompanionNotificationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record FieldCompanionNotificationDispatchResult(
    Guid NotificationId,
    string DispatchStatus);

public sealed record FieldCompanionNotificationDispatchSkip(
    Guid NotificationId,
    string Reason);

public sealed record ProcessFieldCompanionNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingFound,
    int DispatchedCount,
    int SkippedCount,
    IReadOnlyList<FieldCompanionNotificationDispatchResult> Dispatches,
    IReadOnlyList<FieldCompanionNotificationDispatchSkip> Skipped);
