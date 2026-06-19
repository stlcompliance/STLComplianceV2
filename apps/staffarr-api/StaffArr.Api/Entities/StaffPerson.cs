using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class StaffPerson : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ExternalUserId { get; set; }

    public string GivenName { get; set; } = string.Empty;

    public string FamilyName { get; set; } = string.Empty;

    public string LegalFirstName { get; set; } = string.Empty;

    public string? LegalMiddleName { get; set; }

    public string LegalLastName { get; set; } = string.Empty;

    public string? PreferredName { get; set; }

    public string? Pronouns { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string PrimaryEmail { get; set; } = string.Empty;

    public string? AlternateEmail { get; set; }

    public string? PrimaryPhone { get; set; }

    public string? AlternatePhone { get; set; }

    public string EmploymentStatus { get; set; } = "active";

    public string? WorkRelationshipType { get; set; }

    public string? EmploymentType { get; set; }

    public string? WorkerCategory { get; set; }

    public string? FlsaStatus { get; set; }

    public string? PositionNumber { get; set; }

    public string? CurrentEmploymentAction { get; set; }

    public DateTimeOffset? CurrentEmploymentActionAt { get; set; }

    public string? LeaveStatus { get; set; }

    public bool EligibleForRehire { get; set; } = true;

    public Guid? PrimaryOrgUnitId { get; set; }

    public Guid? ManagerPersonId { get; set; }

    public string? JobTitle { get; set; }

    public string? WorkPhone { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? ExpectedStartDate { get; set; }

    public Guid? HomeBaseLocationId { get; set; }

    public bool CanLoginSnapshot { get; set; }

    public bool HasUserAccountSnapshot { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public OrgUnit? PrimaryOrgUnit { get; set; }

    public InternalLocation? HomeBaseLocation { get; set; }

    public StaffPerson? Manager { get; set; }
}
