namespace SupplyArr.Api.Contracts;

public sealed record ProcurementNotificationSettingsResponse(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnPurchaseRequestSubmitted,
    bool NotifyOnPurchaseRequestApproved,
    bool NotifyOnPurchaseOrderIssued,
    bool NotifyOnReceivingReceiptPosted,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertProcurementNotificationSettingsRequest(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnPurchaseRequestSubmitted,
    bool NotifyOnPurchaseRequestApproved,
    bool NotifyOnPurchaseOrderIssued,
    bool NotifyOnReceivingReceiptPosted);

public sealed record ProcurementNotificationDispatchItem(
    Guid NotificationId,
    string EventKind,
    string DispatchStatus,
    Guid? SupplierId,
    string? SupplierKey,
    string? SupplierDisplayName,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string? WebhookHost,
    int? HttpStatusCode,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DispatchedAt);

public sealed record ProcurementNotificationDispatchesResponse(
    IReadOnlyList<ProcurementNotificationDispatchItem> Items);

public sealed record PendingProcurementNotificationItem(
    Guid NotificationId,
    Guid TenantId,
    string EventKind,
    Guid? SupplierId,
    DateTimeOffset CreatedAt);

public sealed record PendingProcurementNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingProcurementNotificationItem> Items);

public sealed record ProcessProcurementNotificationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcurementNotificationDispatchResult(
    Guid NotificationId,
    string DispatchStatus);

public sealed record ProcurementNotificationDispatchSkip(
    Guid NotificationId,
    string Reason);

public sealed record ProcessProcurementNotificationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingFound,
    int DispatchedCount,
    int SkippedCount,
    IReadOnlyList<ProcurementNotificationDispatchResult> Dispatches,
    IReadOnlyList<ProcurementNotificationDispatchSkip> Skipped);
