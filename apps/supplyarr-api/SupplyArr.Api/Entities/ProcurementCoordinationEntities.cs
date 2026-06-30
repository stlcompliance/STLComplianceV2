using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TenantProcurementCoordinationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int StalenessHours { get; set; } = ProcurementCoordinationDefaults.StalenessHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ProcurementCoordinationRecord : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SubjectType { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public string DocumentKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string CoordinationStage { get; set; } = string.Empty;

    public string NextActionRequired { get; set; } = string.Empty;

    public Guid? PurchaseRequestId { get; set; }

    public Guid? PurchaseOrderId { get; set; }

    public Guid? SupplierId { get; set; }

    public string? SupplierKey { get; set; }

    public string? SupplierDisplayName { get; set; }

    public Guid? ParentSupplierId { get; set; }

    public string? ParentSupplierDisplayName { get; set; }

    public string? SupplierUnitKind { get; set; }

    public string SupplierServiceTypesJson { get; set; } = "[]";

    public Guid? VendorPartyId { get; set; }

    public string VendorDisplayName { get; set; } = string.Empty;

    public string DocumentStatus { get; set; } = string.Empty;

    public int LineCount { get; set; }

    public decimal QuantityOrdered { get; set; }

    public decimal QuantityReceived { get; set; }

    public int? ReceiptProgressPercent { get; set; }

    public bool IsTerminal { get; set; }

    public DateTimeOffset SourceUpdatedAt { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ProcurementCoordinationEvent> Events { get; set; } = [];
}

public sealed class ProcurementCoordinationEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid CoordinationRecordId { get; set; }

    public string SubjectType { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public int SequenceNumber { get; set; }

    public string SourceEntityType { get; set; } = string.Empty;

    public string SourceEntityId { get; set; } = string.Empty;

    public ProcurementCoordinationRecord CoordinationRecord { get; set; } = null!;
}

public sealed class ProcurementCoordinationRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int RefreshedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class ProcurementCoordinationDefaults
{
    public const int StalenessHours = 2;
}

public static class ProcurementCoordinationSubjectTypes
{
    public const string PurchaseRequest = "purchase_request";
    public const string PurchaseOrder = "purchase_order";
}

public static class ProcurementCoordinationStages
{
    public const string AwaitingPrApproval = "awaiting_pr_approval";
    public const string AwaitingPoCreation = "awaiting_po_creation";
    public const string PoAwaitingApproval = "po_awaiting_approval";
    public const string PoAwaitingIssue = "po_awaiting_issue";
    public const string AwaitingReceipt = "awaiting_receipt";
    public const string PartialReceipt = "partial_receipt";
    public const string Fulfilled = "fulfilled";
    public const string Cancelled = "cancelled";
    public const string Rejected = "rejected";
}

public static class ProcurementCoordinationEventKinds
{
    public const string PrSubmitted = "pr_submitted";
    public const string PrApproved = "pr_approved";
    public const string PrRejected = "pr_rejected";
    public const string PoCreated = "po_created";
    public const string PoApproved = "po_approved";
    public const string PoIssued = "po_issued";
    public const string PoCancelled = "po_cancelled";
    public const string ReceiptProgress = "receipt_progress";
    public const string ReceiptComplete = "receipt_complete";
}
