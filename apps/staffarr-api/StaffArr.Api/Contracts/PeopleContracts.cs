namespace StaffArr.Api.Contracts;

public sealed record StaffPersonSummaryResponse(
    Guid PersonId,
    Guid? ExternalUserId,
    string DisplayName,
    string PrimaryEmail,
    string EmploymentStatus,
    Guid? PrimaryOrgUnitId,
    string? PrimaryOrgUnitName,
    Guid? ManagerPersonId,
    string? JobTitle);

public sealed record StaffPersonDetailResponse(
    Guid PersonId,
    Guid? ExternalUserId,
    string GivenName,
    string FamilyName,
    string DisplayName,
    string PrimaryEmail,
    string EmploymentStatus,
    Guid? PrimaryOrgUnitId,
    string? PrimaryOrgUnitName,
    Guid? ManagerPersonId,
    string? JobTitle,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateStaffPersonRequest(
    string GivenName,
    string FamilyName,
    string PrimaryEmail,
    string EmploymentStatus,
    Guid? PrimaryOrgUnitId,
    Guid? ManagerPersonId,
    string? JobTitle);

public sealed record OrgUnitResponse(
    Guid OrgUnitId,
    string UnitType,
    string Name,
    Guid? ParentOrgUnitId,
    string Status);

public sealed record CreateOrgUnitRequest(
    string UnitType,
    string Name,
    Guid? ParentOrgUnitId);

public sealed record UpdateOrgUnitRequest(
    string UnitType,
    string Name,
    Guid? ParentOrgUnitId);

public sealed record UpdateOrgUnitStatusRequest(
    string Status);

public sealed record OrgUnitAssignmentResponse(
    Guid AssignmentId,
    Guid PersonId,
    Guid SiteOrgUnitId,
    Guid DepartmentOrgUnitId,
    Guid TeamOrgUnitId,
    Guid PositionOrgUnitId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateOrgUnitAssignmentRequest(
    Guid SiteOrgUnitId,
    Guid DepartmentOrgUnitId,
    Guid TeamOrgUnitId,
    Guid PositionOrgUnitId);

public sealed record UpdateOrgUnitAssignmentRequest(
    Guid SiteOrgUnitId,
    Guid DepartmentOrgUnitId,
    Guid TeamOrgUnitId,
    Guid PositionOrgUnitId);

public sealed record UpdateOrgUnitAssignmentStatusRequest(
    string Status);
