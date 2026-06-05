using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class OrgUnitAssignment : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid SiteOrgUnitId { get; set; }

    public Guid DepartmentOrgUnitId { get; set; }

    public Guid TeamOrgUnitId { get; set; }

    public Guid PositionOrgUnitId { get; set; }

    public string Status { get; set; } = "active";

    public bool IsPrimary { get; set; }

    public DateTimeOffset EffectiveAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
