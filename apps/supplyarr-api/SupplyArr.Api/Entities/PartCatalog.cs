using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartCatalog : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string CatalogKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Part> Parts { get; set; } = new List<Part>();
}
