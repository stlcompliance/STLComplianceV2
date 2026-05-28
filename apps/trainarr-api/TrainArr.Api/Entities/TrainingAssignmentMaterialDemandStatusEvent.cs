using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingAssignmentMaterialDemandStatusEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainarrPublicationId { get; set; }

    public Guid SupplyarrDemandRefId { get; set; }

    public Guid SupplyarrCallbackPublicationId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string ProcurementStatus { get; set; } = string.Empty;

    public Guid? SupplyarrPurchaseRequestId { get; set; }

    public Guid? SupplyarrPurchaseOrderId { get; set; }

    public Guid? SupplyarrReceivingReceiptId { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class TrainingAssignmentMaterialDemandProcurementStatuses
{
    public const string AwaitingProcurement = "awaiting_procurement";

    public const string PrDrafted = "pr_drafted";

    public const string PrSubmitted = "pr_submitted";

    public const string PrApproved = "pr_approved";

    public const string PrRejected = "pr_rejected";

    public const string PoCreated = "po_created";

    public const string PoIssued = "po_issued";

    public const string PartiallyReceived = "partially_received";

    public const string ReceivedComplete = "received_complete";

    public const string Fulfilled = "fulfilled";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        AwaitingProcurement,
        PrDrafted,
        PrSubmitted,
        PrApproved,
        PrRejected,
        PoCreated,
        PoIssued,
        PartiallyReceived,
        ReceivedComplete,
        Fulfilled,
        Cancelled,
    };
}

public static class TrainingAssignmentMaterialDemandStatusEventTypes
{
    public const string PrDrafted = "pr_drafted";

    public const string PrSubmitted = "pr_submitted";

    public const string PrApproved = "pr_approved";

    public const string PrRejected = "pr_rejected";

    public const string PoCreated = "po_created";

    public const string PoIssued = "po_issued";

    public const string ReceivingPosted = "receiving_posted";

    public const string ReceivingComplete = "receiving_complete";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        PrDrafted,
        PrSubmitted,
        PrApproved,
        PrRejected,
        PoCreated,
        PoIssued,
        ReceivingPosted,
        ReceivingComplete,
    };
}

