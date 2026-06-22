using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class Part : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? PartCatalogId { get; set; }

    public string PartKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CategoryKey { get; set; } = string.Empty;

    public string UnitOfMeasure { get; set; } = "each";

    public string ManufacturerName { get; set; } = string.Empty;

    public string ManufacturerPartNumber { get; set; } = string.Empty;

    public string Status { get; set; } = "active";

    public bool IsTrackable { get; set; } = true;

    public bool IsStocked { get; set; } = true;

    public bool RequiresSerialLotTracking { get; set; }

    public decimal? ReorderPoint { get; set; }

    public decimal? ReorderQuantity { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PartCatalog? PartCatalog { get; set; }

    public ICollection<PartManufacturerAlias> ManufacturerAliases { get; set; } = new List<PartManufacturerAlias>();

    public ICollection<PartSource> Sources { get; set; } = new List<PartSource>();

    public ICollection<PartVendorLink> VendorLinks { get; set; } = new List<PartVendorLink>();
}
