using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class Backorder : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string BackorderKey { get; set; } = string.Empty;

    public string Status { get; set; } = BackorderStatuses.Open;

    public string SourceType { get; set; } = string.Empty;

    public Guid PurchaseOrderId { get; set; }

    public Guid PurchaseOrderLineId { get; set; }

    public Guid? PurchaseRequestId { get; set; }

    public Guid? PurchaseRequestLineId { get; set; }

    public Guid? ReceivingReceiptId { get; set; }

    public Guid? ReceivingReceiptLineId { get; set; }

    public Guid PartId { get; set; }

    public decimal QuantityBackordered { get; set; }

    public decimal QuantityFulfilled { get; set; }

    public DateTimeOffset? ExpectedBy { get; set; }

    public string Notes { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public Guid? FulfilledByUserId { get; set; }

    public DateTimeOffset? FulfilledAt { get; set; }

    public Guid? CancelledByUserId { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public string CancellationReason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public PurchaseOrderLine PurchaseOrderLine { get; set; } = null!;

    public Part Part { get; set; } = null!;
}
