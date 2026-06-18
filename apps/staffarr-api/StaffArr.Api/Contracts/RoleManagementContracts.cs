namespace StaffArr.Api.Contracts;

public sealed record StaffRoleSummaryResponse(
    Guid RoleId,
    Guid TenantId,
    string Name,
    string? Description,
    string RoleType,
    bool IsSystem,
    bool IsArchived,
    int PermissionCount,
    int ScopeCount,
    int AssignedPersonCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record StaffRolePermissionResponse(
    Guid Id,
    string ProductKey,
    string PermissionKey,
    string Effect,
    string Label,
    string? Description,
    string RiskLevel,
    bool RequiresScope,
    IReadOnlyList<string> SupportedScopeTypes,
    IReadOnlyList<string> DependsOn,
    IReadOnlyList<string> ConflictsWith,
    DateTimeOffset CreatedAt);

public sealed record StaffRoleScopeResponse(
    Guid Id,
    string ScopeType,
    string? ScopeRefId,
    string? ScopeRefSnapshot,
    DateTimeOffset CreatedAt);

public sealed record StaffRoleAssignedPersonResponse(
    Guid PersonRoleId,
    Guid PersonId,
    string DisplayName,
    string AssignmentScopeType,
    string? AssignmentScopeRefId,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    DateTimeOffset CreatedAt);

public sealed record PermissionAuditLogEntryResponse(
    Guid Id,
    Guid TenantId,
    Guid? ActorPersonId,
    string Action,
    Guid? RoleId,
    string? BeforeJson,
    string? AfterJson,
    string? Reason,
    DateTimeOffset CreatedAt);

public sealed record StaffRoleDetailResponse(
    Guid RoleId,
    Guid TenantId,
    string Name,
    string? Description,
    string RoleType,
    bool IsSystem,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<StaffRolePermissionResponse> Permissions,
    IReadOnlyList<StaffRoleScopeResponse> Scopes,
    IReadOnlyList<StaffRoleAssignedPersonResponse> AssignedPeople,
    IReadOnlyList<PermissionAuditLogEntryResponse> AuditHistory);

public sealed record CreateStaffRoleRequest(
    string Name,
    string? Description,
    string RoleType = "tenant_role");

public sealed record UpdateStaffRoleRequest(
    string Name,
    string? Description);

public sealed record ArchiveStaffRoleRequest(
    string? Reason);

public sealed record CloneStaffRoleRequest(
    string Name,
    string? Description,
    string RoleType = "tenant_role");

public sealed record SetStaffRolePermissionItemRequest(
    string ProductKey,
    string PermissionKey,
    string Effect = "allow");

public sealed record SetStaffRolePermissionsRequest(
    IReadOnlyList<SetStaffRolePermissionItemRequest> Permissions);

public sealed record SetStaffRoleScopeItemRequest(
    string ScopeType,
    string? ScopeRefId,
    string? ScopeRefSnapshot);

public sealed record SetStaffRoleScopesRequest(
    IReadOnlyList<SetStaffRoleScopeItemRequest> Scopes);

public sealed record StaffPersonRoleAssignmentResponse(
    Guid PersonRoleId,
    Guid TenantId,
    Guid PersonId,
    Guid RoleId,
    string RoleName,
    string RoleType,
    bool RoleIsSystem,
    bool RoleIsArchived,
    string AssignmentScopeType,
    string? AssignmentScopeRefId,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    Guid? AssignedByPersonId,
    DateTimeOffset CreatedAt);

public sealed record SetStaffPersonRoleItemRequest(
    Guid RoleId,
    string AssignmentScopeType,
    string? AssignmentScopeRefId,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt);

public sealed record SetStaffPersonRolesRequest(
    IReadOnlyList<SetStaffPersonRoleItemRequest> Roles,
    string? Reason = null);

public sealed record PermissionCatalogPermissionResponse(
    string Key,
    string Label,
    string? Description,
    string RiskLevel,
    bool RequiresScope,
    IReadOnlyList<string> SupportedScopeTypes,
    IReadOnlyList<string> DependsOn,
    IReadOnlyList<string> ConflictsWith);

public sealed record PermissionCatalogPermissionGroupResponse(
    string Key,
    string Label,
    IReadOnlyList<PermissionCatalogPermissionResponse> Permissions);

public sealed record PermissionCatalogModuleResponse(
    string Key,
    string Label,
    string? Description,
    IReadOnlyList<PermissionCatalogPermissionGroupResponse> PermissionGroups);

public sealed record PermissionCatalogResponse(
    string ProductKey,
    string ProductName,
    string Version,
    IReadOnlyList<PermissionCatalogModuleResponse> Modules);

public sealed record RefreshPermissionCatalogRequest(
    string? ProductKey = null);

public sealed record RefreshPermissionCatalogResponse(
    DateTimeOffset RefreshedAt,
    IReadOnlyList<PermissionCatalogResponse> Catalogs);

public sealed record PermissionEvaluationResourceRequest(
    string Type,
    string? Id,
    string? SiteId,
    string? LocationId,
    string? DepartmentId,
    string? TeamId,
    string? PositionId,
    string? RecordSetId,
    string? AssignedPersonId,
    string? OwnerPersonId,
    string? PersonId,
    string? ManagerPersonId);

public sealed record PermissionEvaluateRequest(
    Guid TenantId,
    Guid PersonId,
    string ProductKey,
    string PermissionKey,
    PermissionEvaluationResourceRequest? Resource);

public sealed record PermissionEvaluateResponse(
    bool Allowed,
    string Reason,
    IReadOnlyList<Guid> RoleIds,
    bool ScopeMatched);
