using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class Asset : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetTypeId { get; set; }

    public string AssetTag { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string LifecycleStatus { get; set; } = "active";

    public string? SiteRef { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public AssetType AssetType { get; set; } = null!;
}
