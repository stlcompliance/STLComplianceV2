using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartStockLevel : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PartId { get; set; }

    public Guid InventoryBinId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public decimal QuantityReserved { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Part? Part { get; set; }

    public InventoryBin? InventoryBin { get; set; }
}
