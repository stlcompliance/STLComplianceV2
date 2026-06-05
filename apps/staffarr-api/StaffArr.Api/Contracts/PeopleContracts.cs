namespace StaffArr.Api.Contracts;

public sealed record StaffPersonSummaryResponse(
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
    string? PreferredName = null,
    string? WorkRelationshipType = null,
    string? EmploymentType = null,
    bool CanLoginSnapshot = false,
    bool HasUserAccountSnapshot = false);

public sealed record StaffPersonDetailResponse(
    Guid PersonId,
    Guid? ExternalUserId,
    string GivenName,
    string FamilyName,
    string LegalFirstName,
    string? LegalMiddleName,
    string LegalLastName,
    string? PreferredName,
    string? Pronouns,
    string DisplayName,
    string PrimaryEmail,
    string? AlternateEmail,
    string? PrimaryPhone,
    string? AlternatePhone,
    string? WorkPhone,
    string EmploymentStatus,
    string? WorkRelationshipType,
    string? EmploymentType,
    Guid? PrimaryOrgUnitId,
    string? PrimaryOrgUnitName,
    Guid? ManagerPersonId,
    string? JobTitle,
    DateTimeOffset? StartDate,
    DateTimeOffset? ExpectedStartDate,
    Guid? HomeBaseLocationId,
    string? HomeBaseLocationName,
    bool CanLoginSnapshot,
    bool HasUserAccountSnapshot,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateStaffPersonRequest(
    string PrimaryEmail,
    string? LegalFirstName = null,
    string? LegalMiddleName = null,
    string? LegalLastName = null,
    string? PreferredName = null,
    string? Pronouns = null,
    string? GivenName = null,
    string? FamilyName = null,
    string EmploymentStatus = "pending_start",
    string? WorkRelationshipType = null,
    string? EmploymentType = null,
    string? AlternateEmail = null,
    string? PrimaryPhone = null,
    string? AlternatePhone = null,
    string? WorkPhone = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? ExpectedStartDate = null,
    Guid? PrimaryOrgUnitId = null,
    Guid? SiteOrgUnitId = null,
    Guid? DepartmentOrgUnitId = null,
    Guid? TeamOrgUnitId = null,
    Guid? PositionOrgUnitId = null,
    Guid? ManagerPersonId = null,
    string? JobTitle = null,
    Guid? HomeBaseLocationId = null,
    bool CanLogin = false,
    IReadOnlyList<CreatePersonRoleAssignmentRequest>? InitialRoleAssignments = null);

public sealed record UpdateStaffPersonRequest(
    string PrimaryEmail,
    string? LegalFirstName = null,
    string? LegalMiddleName = null,
    string? LegalLastName = null,
    string? PreferredName = null,
    string? Pronouns = null,
    string? GivenName = null,
    string? FamilyName = null,
    string? AlternateEmail = null,
    string? PrimaryPhone = null,
    string? AlternatePhone = null,
    string? WorkPhone = null,
    string? WorkRelationshipType = null,
    string? EmploymentType = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? ExpectedStartDate = null,
    Guid? PrimaryOrgUnitId = null,
    Guid? SiteOrgUnitId = null,
    Guid? ManagerPersonId = null,
    string? JobTitle = null,
    Guid? HomeBaseLocationId = null,
    bool? CanLoginSnapshot = null);

public sealed record UpdatePersonEmploymentStatusRequest(
    string EmploymentStatus,
    string? Reason);

public sealed record BulkPersonImportRowRequest(
    string GivenName,
    string FamilyName,
    string PrimaryEmail,
    string EmploymentStatus = "active",
    Guid? PrimaryOrgUnitId = null,
    Guid? ManagerPersonId = null,
    string? ManagerEmail = null,
    string? JobTitle = null);

public sealed record BulkPersonImportRequest(
    IReadOnlyList<BulkPersonImportRowRequest> People,
    bool DryRun = false);

public sealed record BulkPersonImportRowResult(
    int RowIndex,
    string PrimaryEmail,
    string Status,
    Guid? PersonId,
    string? ErrorCode,
    string? Message);

public sealed record BulkPersonImportResponse(
    Guid ImportId,
    bool DryRun,
    int TotalRows,
    int CreatedCount,
    int ValidatedCount,
    int ErrorCount,
    IReadOnlyList<BulkPersonImportRowResult> Results);

public sealed record OrgUnitResponse(
    Guid OrgUnitId,
    string UnitType,
    string Name,
    Guid? ParentOrgUnitId,
    string Status,
    string? Description = null,
    Guid? ManagerPersonId = null,
    DateTimeOffset? EffectiveStartDate = null,
    DateTimeOffset? EffectiveEndDate = null,
    string? SiteType = null,
    string? Timezone = null,
    string? Phone = null,
    string? EmergencyContact = null,
    string? TeamType = null,
    string? PositionCode = null,
    Guid? DefaultSiteOrgUnitId = null,
    bool ComplianceSensitive = false,
    bool SafetySensitive = false,
    bool CanSupervise = false,
    bool CanApprove = false);

public sealed record CreateOrgUnitRequest(
    string UnitType,
    string Name,
    Guid? ParentOrgUnitId,
    string? Description = null,
    Guid? ManagerPersonId = null,
    DateTimeOffset? EffectiveStartDate = null,
    DateTimeOffset? EffectiveEndDate = null,
    string? SiteType = null,
    string? Timezone = null,
    string? Phone = null,
    string? EmergencyContact = null,
    string? TeamType = null,
    string? PositionCode = null,
    Guid? DefaultSiteOrgUnitId = null,
    bool ComplianceSensitive = false,
    bool SafetySensitive = false,
    bool CanSupervise = false,
    bool CanApprove = false,
    string? Status = null);

public sealed record UpdateOrgUnitRequest(
    string UnitType,
    string Name,
    Guid? ParentOrgUnitId,
    string? Description = null,
    Guid? ManagerPersonId = null,
    DateTimeOffset? EffectiveStartDate = null,
    DateTimeOffset? EffectiveEndDate = null,
    string? SiteType = null,
    string? Timezone = null,
    string? Phone = null,
    string? EmergencyContact = null,
    string? TeamType = null,
    string? PositionCode = null,
    Guid? DefaultSiteOrgUnitId = null,
    bool ComplianceSensitive = false,
    bool SafetySensitive = false,
    bool CanSupervise = false,
    bool CanApprove = false,
    string? Status = null);

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
    DateTimeOffset UpdatedAt,
    bool IsPrimary = false,
    DateTimeOffset? EffectiveAt = null,
    DateTimeOffset? EndsAt = null,
    string? Reason = null);

public sealed record CreateOrgUnitAssignmentRequest(
    Guid SiteOrgUnitId,
    Guid DepartmentOrgUnitId,
    Guid TeamOrgUnitId,
    Guid PositionOrgUnitId,
    string Status = "active",
    bool? IsPrimary = null,
    DateTimeOffset? EffectiveAt = null,
    DateTimeOffset? EndsAt = null,
    string? Reason = null);

public sealed record UpdateOrgUnitAssignmentRequest(
    Guid SiteOrgUnitId,
    Guid DepartmentOrgUnitId,
    Guid TeamOrgUnitId,
    Guid PositionOrgUnitId,
    string Status = "active",
    bool? IsPrimary = null,
    DateTimeOffset? EffectiveAt = null,
    DateTimeOffset? EndsAt = null,
    string? Reason = null);

public sealed record UpdateOrgUnitAssignmentStatusRequest(
    string Status,
    DateTimeOffset? EndsAt = null,
    string? Reason = null);

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
    string Status,
    string ProductKey = "staffarr",
    string PermissionScope = "tenant",
    string Sensitivity = "standard",
    DateTimeOffset? LastSyncedAt = null);

public sealed record RoleTemplatePermissionResponse(
    Guid MappingId,
    Guid PermissionTemplateId,
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue,
    string ProductKey = "staffarr",
    string PermissionSensitivity = "standard",
    DateTimeOffset? LastSyncedAt = null);

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
    string EffectiveStatus,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePersonRoleAssignmentRequest(
    Guid RoleTemplateId,
    string ScopeType,
    string? ScopeValue,
    DateTimeOffset? ExpiresAt = null);

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
