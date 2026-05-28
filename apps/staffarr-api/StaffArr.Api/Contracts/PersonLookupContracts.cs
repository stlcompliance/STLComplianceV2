namespace StaffArr.Api.Contracts;

public sealed record PersonLookupOrgAssignmentResponse(
    Guid AssignmentId,
    Guid SiteOrgUnitId,
    string SiteName,
    Guid DepartmentOrgUnitId,
    string DepartmentName,
    Guid TeamOrgUnitId,
    string TeamName,
    Guid PositionOrgUnitId,
    string PositionName,
    string AssignmentPath);

public sealed record PersonLookupPlacementResponse(
    Guid? PrimaryOrgUnitId,
    string? PrimaryOrgUnitName,
    string? PrimaryOrgUnitType,
    Guid? ManagerPersonId,
    string? ManagerDisplayName,
    IReadOnlyList<PersonLookupOrgAssignmentResponse> ActiveAssignments);

public sealed record PersonLookupResponse(
    Guid PersonId,
    Guid? ExternalUserId,
    string GivenName,
    string FamilyName,
    string DisplayName,
    string PrimaryEmail,
    string EmploymentStatus,
    string? JobTitle,
    PersonLookupPlacementResponse Placement,
    DateTimeOffset LookedUpAt);
