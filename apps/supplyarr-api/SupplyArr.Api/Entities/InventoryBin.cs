using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class InventoryBin : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid InventoryLocationId { get; set; }

    public string BinKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public InventoryLocation? InventoryLocation { get; set; }

    public ICollection<PartStockLevel> StockLevels { get; set; } = new List<PartStockLevel>();
}
