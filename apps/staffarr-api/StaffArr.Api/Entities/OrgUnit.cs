using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class OrgUnit : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string UnitType { get; set; } = "department";

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? Description { get; set; }

    public Guid? ParentOrgUnitId { get; set; }

    public Guid? ManagerPersonId { get; set; }

    public DateTimeOffset? EffectiveStartDate { get; set; }

    public DateTimeOffset? EffectiveEndDate { get; set; }

    public string Status { get; set; } = "planned";

    public string? SiteType { get; set; }

    public string? Timezone { get; set; }

    public string? Phone { get; set; }

    public string? EmergencyContact { get; set; }

    public string? TeamType { get; set; }

    public string? PositionCode { get; set; }

    public Guid? DefaultSiteOrgUnitId { get; set; }

    public bool ComplianceSensitive { get; set; }

    public bool SafetySensitive { get; set; }

    public bool CanSupervise { get; set; }

    public bool CanApprove { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }

    public Guid? ArchivedByUserId { get; set; }

    public string? ArchiveReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public OrgUnit? ParentOrgUnit { get; set; }

    public StaffPerson? ManagerPerson { get; set; }

    public OrgUnit? DefaultSiteOrgUnit { get; set; }

    public StaffPerson? ArchivedByUser { get; set; }
}
