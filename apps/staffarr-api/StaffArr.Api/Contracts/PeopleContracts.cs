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

public sealed record UpdateStaffPersonRequest(
    string GivenName,
    string FamilyName,
    string PrimaryEmail,
    Guid? PrimaryOrgUnitId,
    Guid? ManagerPersonId,
    string? JobTitle);

public sealed record UpdatePersonEmploymentStatusRequest(
    string EmploymentStatus,
    string? Reason);

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

public sealed record UpdatePersonManagerRequest(
    Guid? ManagerPersonId);

public sealed record PersonManagerResponse(
    Guid PersonId,
    Guid? ManagerPersonId,
    string? ManagerDisplayName,
    DateTimeOffset UpdatedAt);

public sealed record ManagerChainEntryResponse(
    Guid PersonId,
    string DisplayName,
    string PrimaryEmail,
    string? JobTitle,
    string? PrimaryOrgUnitName,
    Guid? ManagerPersonId,
    int Level);

public sealed record SubordinateSummaryResponse(
    Guid PersonId,
    string DisplayName,
    string PrimaryEmail,
    string EmploymentStatus,
    string? JobTitle,
    string? PrimaryOrgUnitName,
    Guid? ManagerPersonId,
    string? ManagerDisplayName,
    int Depth,
    int DirectReportCount,
    string? ActiveAssignmentPath);

public sealed record PermissionTemplateSummaryResponse(
    Guid PermissionTemplateId,
    string PermissionKey,
    string Name,
    string? Description,
    string Status);

public sealed record RoleTemplatePermissionResponse(
    Guid MappingId,
    Guid PermissionTemplateId,
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue);

public sealed record RoleTemplateResponse(
    Guid RoleTemplateId,
    string RoleKey,
    string Name,
    string? Description,
    string Status,
    IReadOnlyList<RoleTemplatePermissionResponse> Permissions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertPermissionTemplateRequest(
    string PermissionKey,
    string Name,
    string? Description);

public sealed record RoleTemplatePermissionInput(
    Guid PermissionTemplateId,
    string ScopeType,
    string? ScopeValue);

public sealed record CreateRoleTemplateRequest(
    string RoleKey,
    string Name,
    string? Description,
    IReadOnlyList<RoleTemplatePermissionInput> Permissions);

public sealed record UpdateRoleTemplateRequest(
    string Name,
    string? Description,
    string Status,
    IReadOnlyList<RoleTemplatePermissionInput> Permissions);

public sealed record PersonRoleAssignmentResponse(
    Guid AssignmentId,
    Guid PersonId,
    Guid RoleTemplateId,
    string RoleKey,
    string RoleName,
    string ScopeType,
    string? ScopeValue,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePersonRoleAssignmentRequest(
    Guid RoleTemplateId,
    string ScopeType,
    string? ScopeValue);

public sealed record UpdatePersonRoleAssignmentStatusRequest(
    string Status);

public sealed record EffectivePermissionSourceResponse(
    Guid AssignmentId,
    Guid RoleTemplateId,
    string RoleKey,
    string RoleName,
    string AssignmentStatus,
    string AssignmentScopeType,
    string? AssignmentScopeValue,
    DateTimeOffset AssignedAt);

public sealed record EffectivePermissionResponse(
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue,
    IReadOnlyList<EffectivePermissionSourceResponse> Sources);

public sealed record EffectivePermissionProjectionResponse(
    Guid PersonId,
    DateTimeOffset ComputedAt,
    IReadOnlyList<EffectivePermissionResponse> Permissions);

public sealed record PermissionHistoryTimelineEntryResponse(
    Guid EventId,
    Guid PersonId,
    Guid AssignmentId,
    Guid RoleTemplateId,
    Guid PermissionTemplateId,
    Guid? ActorUserId,
    string EventType,
    string AssignmentStatus,
    string RoleKey,
    string RoleName,
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue,
    DateTimeOffset OccurredAt);
