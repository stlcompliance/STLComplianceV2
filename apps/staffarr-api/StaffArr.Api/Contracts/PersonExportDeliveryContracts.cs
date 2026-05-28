namespace StaffArr.Api.Contracts;

public sealed record PersonExportScheduleResponse(
    bool IsEnabled,
    int IntervalHours,
    DateTimeOffset? LastDeliveredAt,
    DateTimeOffset? UpdatedAt,
    string? NotificationWebhookUrl,
    bool NotifyOnSuccess,
    bool NotifyOnFailure);

public sealed record UpsertPersonExportScheduleRequest(
    bool IsEnabled,
    int IntervalHours,
    string? NotificationWebhookUrl,
    bool NotifyOnSuccess,
    bool NotifyOnFailure);

public sealed record PersonExportDeliveryNotificationItem(
    Guid NotificationId,
    Guid? DeliveryRunId,
    string EventKind,
    string DeliveryStatus,
    string? WebhookHost,
    int? HttpStatusCode,
    string? ErrorMessage,
    Guid? ExportId,
    int? PersonCount,
    DateTimeOffset AttemptedAt);

public sealed record PersonExportDeliveryNotificationsResponse(
    IReadOnlyList<PersonExportDeliveryNotificationItem> Items);

public sealed record PendingPersonExportDeliveryItem(
    Guid TenantId,
    int IntervalHours,
    DateTimeOffset? LastDeliveredAt);

public sealed record PendingPersonExportDeliveriesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingPersonExportDeliveryItem> Items);

public sealed record ProcessPersonExportDeliveriesRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record PersonExportDeliverySkip(
    Guid TenantId,
    string Reason);

public sealed record PersonExportDeliveryResult(
    Guid TenantId,
    Guid ExportId,
    int PersonCount,
    DateTimeOffset DeliveredAt);

public sealed record ProcessPersonExportDeliveriesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int DeliveredCount,
    int SkippedCount,
    IReadOnlyList<PersonExportDeliveryResult> Deliveries,
    IReadOnlyList<PersonExportDeliverySkip> Skipped);
