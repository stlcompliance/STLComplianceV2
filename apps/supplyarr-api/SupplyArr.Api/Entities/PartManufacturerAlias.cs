using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartManufacturerAlias : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PartId { get; set; }

    public string AliasKey { get; set; } = string.Empty;

    public string ManufacturerName { get; set; } = string.Empty;

    public string ManufacturerPartNumber { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Part Part { get; set; } = null!;
}
