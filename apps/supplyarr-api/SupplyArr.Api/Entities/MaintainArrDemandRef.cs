using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class MaintainArrDemandRef : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid MaintainarrPublicationId { get; set; }

    public Guid MaintainarrWorkOrderId { get; set; }

    public string MaintainarrWorkOrderNumber { get; set; } = string.Empty;

    public Guid MaintainarrAssetId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = MaintainArrDemandRefStatuses.Received;

    public string ProcurementStatus { get; set; } = MaintainArrDemandRefProcurementStatuses.Received;

    public Guid? PurchaseRequestId { get; set; }

    public Guid? PurchaseOrderId { get; set; }

    public DateTimeOffset? LastStatusCallbackAt { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PurchaseRequest? PurchaseRequest { get; set; }

    public ICollection<MaintainArrDemandRefLine> Lines { get; set; } = [];
}

public sealed class MaintainArrDemandRefLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid DemandRefId { get; set; }

    public int LineNumber { get; set; }

    public Guid MaintainarrDemandLineId { get; set; }

    public Guid? PartId { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal QuantityRequested { get; set; }

    public string UnitOfMeasure { get; set; } = "each";

    public string Notes { get; set; } = string.Empty;

    public MaintainArrDemandRef DemandRef { get; set; } = null!;

    public Part? Part { get; set; }
}

public static class MaintainArrDemandRefProcurementStatuses
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

public static class MaintainArrDemandRefStatuses
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
