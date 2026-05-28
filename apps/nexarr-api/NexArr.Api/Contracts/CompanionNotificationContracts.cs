namespace NexArr.Api.Contracts;

public sealed record CompanionNotificationSettingsResponse(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnHandoffRedeemed,
    bool NotifyOnFieldInboxRefreshed,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertCompanionNotificationSettingsRequest(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnHandoffRedeemed,
    bool NotifyOnFieldInboxRefreshed);

public sealed record CompanionNotificationDispatchItem(
    Guid NotificationId,
    string EventKind,
    string DispatchStatus,
    Guid? ActorUserId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string? WebhookHost,
    int? HttpStatusCode,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DispatchedAt);

public sealed record CompanionNotificationDispatchesResponse(
    IReadOnlyList<CompanionNotificationDispatchItem> Items);

public sealed record PendingCompanionNotificationItem(
    Guid NotificationId,
    Guid TenantId,
    string EventKind,
    Guid? ActorUserId,
    DateTimeOffset CreatedAt);

public sealed record PendingCompanionNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingCompanionNotificationItem> Items);

public sealed record ProcessCompanionNotificationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record CompanionNotificationDispatchResult(
    Guid NotificationId,
    string DispatchStatus);

public sealed record CompanionNotificationDispatchSkip(
    Guid NotificationId,
    string Reason);

public sealed record ProcessCompanionNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingFound,
    int DispatchedCount,
    int SkippedCount,
    IReadOnlyList<CompanionNotificationDispatchResult> Dispatches,
    IReadOnlyList<CompanionNotificationDispatchSkip> Skipped);
