using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class ReceivingReceiptLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ReceivingReceiptId { get; set; }

    public Guid PurchaseOrderLineId { get; set; }

    public Guid PartId { get; set; }

    public int LineNumber { get; set; }

    public decimal QuantityExpected { get; set; }

    public decimal QuantityReceived { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ReceivingReceipt ReceivingReceipt { get; set; } = null!;

    public PurchaseOrderLine PurchaseOrderLine { get; set; } = null!;

    public Part Part { get; set; } = null!;

    public ICollection<ReceivingException> Exceptions { get; set; } = new List<ReceivingException>();
}
