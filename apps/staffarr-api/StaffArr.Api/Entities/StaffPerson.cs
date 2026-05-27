using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class StaffPerson : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ExternalUserId { get; set; }

    public string GivenName { get; set; } = string.Empty;

    public string FamilyName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PrimaryEmail { get; set; } = string.Empty;

    public string EmploymentStatus { get; set; } = "active";

    public Guid? PrimaryOrgUnitId { get; set; }

    public Guid? ManagerPersonId { get; set; }

    public string? JobTitle { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public OrgUnit? PrimaryOrgUnit { get; set; }

    public StaffPerson? Manager { get; set; }
}
