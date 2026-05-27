using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class ReceivingReceipt : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ReceiptKey { get; set; } = string.Empty;

    public Guid PurchaseOrderId { get; set; }

    public Guid InventoryBinId { get; set; }

    public string Status { get; set; } = ReceivingReceiptStatuses.Draft;

    public string Notes { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset? PostedAt { get; set; }

    public Guid? PostedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public InventoryBin InventoryBin { get; set; } = null!;

    public ICollection<ReceivingReceiptLine> Lines { get; set; } = new List<ReceivingReceiptLine>();
}
