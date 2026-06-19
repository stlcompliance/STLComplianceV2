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
    string? WorkerCategory = null,
    string? FlsaStatus = null,
    string? PositionNumber = null,
    string? CurrentEmploymentAction = null,
    DateTimeOffset? CurrentEmploymentActionAt = null,
    string? LeaveStatus = null,
    bool EligibleForRehire = true,
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
    string? WorkerCategory,
    string? FlsaStatus,
    string? PositionNumber,
    string? CurrentEmploymentAction,
    DateTimeOffset? CurrentEmploymentActionAt,
    string? LeaveStatus,
    bool EligibleForRehire,
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
    string? WorkerCategory = null,
    string? FlsaStatus = null,
    string? PositionNumber = null,
    string? CurrentEmploymentAction = null,
    DateTimeOffset? CurrentEmploymentActionAt = null,
    string? LeaveStatus = null,
    bool EligibleForRehire = true,
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
    bool CanLogin = false);

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
    string? WorkerCategory = null,
    string? FlsaStatus = null,
    string? PositionNumber = null,
    string? CurrentEmploymentAction = null,
    DateTimeOffset? CurrentEmploymentActionAt = null,
    string? LeaveStatus = null,
    bool EligibleForRehire = true,
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
    bool CanApprove = false,
    string? Code = null,
    DateTimeOffset? ArchivedAt = null,
    Guid? ArchivedByUserId = null,
    string? ArchiveReason = null,
    int DescendantCount = 0,
    int AssignmentCount = 0);

public sealed record CreateOrgUnitRequest(
    string UnitType,
    string Name,
    Guid? ParentOrgUnitId,
    string? Code = null,
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
    string? Code = null,
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
    string Status,
    string? Reason = null);

public sealed record ArchiveOrgUnitRequest(
    string Reason);

public sealed record RestoreOrgUnitRequest(
    string? Status = null);

public sealed record InternalLocationResponse(
    Guid LocationId,
    Guid TenantId,
    string LocationNumber,
    string Name,
    string LocationType,
    Guid? ParentLocationId,
    Guid? SiteOrgUnitId,
    string SiteNameSnapshot,
    string ParentPathSnapshot,
    string Status,
    string AllowedProductUsage = "all",
    string? Code = null,
    string? Description = null,
    DateTimeOffset? ArchivedAt = null,
    Guid? ArchivedByUserId = null,
    string? ArchiveReason = null,
    int DescendantCount = 0,
    int AssignmentCount = 0);

public sealed record CreateInternalLocationRequest(
    string Name,
    string LocationType,
    Guid? ParentLocationId,
    Guid? SiteOrgUnitId,
    string? Code = null,
    string? Description = null,
    string Status = "planned",
    string AllowedProductUsage = "all");

public sealed record UpdateInternalLocationRequest(
    string Name,
    string LocationType,
    Guid? ParentLocationId,
    Guid? SiteOrgUnitId,
    string? Code = null,
    string? Description = null,
    string Status = "planned",
    string AllowedProductUsage = "all");

public sealed record ArchiveInternalLocationRequest(
    string Reason);

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

public sealed record EffectivePermissionSourceResponse(
    Guid AssignmentId,
    Guid RoleId,
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
