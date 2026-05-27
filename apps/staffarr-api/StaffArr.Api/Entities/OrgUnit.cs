using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class OrgUnit : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string UnitType { get; set; } = "department";

    public string Name { get; set; } = string.Empty;

    public Guid? ParentOrgUnitId { get; set; }

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public OrgUnit? ParentOrgUnit { get; set; }
}
