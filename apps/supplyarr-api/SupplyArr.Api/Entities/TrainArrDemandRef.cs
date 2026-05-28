using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TrainArrDemandRef : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainarrPublicationId { get; set; }

    public Guid TrainarrAssignmentId { get; set; }

    public string TrainarrAssignmentRefKey { get; set; } = string.Empty;

    public Guid StaffarrPersonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = TrainArrDemandRefStatuses.Received;

    public string ProcurementStatus { get; set; } = TrainArrDemandRefProcurementStatuses.Received;

    public Guid? PurchaseRequestId { get; set; }

    public Guid? PurchaseOrderId { get; set; }

    public DateTimeOffset? LastStatusCallbackAt { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PurchaseRequest? PurchaseRequest { get; set; }

    public ICollection<TrainArrDemandRefLine> Lines { get; set; } = [];
}

public sealed class TrainArrDemandRefLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid DemandRefId { get; set; }

    public int LineNumber { get; set; }

    public Guid TrainarrDemandLineId { get; set; }

    public Guid? PartId { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal QuantityRequested { get; set; }

    public string UnitOfMeasure { get; set; } = "each";

    public string Notes { get; set; } = string.Empty;

    public TrainArrDemandRef DemandRef { get; set; } = null!;

    public Part? Part { get; set; }
}

public static class TrainArrDemandRefProcurementStatuses
{
    public const string Received = "received";

    public const string PrDrafted = "pr_drafted";

    public const string PrSubmitted = "pr_submitted";

    public const string PrApproved = "pr_approved";

    public const string PrRejected = "pr_rejected";

    public const string PoCreated = "po_created";

    public const string PoIssued = "po_issued";

    public const string PartiallyReceived = "partially_received";

    public const string ReceivedComplete = "received_complete";
}

public static class TrainArrDemandRefStatuses
{
    public const string Received = "received";

    public const string PrDrafted = "pr_drafted";

    public const string Fulfilled = "fulfilled";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Received,
        PrDrafted,
        Fulfilled,
        Cancelled,
    };
}
