using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class InventoryLocation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string LocationKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string LocationType { get; set; } = "warehouse";

    public string AddressLine { get; set; } = string.Empty;

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<InventoryBin> Bins { get; set; } = new List<InventoryBin>();
}
