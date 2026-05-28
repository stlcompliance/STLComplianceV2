using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class ProcurementException : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ExceptionKey { get; set; } = string.Empty;

    public string SubjectType { get; set; } = ProcurementExceptionSubjectTypes.PurchaseRequest;

    public Guid SubjectId { get; set; }

    public string SubjectKey { get; set; } = string.Empty;

    public Guid? VendorPartyId { get; set; }

    public string ExceptionCategory { get; set; } = ProcurementExceptionCategories.Other;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = ProcurementExceptionStatuses.Open;

    public string ResolutionNotes { get; set; } = string.Empty;

    public string WaiveJustification { get; set; } = string.Empty;

    public string WaiveRejectionReason { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public Guid? InvestigatedByUserId { get; set; }

    public DateTimeOffset? InvestigatedAt { get; set; }

    public Guid? ResolvedByUserId { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public Guid? WaiveRequestedByUserId { get; set; }

    public DateTimeOffset? WaiveRequestedAt { get; set; }

    public Guid? WaivedByUserId { get; set; }

    public DateTimeOffset? WaivedAt { get; set; }

    public Guid? ClosedByUserId { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public Guid? CancelledByUserId { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public string CancellationReason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class ProcurementExceptionSubjectTypes
{
    public const string PurchaseRequest = "purchase_request";

    public const string PurchaseOrder = "purchase_order";

    public const string Rfq = "rfq";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        PurchaseRequest,
        PurchaseOrder,
        Rfq,
    };
}

public static class ProcurementExceptionCategories
{
    public const string ApprovalDelay = "approval_delay";

    public const string VendorIssue = "vendor_issue";

    public const string BudgetOverride = "budget_override";

    public const string PolicyViolation = "policy_violation";

    public const string PricingVariance = "pricing_variance";

    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ApprovalDelay,
        VendorIssue,
        BudgetOverride,
        PolicyViolation,
        PricingVariance,
        Other,
    };
}

public static class ProcurementExceptionStatuses
{
    public const string Open = "open";

    public const string Investigating = "investigating";

    public const string Resolved = "resolved";

    public const string WaivePending = "waive_pending";

    public const string Waived = "waived";

    public const string Closed = "closed";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> Active = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Investigating,
        Resolved,
        WaivePending,
        Waived,
    };

    public static readonly IReadOnlySet<string> Editable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Investigating,
    };
}
