using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class InternalLocation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string LocationNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string LocationType { get; set; } = "other";

    public Guid? ParentLocationId { get; set; }

    public Guid? SiteOrgUnitId { get; set; }

    public string Status { get; set; } = "planned";

    public string AllowedProductUsage { get; set; } = "all";

    public DateTimeOffset? ArchivedAt { get; set; }

    public Guid? ArchivedByUserId { get; set; }

    public string? ArchiveReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public InternalLocation? ParentLocation { get; set; }

    public OrgUnit? SiteOrgUnit { get; set; }

    public StaffPerson? ArchivedByUser { get; set; }
}
