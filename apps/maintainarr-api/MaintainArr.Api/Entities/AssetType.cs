using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class AssetType : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetClassId { get; set; }

    public string TypeKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public AssetClass AssetClass { get; set; } = null!;
}
