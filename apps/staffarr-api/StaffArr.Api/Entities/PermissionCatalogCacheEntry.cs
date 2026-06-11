using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PermissionCatalogCacheEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string CatalogVersion { get; set; } = string.Empty;

    public string CatalogJson { get; set; } = string.Empty;

    public DateTimeOffset FetchedAt { get; set; }

    public bool IsActive { get; set; }
}
