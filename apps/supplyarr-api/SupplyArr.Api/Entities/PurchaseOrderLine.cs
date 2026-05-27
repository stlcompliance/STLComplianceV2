using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PurchaseOrderLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PurchaseOrderId { get; set; }

    public Guid? PurchaseRequestLineId { get; set; }

    public int LineNumber { get; set; }

    public Guid PartId { get; set; }

    public decimal QuantityOrdered { get; set; }

    public decimal QuantityReceived { get; set; }

    public string UnitOfMeasure { get; set; } = "each";

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public Part Part { get; set; } = null!;

    public PurchaseRequestLine? PurchaseRequestLine { get; set; }
}
