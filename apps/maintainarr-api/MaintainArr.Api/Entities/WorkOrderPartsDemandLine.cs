using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class WorkOrderPartsDemandLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public int LineNumber { get; set; }

    public Guid? SupplyarrPartId { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal QuantityRequested { get; set; }

    public string UnitOfMeasure { get; set; } = "each";

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = WorkOrderPartsDemandStatuses.Pending;

    public Guid? MaintainarrPublicationId { get; set; }

    public Guid? SupplyarrDemandRefId { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public string ProcurementStatus { get; set; } = WorkOrderPartsDemandProcurementStatuses.AwaitingProcurement;

    public Guid? SupplyarrPurchaseRequestId { get; set; }

    public Guid? SupplyarrPurchaseOrderId { get; set; }

    public decimal QuantityReceived { get; set; }

    public string ProcurementStatusMessage { get; set; } = string.Empty;

    public DateTimeOffset? LastProcurementStatusAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;
}

public static class WorkOrderPartsDemandStatuses
{
    public const string Pending = "pending";

    public const string Published = "published";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Published,
        Cancelled,
    };
}
