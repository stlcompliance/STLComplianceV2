using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TenantProcurementNotificationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public string? NotificationWebhookUrl { get; set; }

    public bool NotifyOnPurchaseRequestSubmitted { get; set; } = true;

    public bool NotifyOnPurchaseRequestApproved { get; set; } = true;

    public bool NotifyOnPurchaseOrderIssued { get; set; } = true;

    public bool NotifyOnReceivingReceiptPosted { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ProcurementNotificationDispatch : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public Guid? VendorPartyId { get; set; }

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string DispatchStatus { get; set; } = string.Empty;

    public string? WebhookHost { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? DispatchedAt { get; set; }
}

public static class ProcurementNotificationEventKinds
{
    public const string PurchaseRequestSubmitted = "purchase_request_submitted";

    public const string PurchaseRequestApproved = "purchase_request_approved";

    public const string PurchaseOrderIssued = "purchase_order_issued";

    public const string ReceivingReceiptPosted = "receiving_receipt_posted";

    public const string PurchaseRequestApprovalReminder = "purchase_request_approval_reminder";

    public const string PurchaseOrderApprovalReminder = "purchase_order_approval_reminder";
}

public static class ProcurementNotificationDispatchStatuses
{
    public const string Pending = "pending";

    public const string Sent = "sent";

    public const string Failed = "failed";

    public const string Skipped = "skipped";
}
