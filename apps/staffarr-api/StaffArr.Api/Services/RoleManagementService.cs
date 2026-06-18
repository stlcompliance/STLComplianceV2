using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class RoleManagementService(
    StaffArrDbContext db,
    IStaffArrAuditService audit,
    StaffArrTenantSettingsService tenantSettingsService)
{
    public const string PermissionCatalogReadActionScope = "staffarr.permission_catalog.read";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> AllowedRoleTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "system_template",
        "tenant_role",
        "product_template"
    };

    private static readonly HashSet<string> AllowedPermissionEffects = new(StringComparer.OrdinalIgnoreCase)
    {
        "allow",
        "deny"
    };

    private static readonly HashSet<string> AllowedScopeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "tenant",
        "site",
        "department",
        "location",
        "team",
        "position",
        "record_set",
        "assigned_assets",
        "own_records",
        "direct_reports"
    };

    private static readonly HashSet<string> ScopeTypesRequiringReference = new(StringComparer.OrdinalIgnoreCase)
    {
        "site",
        "department",
        "location",
        "team",
        "position",
        "record_set"
    };

    private static readonly string[] SystemTemplateNames =
    [
        "Owner",
        "Platform Admin",
        TenantAdminPermissionInheritanceRules.TenantAdminSystemTemplateName,
        "Product Admin",
        "Site Manager",
        "Maintenance Manager",
        "Technician",
        "Operator",
        "Dispatcher",
        "Warehouse Receiver",
        "Inventory Clerk",
        "Trainer",
        "Auditor",
        "Read-Only Auditor",
        "Vendor Portal User",
        "Customer Portal User"
    ];

    private static readonly HashSet<string> FullAccessSystemTemplateNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Owner",
        "Platform Admin",
        TenantAdminPermissionInheritanceRules.TenantAdminSystemTemplateName
    };

    private static readonly Dictionary<string, string> ProductNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["staffarr"] = "StaffArr",
        ["maintainarr"] = "MaintainArr",
        ["trainarr"] = "TrainArr",
        ["routarr"] = "RoutArr",
        ["supplyarr"] = "SupplyArr",
        ["loadarr"] = "LoadArr",
        ["compliancecore"] = "ComplianceCore",
        ["recordarr"] = "RecordArr",
        ["reportarr"] = "ReportArr",
        ["assurarr"] = "AssurArr",
        ["ordarr"] = "OrdArr",
        ["customarr"] = "CustomArr",
        ["fieldcompanion"] = "Field Companion"
    };

    public async Task<IReadOnlyList<StaffRoleSummaryResponse>> ListRolesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await EnsureSystemTemplatesAsync(tenantId, cancellationToken);

        var roles = await db.StaffRoles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.IsArchived)
            .ThenBy(x => x.IsSystem ? 0 : 1)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(x => x.Id).ToArray();
        var permissionCounts = await db.StaffRolePermissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.RoleId))
            .GroupBy(x => x.RoleId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var scopeCounts = await db.StaffRoleScopes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.RoleId))
            .GroupBy(x => x.RoleId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var assignmentCounts = await db.StaffPersonRoles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.RoleId))
            .GroupBy(x => x.RoleId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        return roles
            .Select(role => new StaffRoleSummaryResponse(
                role.Id,
                role.TenantId,
                role.Name,
                role.Description,
                role.RoleType,
                role.IsSystem,
                role.IsArchived,
                permissionCounts.GetValueOrDefault(role.Id),
                scopeCounts.GetValueOrDefault(role.Id),
                assignmentCounts.GetValueOrDefault(role.Id),
                role.CreatedAt,
                role.UpdatedAt))
            .ToList();
    }

    public async Task<StaffRoleDetailResponse> CreateRoleAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        CreateStaffRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureSystemTemplatesAsync(tenantId, cancellationToken);

        var normalizedName = NormalizeName(request.Name);
        var normalizedDescription = NormalizeDescription(request.Description);
        var roleType = NormalizeRoleType(request.RoleType, allowSystemTemplate: false);
        await EnsureRoleNameIsAvailableAsync(tenantId, normalizedName, null, cancellationToken);

        var role = new StaffRole
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = normalizedName,
            Description = normalizedDescription,
            RoleType = roleType,
            IsSystem = false,
            IsArchived = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.StaffRoles.Add(role);
        await db.SaveChangesAsync(cancellationToken);

        await WritePermissionAuditAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            "role.create",
            role.Id,
            before: null,
            after: await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken),
            reason: null,
            cancellationToken);

        return await GetRoleAsync(tenantId, role.Id, cancellationToken);
    }

    public async Task<StaffRoleDetailResponse> GetRoleAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        await EnsureSystemTemplatesAsync(tenantId, cancellationToken);

        var role = await db.StaffRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == roleId, cancellationToken);
        if (role is null)
        {
            throw new StlApiException("staff_role.not_found", "Role was not found.", 404);
        }

        return await BuildRoleDetailAsync(role, cancellationToken);
    }

    public async Task<StaffRoleDetailResponse> UpdateRoleAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid roleId,
        UpdateStaffRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await GetEditableRoleAsync(tenantId, roleId, cancellationToken);
        var before = await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken);

        var normalizedName = NormalizeName(request.Name);
        await EnsureRoleNameIsAvailableAsync(tenantId, normalizedName, role.Id, cancellationToken);

        role.Name = normalizedName;
        role.Description = NormalizeDescription(request.Description);
        role.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await WritePermissionAuditAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            "role.update",
            role.Id,
            before,
            await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken),
            null,
            cancellationToken);

        return await GetRoleAsync(tenantId, roleId, cancellationToken);
    }

    public async Task<StaffRoleDetailResponse> ArchiveRoleAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid roleId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var role = await GetEditableRoleAsync(tenantId, roleId, cancellationToken);
        var before = await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken);

        role.IsArchived = true;
        role.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await WritePermissionAuditAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            "role.archive",
            role.Id,
            before,
            await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken),
            reason,
            cancellationToken);

        return await GetRoleAsync(tenantId, roleId, cancellationToken);
    }

    public async Task<StaffRoleDetailResponse> CloneRoleAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid roleId,
        CloneStaffRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = await db.StaffRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == roleId, cancellationToken);
        if (source is null)
        {
            throw new StlApiException("staff_role.not_found", "Role was not found.", 404);
        }

        var normalizedName = NormalizeName(request.Name);
        var normalizedDescription = NormalizeDescription(request.Description);
        var roleType = NormalizeRoleType(request.RoleType, allowSystemTemplate: false);
        await EnsureRoleNameIsAvailableAsync(tenantId, normalizedName, null, cancellationToken);

        var clone = new StaffRole
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = normalizedName,
            Description = normalizedDescription,
            RoleType = roleType,
            IsSystem = false,
            IsArchived = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.StaffRoles.Add(clone);

        var sourcePermissions = await db.StaffRolePermissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RoleId == source.Id)
            .ToListAsync(cancellationToken);
        var sourceScopes = await db.StaffRoleScopes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RoleId == source.Id)
            .ToListAsync(cancellationToken);

        foreach (var permission in sourcePermissions)
        {
            db.StaffRolePermissions.Add(new StaffRolePermission
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RoleId = clone.Id,
                ProductKey = permission.ProductKey,
                PermissionKey = permission.PermissionKey,
                Effect = permission.Effect,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        foreach (var scope in sourceScopes)
        {
            db.StaffRoleScopes.Add(new StaffRoleScope
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RoleId = clone.Id,
                ScopeType = scope.ScopeType,
                ScopeRefId = scope.ScopeRefId,
                ScopeRefSnapshot = scope.ScopeRefSnapshot,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        await WritePermissionAuditAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            "role.clone",
            clone.Id,
            before: null,
            after: await BuildRoleAuditSnapshotAsync(tenantId, clone.Id, cancellationToken),
            reason: $"sourceRoleId={source.Id}",
            cancellationToken);

        return await GetRoleAsync(tenantId, clone.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<StaffRolePermissionResponse>> GetRolePermissionsAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var detail = await GetRoleAsync(tenantId, roleId, cancellationToken);
        return detail.Permissions;
    }

    public async Task<IReadOnlyList<StaffRolePermissionResponse>> SetRolePermissionsAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid roleId,
        SetStaffRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await GetEditableRoleAsync(tenantId, roleId, cancellationToken);
        var before = await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken);
        var permissions = await NormalizeRolePermissionsAsync(tenantId, request.Permissions, cancellationToken);

        var existing = await db.StaffRolePermissions
            .Where(x => x.TenantId == tenantId && x.RoleId == roleId)
            .ToListAsync(cancellationToken);
        db.StaffRolePermissions.RemoveRange(existing);

        foreach (var permission in permissions)
        {
            db.StaffRolePermissions.Add(new StaffRolePermission
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RoleId = roleId,
                ProductKey = permission.ProductKey,
                PermissionKey = permission.PermissionKey,
                Effect = permission.Effect,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        role.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await WritePermissionAuditAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            "role.permissions.set",
            role.Id,
            before,
            await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken),
            null,
            cancellationToken);

        return (await GetRoleAsync(tenantId, roleId, cancellationToken)).Permissions;
    }

    public async Task<IReadOnlyList<StaffRoleScopeResponse>> GetRoleScopesAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var detail = await GetRoleAsync(tenantId, roleId, cancellationToken);
        return detail.Scopes;
    }

    public async Task<IReadOnlyList<StaffRoleScopeResponse>> SetRoleScopesAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid roleId,
        SetStaffRoleScopesRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await GetEditableRoleAsync(tenantId, roleId, cancellationToken);
        var before = await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken);
        var scopes = await NormalizeRoleScopesAsync(tenantId, request.Scopes, cancellationToken);

        var existing = await db.StaffRoleScopes
            .Where(x => x.TenantId == tenantId && x.RoleId == roleId)
            .ToListAsync(cancellationToken);
        db.StaffRoleScopes.RemoveRange(existing);

        foreach (var scope in scopes)
        {
            db.StaffRoleScopes.Add(scope);
        }

        role.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await WritePermissionAuditAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            "role.scopes.set",
            role.Id,
            before,
            await BuildRoleAuditSnapshotAsync(tenantId, role.Id, cancellationToken),
            null,
            cancellationToken);

        return (await GetRoleAsync(tenantId, roleId, cancellationToken)).Scopes;
    }

    public async Task<IReadOnlyList<StaffPersonRoleAssignmentResponse>> GetPersonRolesAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        await EnsureSystemTemplatesAsync(tenantId, cancellationToken);

        var assignments = await db.StaffPersonRoles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var roleIds = assignments.Select(x => x.RoleId).Distinct().ToArray();
        var roles = await db.StaffRoles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return assignments
            .Where(assignment => roles.ContainsKey(assignment.RoleId))
            .Select(assignment =>
            {
                var role = roles[assignment.RoleId];
                return new StaffPersonRoleAssignmentResponse(
                    assignment.Id,
                    assignment.TenantId,
                    assignment.PersonId,
                    assignment.RoleId,
                    role.Name,
                    role.RoleType,
                    role.IsSystem,
                    role.IsArchived,
                    assignment.AssignmentScopeType,
                    assignment.AssignmentScopeRefId,
                    assignment.StartsAt,
                    assignment.EndsAt,
                    assignment.AssignedByPersonId,
                    assignment.CreatedAt);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<StaffPersonRoleAssignmentResponse>> SetPersonRolesAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid personId,
        SetStaffPersonRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings.RoleAssignmentApprovalRequired)
        {
            throw new StlApiException(
                "staff_role.assignment_approval_required",
                "Role assignments require approval before they can be granted.",
                409);
        }

        if (settings.RequireAssignmentReason && string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new StlApiException(
                "staff_role.assignment_reason_required",
                "A role assignment reason is required.",
                400);
        }

        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        if (request.Roles.Count > 0)
        {
            await EnsurePersonCanReceiveRoleAssignmentsAsync(tenantId, personId, settings, cancellationToken);
        }
        await EnsureSystemTemplatesAsync(tenantId, cancellationToken);

        var before = await BuildPersonRoleAuditSnapshotAsync(tenantId, personId, cancellationToken);
        var assignments = await NormalizePersonRolesAsync(tenantId, request.Roles, settings, cancellationToken);

        var existing = await db.StaffPersonRoles
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        db.StaffPersonRoles.RemoveRange(existing);

        foreach (var assignment in assignments)
        {
            assignment.PersonId = personId;
            assignment.AssignedByPersonId = actorPersonId;
            db.StaffPersonRoles.Add(assignment);
        }

        await db.SaveChangesAsync(cancellationToken);

        await WritePermissionAuditAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            "person.roles.set",
            roleId: null,
            before,
            await BuildPersonRoleAuditSnapshotAsync(tenantId, personId, cancellationToken),
            string.IsNullOrWhiteSpace(request.Reason) ? $"personId={personId}" : request.Reason.Trim(),
            cancellationToken);

        return await GetPersonRolesAsync(tenantId, personId, cancellationToken);
    }

    public async Task<IReadOnlyList<PermissionCatalogResponse>> GetPermissionCatalogsAsync(
        Guid tenantId,
        IReadOnlyList<string> entitlements,
        CancellationToken cancellationToken = default)
    {
        return await GetActiveCatalogsAsync(tenantId, entitlements, null, cancellationToken);
    }

    public async Task<RefreshPermissionCatalogResponse> RefreshPermissionCatalogsAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        string? productKey,
        IReadOnlyList<string> entitlements,
        CancellationToken cancellationToken = default)
    {
        var catalogs = await RebuildCatalogCacheAsync(tenantId, productKey, cancellationToken);
        await WritePermissionAuditAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            "permission_catalog.refresh",
            roleId: null,
            before: null,
            after: new
            {
                ProductKey = productKey,
                CatalogCount = catalogs.Count
            },
            reason: null,
            cancellationToken);

        var filtered = FilterCatalogsByEntitlement(catalogs, entitlements, productKey);
        return new RefreshPermissionCatalogResponse(DateTimeOffset.UtcNow, filtered);
    }

    public async Task<PermissionEvaluateResponse> EvaluatePermissionAsync(
        PermissionEvaluateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new StlApiException("permission_evaluate.validation", "tenantId is required.", 400);
        }

        if (request.PersonId == Guid.Empty)
        {
            throw new StlApiException("permission_evaluate.validation", "personId is required.", 400);
        }

        var productKey = NormalizeProductKey(request.ProductKey);
        var permissionKey = NormalizePermissionKey(productKey, request.PermissionKey);
        await EnsurePersonExistsAsync(request.TenantId, request.PersonId, cancellationToken);
        await EnsureSystemTemplatesAsync(request.TenantId, cancellationToken);

        var catalogs = await GetActiveCatalogsAsync(
            request.TenantId,
            entitlements: [],
            productKey,
            cancellationToken);
        var catalog = catalogs.FirstOrDefault(x => x.ProductKey.Equals(productKey, StringComparison.OrdinalIgnoreCase));
        var permissionDefinition = catalog?
            .Modules
            .SelectMany(module => module.PermissionGroups)
            .SelectMany(group => group.Permissions)
            .FirstOrDefault(permission => permission.Key.Equals(permissionKey, StringComparison.OrdinalIgnoreCase));

        if (catalog is null || permissionDefinition is null)
        {
            return new PermissionEvaluateResponse(false, "unknown_permission_key", [], false);
        }

        var now = DateTimeOffset.UtcNow;
        var assignments = await db.StaffPersonRoles
            .AsNoTracking()
            .Where(x =>
                x.TenantId == request.TenantId
                && x.PersonId == request.PersonId
                && (x.StartsAt == null || x.StartsAt <= now)
                && (x.EndsAt == null || x.EndsAt > now))
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
        {
            return new PermissionEvaluateResponse(false, "missing_role_permission", [], false);
        }

        var roleIds = assignments.Select(x => x.RoleId).Distinct().ToArray();
        var roles = await db.StaffRoles
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && roleIds.Contains(x.Id) && !x.IsArchived)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var permissions = await db.StaffRolePermissions
            .AsNoTracking()
            .Where(x =>
                x.TenantId == request.TenantId
                && roleIds.Contains(x.RoleId)
                && x.ProductKey == productKey
                && x.PermissionKey == permissionKey)
            .ToListAsync(cancellationToken);
        var scopes = await db.StaffRoleScopes
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && roleIds.Contains(x.RoleId))
            .ToListAsync(cancellationToken);

        var directReportIds = await GetDirectReportIdsAsync(request.TenantId, request.PersonId, cancellationToken);

        var allowRoleIds = new List<Guid>();
        var denyRoleIds = new List<Guid>();
        var scopeMatched = false;
        var permissionSeen = false;

        foreach (var assignment in assignments)
        {
            if (!roles.TryGetValue(assignment.RoleId, out var role))
            {
                continue;
            }

            var rolePermissions = permissions.Where(x => x.RoleId == role.Id).ToList();
            if (rolePermissions.Count == 0)
            {
                continue;
            }

            permissionSeen = true;

            var roleScopes = scopes.Where(x => x.RoleId == role.Id).ToList();
            if (roleScopes.Count == 0)
            {
                roleScopes =
                [
                    new StaffRoleScope
                    {
                        Id = Guid.Empty,
                        TenantId = request.TenantId,
                        RoleId = role.Id,
                        ScopeType = "tenant",
                        CreatedAt = role.CreatedAt
                    }
                ];
            }

            var assignmentMatches = MatchesScope(
                assignment.AssignmentScopeType,
                assignment.AssignmentScopeRefId,
                request.Resource,
                request.PersonId,
                directReportIds);
            if (!assignmentMatches)
            {
                continue;
            }

            foreach (var roleScope in roleScopes)
            {
                if (!MatchesScope(roleScope.ScopeType, roleScope.ScopeRefId, request.Resource, request.PersonId, directReportIds))
                {
                    continue;
                }

                scopeMatched = true;

                foreach (var rolePermission in rolePermissions)
                {
                    if (rolePermission.Effect.Equals("deny", StringComparison.OrdinalIgnoreCase))
                    {
                        denyRoleIds.Add(role.Id);
                    }
                    else
                    {
                        allowRoleIds.Add(role.Id);
                    }
                }
            }
        }

        if (denyRoleIds.Count > 0)
        {
            return new PermissionEvaluateResponse(false, "denied_by_role", denyRoleIds.Distinct().ToList(), scopeMatched);
        }

        if (allowRoleIds.Count > 0)
        {
            return new PermissionEvaluateResponse(true, "allowed_by_role", allowRoleIds.Distinct().ToList(), true);
        }

        if (permissionSeen)
        {
            return new PermissionEvaluateResponse(false, "scope_mismatch", roleIds, false);
        }

        return new PermissionEvaluateResponse(false, "missing_role_permission", [], false);
    }

    private async Task<StaffRoleDetailResponse> BuildRoleDetailAsync(
        StaffRole role,
        CancellationToken cancellationToken)
    {
        var permissions = await db.StaffRolePermissions
            .AsNoTracking()
            .Where(x => x.TenantId == role.TenantId && x.RoleId == role.Id)
            .OrderBy(x => x.ProductKey)
            .ThenBy(x => x.PermissionKey)
            .ToListAsync(cancellationToken);
        var scopes = await db.StaffRoleScopes
            .AsNoTracking()
            .Where(x => x.TenantId == role.TenantId && x.RoleId == role.Id)
            .OrderBy(x => x.ScopeType)
            .ThenBy(x => x.ScopeRefSnapshot)
            .ToListAsync(cancellationToken);
        var personRoles = await db.StaffPersonRoles
            .AsNoTracking()
            .Where(x => x.TenantId == role.TenantId && x.RoleId == role.Id)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        var people = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == role.TenantId && personRoles.Select(pr => pr.PersonId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var audits = await db.PermissionAuditLogEntries
            .AsNoTracking()
            .Where(x => x.TenantId == role.TenantId && x.RoleId == role.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var productKeys = permissions.Select(x => x.ProductKey).Distinct().ToArray();
        var catalogs = await GetActiveCatalogsAsync(role.TenantId, entitlements: [], null, cancellationToken);
        var definitionLookup = BuildPermissionLookup(catalogs.Where(catalog => productKeys.Contains(catalog.ProductKey, StringComparer.OrdinalIgnoreCase)));

        return new StaffRoleDetailResponse(
            role.Id,
            role.TenantId,
            role.Name,
            role.Description,
            role.RoleType,
            role.IsSystem,
            role.IsArchived,
            role.CreatedAt,
            role.UpdatedAt,
            permissions.Select(permission => MapRolePermission(permission, definitionLookup)).ToList(),
            scopes.Select(scope => new StaffRoleScopeResponse(
                scope.Id,
                scope.ScopeType,
                scope.ScopeRefId,
                scope.ScopeRefSnapshot,
                scope.CreatedAt)).ToList(),
            personRoles.Select(personRole => new StaffRoleAssignedPersonResponse(
                personRole.Id,
                personRole.PersonId,
                people.TryGetValue(personRole.PersonId, out var person) ? person.DisplayName : personRole.PersonId.ToString(),
                personRole.AssignmentScopeType,
                personRole.AssignmentScopeRefId,
                personRole.StartsAt,
                personRole.EndsAt,
                personRole.CreatedAt)).ToList(),
            audits.Select(entry => new PermissionAuditLogEntryResponse(
                entry.Id,
                entry.TenantId,
                entry.ActorPersonId,
                entry.Action,
                entry.RoleId,
                entry.BeforeJson,
                entry.AfterJson,
                entry.Reason,
                entry.CreatedAt)).ToList());
    }

    private static StaffRolePermissionResponse MapRolePermission(
        StaffRolePermission permission,
        IReadOnlyDictionary<string, PermissionCatalogPermissionResponse> definitionLookup)
    {
        if (!definitionLookup.TryGetValue(permission.PermissionKey, out var definition))
        {
            definition = new PermissionCatalogPermissionResponse(
                permission.PermissionKey,
                HumanizeSegment(permission.PermissionKey.Split('.').Last()),
                null,
                InferRiskLevel(permission.PermissionKey, "medium"),
                true,
                ["tenant"],
                [],
                []);
        }

        return new StaffRolePermissionResponse(
            permission.Id,
            permission.ProductKey,
            permission.PermissionKey,
            permission.Effect,
            definition.Label,
            definition.Description,
            definition.RiskLevel,
            definition.RequiresScope,
            definition.SupportedScopeTypes,
            definition.DependsOn,
            definition.ConflictsWith,
            permission.CreatedAt);
    }

    private async Task<StaffRole> GetEditableRoleAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var role = await db.StaffRoles
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == roleId, cancellationToken);
        if (role is null)
        {
            throw new StlApiException("staff_role.not_found", "Role was not found.", 404);
        }

        if (role.IsSystem)
        {
            throw new StlApiException("staff_role.readonly", "System templates cannot be edited directly.", 409);
        }

        if (role.IsArchived)
        {
            throw new StlApiException("staff_role.archived", "Archived roles cannot be edited.", 409);
        }

        return role;
    }

    private async Task EnsureRoleNameIsAvailableAsync(
        Guid tenantId,
        string name,
        Guid? existingRoleId,
        CancellationToken cancellationToken)
    {
        var duplicate = await db.StaffRoles
            .AnyAsync(
                x => x.TenantId == tenantId
                    && x.Id != existingRoleId
                    && x.Name.ToLower() == name.ToLower(),
                cancellationToken);
        if (duplicate)
        {
            throw new StlApiException("staff_role.duplicate", "A role with this name already exists.", 409);
        }
    }

    private async Task EnsurePersonExistsAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var exists = await db.People
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("staff_role.person_not_found", "Person was not found.", 404);
        }
    }

    private async Task EnsurePersonCanReceiveRoleAssignmentsAsync(
        Guid tenantId,
        Guid personId,
        StaffArrTenantSettings settings,
        CancellationToken cancellationToken)
    {
        if (settings.AllowInactivePeopleToBeAssignedWork)
        {
            return;
        }

        var status = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == personId)
            .Select(x => x.EmploymentStatus)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "staff_role.inactive_person",
                "Inactive people cannot receive work-authority role assignments for this tenant.",
                409);
        }
    }

    private async Task<List<(string ProductKey, string PermissionKey, string Effect)>> NormalizeRolePermissionsAsync(
        Guid tenantId,
        IReadOnlyList<SetStaffRolePermissionItemRequest> requestPermissions,
        CancellationToken cancellationToken)
    {
        var normalized = requestPermissions
            .Select(permission => (
                ProductKey: NormalizeProductKey(permission.ProductKey),
                PermissionKey: permission.PermissionKey.Trim().ToLowerInvariant(),
                Effect: NormalizeEffect(permission.Effect)))
            .ToList();

        var duplicate = normalized
            .GroupBy(x => $"{x.ProductKey}|{x.PermissionKey}|{x.Effect}", StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new StlApiException("staff_role.permissions_duplicate", "Duplicate permissions are not allowed.", 400);
        }

        var catalogs = await GetActiveCatalogsAsync(
            tenantId,
            entitlements: [],
            productKey: null,
            cancellationToken);
        var lookup = BuildPermissionLookup(catalogs);

        foreach (var permission in normalized)
        {
            var normalizedKey = NormalizePermissionKey(permission.ProductKey, permission.PermissionKey);
            if (!lookup.ContainsKey(normalizedKey))
            {
                throw new StlApiException(
                    "staff_role.permission_unknown",
                    $"Permission '{normalizedKey}' is not present in the active catalog.",
                    400);
            }
        }

        foreach (var group in normalized.GroupBy(x => x.ProductKey))
        {
            var assignedKeys = group.Select(x => x.PermissionKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var permission in group)
            {
                var definition = lookup[permission.PermissionKey];
                var missingDependency = definition.DependsOn.FirstOrDefault(key => !assignedKeys.Contains(key));
                if (!string.IsNullOrWhiteSpace(missingDependency))
                {
                    throw new StlApiException(
                        "staff_role.permission_dependency",
                        $"Permission '{permission.PermissionKey}' depends on '{missingDependency}'.",
                        400);
                }

                var conflict = definition.ConflictsWith.FirstOrDefault(assignedKeys.Contains);
                if (!string.IsNullOrWhiteSpace(conflict))
                {
                    throw new StlApiException(
                        "staff_role.permission_conflict",
                        $"Permission '{permission.PermissionKey}' conflicts with '{conflict}'.",
                        400);
                }
            }
        }

        return normalized;
    }

    private async Task<List<StaffRoleScope>> NormalizeRoleScopesAsync(
        Guid tenantId,
        IReadOnlyList<SetStaffRoleScopeItemRequest> requestScopes,
        CancellationToken cancellationToken)
    {
        if (requestScopes.Count == 0)
        {
            return
            [
                new StaffRoleScope
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ScopeType = "tenant",
                    ScopeRefSnapshot = "Entire tenant",
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ];
        }

        var result = new List<StaffRoleScope>(requestScopes.Count);
        foreach (var scope in requestScopes)
        {
            var scopeType = NormalizeScopeType(scope.ScopeType);
            var scopeRefId = NormalizeScopeReference(scopeType, scope.ScopeRefId);
            var snapshot = await HydrateScopeSnapshotAsync(tenantId, scopeType, scopeRefId, scope.ScopeRefSnapshot, cancellationToken);
            result.Add(new StaffRoleScope
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ScopeType = scopeType,
                ScopeRefId = scopeRefId,
                ScopeRefSnapshot = snapshot,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        var duplicate = result
            .GroupBy(x => $"{x.ScopeType}|{x.ScopeRefId}", StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new StlApiException("staff_role.scope_duplicate", "Duplicate scopes are not allowed.", 400);
        }

        return result;
    }

    private async Task<List<StaffPersonRole>> NormalizePersonRolesAsync(
        Guid tenantId,
        IReadOnlyList<SetStaffPersonRoleItemRequest> requestRoles,
        StaffArrTenantSettings settings,
        CancellationToken cancellationToken)
    {
        var roleIds = requestRoles.Select(x => x.RoleId).Distinct().ToArray();
        var roles = await db.StaffRoles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var roleId in roleIds)
        {
            if (!roles.TryGetValue(roleId, out var role))
            {
                throw new StlApiException("staff_role.not_found", $"Role '{roleId}' was not found.", 404);
            }

            if (role.IsArchived)
            {
                throw new StlApiException("staff_role.archived", $"Role '{role.Name}' is archived.", 409);
            }
        }

        var result = new List<StaffPersonRole>(requestRoles.Count);
        foreach (var requestRole in requestRoles)
        {
            var scopeType = NormalizeScopeType(requestRole.AssignmentScopeType);
            var scopeRefId = NormalizeScopeReference(scopeType, requestRole.AssignmentScopeRefId);
            if (!settings.SiteScopedRoleAssignmentsEnabled
                && scopeType.Equals("site", StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "staff_role.assignment_scope_disabled",
                    "Site-scoped role assignments are disabled for this tenant.",
                    409);
            }

            if (requestRole.StartsAt is DateTimeOffset startsAt
                && requestRole.EndsAt is DateTimeOffset endsAt
                && endsAt <= startsAt)
            {
                throw new StlApiException(
                    "staff_role.assignment_window",
                    "Role assignment end time must be after start time.",
                    400);
            }

            var startsAtValue = requestRole.StartsAt;
            var endsAtValue = requestRole.EndsAt;
            if (settings.RoleExpirationEnabled && endsAtValue is null)
            {
                var grantDays = settings.DefaultRoleGrantDurationDays
                    ?? StaffArrTenantSettingsDefaults.DefaultRoleGrantDurationDays;
                endsAtValue = (startsAtValue ?? DateTimeOffset.UtcNow).AddDays(grantDays);
            }

            if (!settings.RoleExpirationEnabled && endsAtValue is not null)
            {
                throw new StlApiException(
                    "staff_role.assignment_expiration_disabled",
                    "Role expiration is disabled for this tenant.",
                    409);
            }

            result.Add(new StaffPersonRole
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RoleId = requestRole.RoleId,
                AssignmentScopeType = scopeType,
                AssignmentScopeRefId = scopeRefId,
                StartsAt = startsAtValue,
                EndsAt = endsAtValue,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        var duplicate = result
            .GroupBy(x => $"{x.RoleId}|{x.AssignmentScopeType}|{x.AssignmentScopeRefId}", StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new StlApiException(
                "staff_role.assignment_duplicate",
                "Duplicate person role assignments are not allowed.",
                400);
        }

        return result;
    }

    private async Task<string?> HydrateScopeSnapshotAsync(
        Guid tenantId,
        string scopeType,
        string? scopeRefId,
        string? providedSnapshot,
        CancellationToken cancellationToken)
    {
        if (scopeType.Equals("tenant", StringComparison.OrdinalIgnoreCase))
        {
            return "Entire tenant";
        }

        if (scopeType.Equals("assigned_assets", StringComparison.OrdinalIgnoreCase))
        {
            return "Assigned assets only";
        }

        if (scopeType.Equals("own_records", StringComparison.OrdinalIgnoreCase))
        {
            return "Own records only";
        }

        if (scopeType.Equals("direct_reports", StringComparison.OrdinalIgnoreCase))
        {
            return "Direct reports only";
        }

        if (scopeType.Equals("record_set", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(providedSnapshot) ? scopeRefId : providedSnapshot.Trim();
        }

        if (scopeRefId is null || !Guid.TryParse(scopeRefId, out var parsedId))
        {
            throw new StlApiException("staff_role.scope_reference", $"Scope '{scopeType}' requires a valid reference id.", 400);
        }

        if (scopeType.Equals("location", StringComparison.OrdinalIgnoreCase))
        {
            var location = await db.InternalLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == parsedId, cancellationToken);
            if (location is null)
            {
                throw new StlApiException("staff_role.scope_reference", "Location scope reference was not found.", 404);
            }

            return location.Name;
        }

        var orgUnit = await db.OrgUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == parsedId, cancellationToken);
        if (orgUnit is null)
        {
            throw new StlApiException("staff_role.scope_reference", "Org unit scope reference was not found.", 404);
        }

        return orgUnit.Name;
    }

    private static string NormalizeName(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new StlApiException("staff_role.validation", "Role name is required.", 400);
        }

        if (trimmed.Length > 128)
        {
            throw new StlApiException("staff_role.validation", "Role name must be 128 characters or fewer.", 400);
        }

        return trimmed;
    }

    private static string? NormalizeDescription(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 1024)
        {
            throw new StlApiException("staff_role.validation", "Role description must be 1024 characters or fewer.", 400);
        }

        return trimmed;
    }

    private static string NormalizeRoleType(string? value, bool allowSystemTemplate)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedRoleTypes.Contains(normalized))
        {
            throw new StlApiException("staff_role.validation", "Invalid role type.", 400);
        }

        if (!allowSystemTemplate && normalized == "system_template")
        {
            throw new StlApiException("staff_role.validation", "System templates cannot be created through this endpoint.", 400);
        }

        return normalized;
    }

    private static string NormalizeProductKey(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException("staff_role.validation", "Product key is required.", 400);
        }

        return normalized;
    }

    private static string NormalizePermissionKey(string productKey, string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException("staff_role.validation", "Permission key is required.", 400);
        }

        if (!normalized.StartsWith(productKey + ".", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "staff_role.validation",
                $"Permission key '{normalized}' must start with '{productKey}.'.",
                400);
        }

        return normalized;
    }

    private static string NormalizeEffect(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedPermissionEffects.Contains(normalized))
        {
            throw new StlApiException("staff_role.validation", "Permission effect must be allow or deny.", 400);
        }

        return normalized;
    }

    private static string NormalizeScopeType(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedScopeTypes.Contains(normalized))
        {
            throw new StlApiException("staff_role.validation", $"Unsupported scope type '{value}'.", 400);
        }

        return normalized;
    }

    private static string? NormalizeScopeReference(string scopeType, string? scopeRefId)
    {
        var trimmed = string.IsNullOrWhiteSpace(scopeRefId) ? null : scopeRefId.Trim();
        if (ScopeTypesRequiringReference.Contains(scopeType) && string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("staff_role.validation", $"Scope '{scopeType}' requires a scopeRefId.", 400);
        }

        if (!ScopeTypesRequiringReference.Contains(scopeType))
        {
            return null;
        }

        return trimmed;
    }

    private async Task WritePermissionAuditAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        string action,
        Guid? roleId,
        object? before,
        object? after,
        string? reason,
        CancellationToken cancellationToken)
    {
        db.PermissionAuditLogEntries.Add(new PermissionAuditLogEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ActorPersonId = actorPersonId,
            Action = action,
            RoleId = roleId,
            BeforeJson = before is null ? null : JsonSerializer.Serialize(before, JsonOptions),
            AfterJson = after is null ? null : JsonSerializer.Serialize(after, JsonOptions),
            Reason = reason,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            action,
            tenantId,
            actorUserId,
            roleId is null ? "permission_catalog" : "role",
            roleId?.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private async Task<object> BuildRoleAuditSnapshotAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var role = await db.StaffRoles
            .AsNoTracking()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == roleId, cancellationToken);
        var permissions = await db.StaffRolePermissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RoleId == roleId)
            .OrderBy(x => x.ProductKey)
            .ThenBy(x => x.PermissionKey)
            .Select(x => new
            {
                x.ProductKey,
                x.PermissionKey,
                x.Effect
            })
            .ToListAsync(cancellationToken);
        var scopes = await db.StaffRoleScopes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RoleId == roleId)
            .OrderBy(x => x.ScopeType)
            .ThenBy(x => x.ScopeRefId)
            .Select(x => new
            {
                x.ScopeType,
                x.ScopeRefId,
                x.ScopeRefSnapshot
            })
            .ToListAsync(cancellationToken);

        return new
        {
            role.Id,
            role.Name,
            role.Description,
            role.RoleType,
            role.IsSystem,
            role.IsArchived,
            Permissions = permissions,
            Scopes = scopes
        };
    }

    private async Task<object> BuildPersonRoleAuditSnapshotAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var assignments = await db.StaffPersonRoles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                x.RoleId,
                x.AssignmentScopeType,
                x.AssignmentScopeRefId,
                x.StartsAt,
                x.EndsAt
            })
            .ToListAsync(cancellationToken);

        return new
        {
            PersonId = personId,
            Assignments = assignments
        };
    }

    private async Task EnsureSystemTemplatesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var existingNames = await db.StaffRoles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsSystem)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var missing = SystemTemplateNames
            .Where(name => existingNames.All(existing => !existing.Equals(name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var now = DateTimeOffset.UtcNow;
        foreach (var name in missing)
        {
            db.StaffRoles.Add(new StaffRole
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                Description = $"{name} starter template.",
                RoleType = "system_template",
                IsSystem = true,
                IsArchived = false,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (missing.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        await EnsureFullAccessSystemTemplatesAsync(tenantId, now, cancellationToken);
    }

    private async Task EnsureFullAccessSystemTemplatesAsync(
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var fullAccessRoles = (await db.StaffRoles
                .Where(x => x.TenantId == tenantId && x.IsSystem && !x.IsArchived)
                .ToListAsync(cancellationToken))
            .Where(role => FullAccessSystemTemplateNames.Contains(role.Name))
            .ToList();
        if (fullAccessRoles.Count == 0)
        {
            return;
        }

        var roleIds = fullAccessRoles.Select(x => x.Id).ToArray();
        var catalogs = await BuildCatalogsAsync(tenantId, productKey: null, cancellationToken);
        var catalogPermissions = catalogs
            .SelectMany(catalog => catalog.Modules
                .SelectMany(module => module.PermissionGroups)
                .SelectMany(group => group.Permissions)
                .Select(permission => (catalog.ProductKey, PermissionKey: permission.Key)))
            .GroupBy(permission => $"{permission.ProductKey}|{permission.PermissionKey}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
        if (catalogPermissions.Count == 0)
        {
            return;
        }

        var changedRoleIds = new HashSet<Guid>();
        var existingPermissions = await db.StaffRolePermissions
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.RoleId))
            .ToListAsync(cancellationToken);

        var denyPermissions = existingPermissions
            .Where(x => x.Effect.Equals("deny", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (denyPermissions.Count > 0)
        {
            foreach (var permission in denyPermissions)
            {
                changedRoleIds.Add(permission.RoleId);
            }

            db.StaffRolePermissions.RemoveRange(denyPermissions);
        }

        var tenantAdminRoleIds = fullAccessRoles
            .Where(role => TenantAdminPermissionInheritanceRules.IsTenantAdminSystemTemplateName(role.Name))
            .Select(role => role.Id)
            .ToHashSet();
        var disallowedTenantAdminPermissions = existingPermissions
            .Where(x =>
                tenantAdminRoleIds.Contains(x.RoleId)
                && TenantAdminPermissionInheritanceRules.IsPlatformAdminPermission(x.ProductKey, x.PermissionKey))
            .ToList();
        if (disallowedTenantAdminPermissions.Count > 0)
        {
            foreach (var permission in disallowedTenantAdminPermissions)
            {
                changedRoleIds.Add(permission.RoleId);
            }

            db.StaffRolePermissions.RemoveRange(disallowedTenantAdminPermissions);
        }

        var existingAllowKeys = existingPermissions
            .Where(x => x.Effect.Equals("allow", StringComparison.OrdinalIgnoreCase))
            .Where(x => !tenantAdminRoleIds.Contains(x.RoleId)
                || !TenantAdminPermissionInheritanceRules.IsPlatformAdminPermission(x.ProductKey, x.PermissionKey))
            .Select(x => BuildRolePermissionKey(x.RoleId, x.ProductKey, x.PermissionKey, "allow"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var role in fullAccessRoles)
        {
            var inheritedPermissions = TenantAdminPermissionInheritanceRules.IsTenantAdminSystemTemplateName(role.Name)
                ? catalogPermissions
                    .Where(permission => !TenantAdminPermissionInheritanceRules.IsPlatformAdminPermission(
                        permission.ProductKey,
                        permission.PermissionKey))
                : catalogPermissions;

            foreach (var permission in inheritedPermissions)
            {
                var permissionKey = BuildRolePermissionKey(
                    role.Id,
                    permission.ProductKey,
                    permission.PermissionKey,
                    "allow");
                if (!existingAllowKeys.Add(permissionKey))
                {
                    continue;
                }

                db.StaffRolePermissions.Add(new StaffRolePermission
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RoleId = role.Id,
                    ProductKey = permission.ProductKey,
                    PermissionKey = permission.PermissionKey,
                    Effect = "allow",
                    CreatedAt = now
                });
                changedRoleIds.Add(role.Id);
            }
        }

        var existingScopes = await db.StaffRoleScopes
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.RoleId))
            .ToListAsync(cancellationToken);
        var scopedRoleIds = existingScopes
            .Where(x => x.ScopeType.Equals("tenant", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(x.ScopeRefId))
            .Select(x => x.RoleId)
            .ToHashSet();

        foreach (var role in fullAccessRoles)
        {
            if (scopedRoleIds.Contains(role.Id))
            {
                continue;
            }

            db.StaffRoleScopes.Add(new StaffRoleScope
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RoleId = role.Id,
                ScopeType = "tenant",
                ScopeRefSnapshot = "Entire tenant",
                CreatedAt = now
            });
            changedRoleIds.Add(role.Id);
        }

        if (changedRoleIds.Count == 0)
        {
            return;
        }

        foreach (var role in fullAccessRoles.Where(role => changedRoleIds.Contains(role.Id)))
        {
            role.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string BuildRolePermissionKey(
        Guid roleId,
        string productKey,
        string permissionKey,
        string effect) =>
        $"{roleId:N}|{productKey.Trim().ToLowerInvariant()}|{permissionKey.Trim().ToLowerInvariant()}|{effect.Trim().ToLowerInvariant()}";

    private async Task<IReadOnlyList<PermissionCatalogResponse>> GetActiveCatalogsAsync(
        Guid tenantId,
        IReadOnlyList<string> entitlements,
        string? productKey,
        CancellationToken cancellationToken)
    {
        var normalizedFilter = string.IsNullOrWhiteSpace(productKey) ? null : NormalizeProductKey(productKey);
        var cacheEntries = await db.PermissionCatalogCacheEntries
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.IsActive
                && (normalizedFilter == null || x.ProductKey == normalizedFilter))
            .OrderBy(x => x.ProductKey)
            .ToListAsync(cancellationToken);

        if (cacheEntries.Count == 0)
        {
            cacheEntries = (await RebuildCatalogCacheAsync(tenantId, normalizedFilter, cancellationToken))
                .Select(catalog => new PermissionCatalogCacheEntry
                {
                    TenantId = tenantId,
                    ProductKey = catalog.ProductKey,
                    CatalogVersion = catalog.Version,
                    CatalogJson = JsonSerializer.Serialize(catalog, JsonOptions),
                    IsActive = true
                })
                .ToList();
        }

        var catalogs = cacheEntries
            .Select(entry => JsonSerializer.Deserialize<PermissionCatalogResponse>(entry.CatalogJson, JsonOptions))
            .Where(entry => entry is not null)
            .Cast<PermissionCatalogResponse>()
            .ToList();

        return FilterCatalogsByEntitlement(catalogs, entitlements, normalizedFilter);
    }

    private static IReadOnlyList<PermissionCatalogResponse> FilterCatalogsByEntitlement(
        IReadOnlyList<PermissionCatalogResponse> catalogs,
        IReadOnlyList<string> entitlements,
        string? productKey)
    {
        if (!string.IsNullOrWhiteSpace(productKey))
        {
            return catalogs
                .Where(catalog => catalog.ProductKey.Equals(productKey, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (entitlements.Count == 0)
        {
            return catalogs;
        }

        var normalizedEntitlements = entitlements
            .Select(entitlement => entitlement.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return catalogs
            .Where(catalog => normalizedEntitlements.Contains(catalog.ProductKey))
            .ToList();
    }

    private async Task<List<PermissionCatalogResponse>> RebuildCatalogCacheAsync(
        Guid tenantId,
        string? productKey,
        CancellationToken cancellationToken)
    {
        var catalogs = await BuildCatalogsAsync(tenantId, productKey, cancellationToken);
        var normalizedFilter = string.IsNullOrWhiteSpace(productKey) ? null : NormalizeProductKey(productKey);
        var existing = await db.PermissionCatalogCacheEntries
            .Where(x =>
                x.TenantId == tenantId
                && (normalizedFilter == null || x.ProductKey == normalizedFilter))
            .ToListAsync(cancellationToken);

        foreach (var catalog in catalogs)
        {
            foreach (var active in existing.Where(x => x.ProductKey == catalog.ProductKey))
            {
                active.IsActive = false;
            }

            var serialized = JsonSerializer.Serialize(catalog, JsonOptions);
            var sameVersion = existing.FirstOrDefault(x =>
                x.ProductKey == catalog.ProductKey
                && x.CatalogVersion == catalog.Version
                && x.CatalogJson == serialized);

            if (sameVersion is not null)
            {
                sameVersion.IsActive = true;
                sameVersion.FetchedAt = DateTimeOffset.UtcNow;
                continue;
            }

            db.PermissionCatalogCacheEntries.Add(new PermissionCatalogCacheEntry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductKey = catalog.ProductKey,
                CatalogVersion = catalog.Version,
                CatalogJson = serialized,
                FetchedAt = DateTimeOffset.UtcNow,
                IsActive = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return catalogs;
    }

    private async Task<List<PermissionCatalogResponse>> BuildCatalogsAsync(
        Guid tenantId,
        string? productKey,
        CancellationToken cancellationToken)
    {
        var normalizedFilter = string.IsNullOrWhiteSpace(productKey) ? null : NormalizeProductKey(productKey);
        var legacyTemplates = await db.PermissionTemplates
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                // Keep this EF-translatable for Postgres-backed catalog reads.
                && x.Status.ToLower() == "active"
                && (normalizedFilter == null || x.ProductKey == normalizedFilter))
            .ToListAsync(cancellationToken);

        var permissions = CatalogSeedDefinitions
            .Where(seed => normalizedFilter == null || seed.ProductKey.Equals(normalizedFilter, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                seed => seed.PermissionKey,
                seed => new CatalogPermissionBuilder(seed),
                StringComparer.OrdinalIgnoreCase);

        foreach (var template in legacyTemplates)
        {
            var normalizedProductKey = NormalizeProductKey(template.ProductKey);
            var normalizedPermissionKey = NormalizePermissionKey(normalizedProductKey, template.PermissionKey);
            if (!permissions.TryGetValue(normalizedPermissionKey, out var builder))
            {
                builder = new CatalogPermissionBuilder(
                    normalizedProductKey,
                    ResolveProductName(normalizedProductKey),
                    InferModuleKey(normalizedPermissionKey),
                    HumanizeSegment(InferModuleKey(normalizedPermissionKey)),
                    null,
                    InferModuleKey(normalizedPermissionKey),
                    HumanizeSegment(InferModuleKey(normalizedPermissionKey)),
                    normalizedPermissionKey,
                    template.Name,
                    template.Description,
                    InferRiskLevel(normalizedPermissionKey, MapSensitivityToRisk(template.Sensitivity)),
                    !string.Equals(template.PermissionScope, "tenant", StringComparison.OrdinalIgnoreCase),
                    InferSupportedScopeTypes(template.PermissionScope),
                    [],
                    []);
                permissions[normalizedPermissionKey] = builder;
            }

            builder.ProductKey = normalizedProductKey;
            builder.ProductName = ResolveProductName(normalizedProductKey);
            builder.Label = string.IsNullOrWhiteSpace(template.Name) ? builder.Label : template.Name;
            builder.Description = string.IsNullOrWhiteSpace(template.Description) ? builder.Description : template.Description;
            builder.RiskLevel = InferRiskLevel(normalizedPermissionKey, MapSensitivityToRisk(template.Sensitivity));
            builder.RequiresScope = !string.Equals(template.PermissionScope, "tenant", StringComparison.OrdinalIgnoreCase) || builder.RequiresScope;
            if (builder.SupportedScopeTypes.Count == 0)
            {
                builder.SupportedScopeTypes = InferSupportedScopeTypes(template.PermissionScope);
            }
        }

        var catalogs = permissions.Values
            .Where(permission => normalizedFilter == null || permission.ProductKey.Equals(normalizedFilter, StringComparison.OrdinalIgnoreCase))
            .GroupBy(permission => permission.ProductKey, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var modules = group
                    .GroupBy(permission => permission.ModuleKey, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(module => module.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(module => new PermissionCatalogModuleResponse(
                        module.Key,
                        module.First().ModuleLabel,
                        module.First().ModuleDescription,
                        module
                            .GroupBy(permission => permission.GroupKey, StringComparer.OrdinalIgnoreCase)
                            .OrderBy(permissionGroup => permissionGroup.Key, StringComparer.OrdinalIgnoreCase)
                            .Select(permissionGroup => new PermissionCatalogPermissionGroupResponse(
                                permissionGroup.Key,
                                permissionGroup.First().GroupLabel,
                                permissionGroup
                                    .OrderBy(permission => permission.PermissionKey, StringComparer.OrdinalIgnoreCase)
                                    .Select(permission => permission.ToResponse())
                                    .ToList()))
                            .ToList()))
                    .ToList();

                var versionBase = legacyTemplates
                    .Where(template => template.ProductKey.Equals(group.Key, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(template => template.LastSyncedAt ?? template.UpdatedAt)
                    .FirstOrDefault();
                var version = $"{(versionBase?.LastSyncedAt ?? versionBase?.UpdatedAt ?? DateTimeOffset.UtcNow):yyyyMMddHHmmss}-{group.Count()}";
                return new PermissionCatalogResponse(
                    group.Key,
                    group.First().ProductName,
                    version,
                    modules);
            })
            .ToList();

        return catalogs;
    }

    private static IReadOnlyDictionary<string, PermissionCatalogPermissionResponse> BuildPermissionLookup(
        IEnumerable<PermissionCatalogResponse> catalogs) =>
        catalogs
            .SelectMany(catalog => catalog.Modules)
            .SelectMany(module => module.PermissionGroups)
            .SelectMany(group => group.Permissions)
            .GroupBy(permission => permission.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

    private async Task<HashSet<Guid>> GetDirectReportIdsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        return await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ManagerPersonId == personId)
            .Select(x => x.Id)
            .ToHashSetAsync(cancellationToken);
    }

    private static bool MatchesScope(
        string scopeType,
        string? scopeRefId,
        PermissionEvaluationResourceRequest? resource,
        Guid evaluatedPersonId,
        HashSet<Guid> directReportIds)
    {
        if (scopeType.Equals("tenant", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (resource is null)
        {
            return false;
        }

        if (scopeType.Equals("site", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesValue(scopeRefId, resource.SiteId);
        }

        if (scopeType.Equals("department", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesValue(scopeRefId, resource.DepartmentId);
        }

        if (scopeType.Equals("location", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesValue(scopeRefId, resource.LocationId);
        }

        if (scopeType.Equals("team", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesValue(scopeRefId, resource.TeamId);
        }

        if (scopeType.Equals("position", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesValue(scopeRefId, resource.PositionId);
        }

        if (scopeType.Equals("record_set", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesValue(scopeRefId, resource.RecordSetId);
        }

        if (scopeType.Equals("assigned_assets", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesValue(evaluatedPersonId.ToString(), resource.AssignedPersonId)
                || MatchesValue(evaluatedPersonId.ToString(), resource.PersonId);
        }

        if (scopeType.Equals("own_records", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesValue(evaluatedPersonId.ToString(), resource.OwnerPersonId)
                || MatchesValue(evaluatedPersonId.ToString(), resource.PersonId);
        }

        if (scopeType.Equals("direct_reports", StringComparison.OrdinalIgnoreCase))
        {
            if (Guid.TryParse(resource.PersonId, out var resourcePersonId) && directReportIds.Contains(resourcePersonId))
            {
                return true;
            }

            return MatchesValue(evaluatedPersonId.ToString(), resource.ManagerPersonId);
        }

        return false;
    }

    private static bool MatchesValue(string? expected, string? actual) =>
        !string.IsNullOrWhiteSpace(expected)
        && !string.IsNullOrWhiteSpace(actual)
        && expected.Equals(actual, StringComparison.OrdinalIgnoreCase);

    private static string ResolveProductName(string productKey) =>
        ProductNames.TryGetValue(productKey, out var productName)
            ? productName
            : HumanizeSegment(productKey);

    private static string InferModuleKey(string permissionKey)
    {
        var segments = permissionKey.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 2 ? segments[1] : "general";
    }

    private static IReadOnlyList<string> InferSupportedScopeTypes(string? permissionScope)
    {
        var normalized = (permissionScope ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "site" => ["tenant", "site", "location"],
            "department" => ["tenant", "department", "team", "position"],
            "team" => ["tenant", "team", "position"],
            "position" => ["tenant", "position"],
            "record" => ["tenant", "record_set", "own_records", "assigned_assets"],
            _ => ["tenant", "site", "department", "location", "team", "position", "record_set", "assigned_assets", "own_records", "direct_reports"]
        };
    }

    private static string MapSensitivityToRisk(string? sensitivity)
    {
        var normalized = (sensitivity ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "critical" => "critical",
            "sensitive" => "high",
            _ => "medium"
        };
    }

    private static string InferRiskLevel(string permissionKey, string currentRiskLevel)
    {
        var requiredRisk = permissionKey.Contains(".roles.", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains(".people.", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains("view_cost", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains("delete", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains("archive", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains(".export", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains(".issue", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains(".override", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains(".adjust", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains(".approve", StringComparison.OrdinalIgnoreCase)
                           || permissionKey.Contains("compliancecore.admin", StringComparison.OrdinalIgnoreCase)
            ? "high"
            : currentRiskLevel;

        if (permissionKey.Contains("archive_delete", StringComparison.OrdinalIgnoreCase)
            || permissionKey.Contains("platform_admin", StringComparison.OrdinalIgnoreCase)
            || permissionKey.Contains(".manage_forms", StringComparison.OrdinalIgnoreCase)
            || permissionKey.Contains(".certificate", StringComparison.OrdinalIgnoreCase)
            || permissionKey.Contains(".dispatch", StringComparison.OrdinalIgnoreCase))
        {
            requiredRisk = "critical";
        }

        return MaxRiskLevel(currentRiskLevel, requiredRisk);
    }

    private static string MaxRiskLevel(string left, string right)
    {
        var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["low"] = 0,
            ["medium"] = 1,
            ["high"] = 2,
            ["critical"] = 3
        };

        return order[left] >= order[right] ? left : right;
    }

    private static string HumanizeSegment(string value)
    {
        var parts = value
            .Split(['_', '-', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant());
        return string.Join(' ', parts);
    }

    private sealed class CatalogPermissionBuilder
    {
        public CatalogPermissionBuilder(CatalogPermissionSeed seed)
            : this(
                seed.ProductKey,
                seed.ProductName,
                seed.ModuleKey,
                seed.ModuleLabel,
                seed.ModuleDescription,
                seed.GroupKey,
                seed.GroupLabel,
                seed.PermissionKey,
                seed.Label,
                seed.Description,
                seed.RiskLevel,
                seed.RequiresScope,
                seed.SupportedScopeTypes,
                seed.DependsOn,
                seed.ConflictsWith)
        {
        }

        public CatalogPermissionBuilder(
            string productKey,
            string productName,
            string moduleKey,
            string moduleLabel,
            string? moduleDescription,
            string groupKey,
            string groupLabel,
            string permissionKey,
            string label,
            string? description,
            string riskLevel,
            bool requiresScope,
            IReadOnlyList<string> supportedScopeTypes,
            IReadOnlyList<string> dependsOn,
            IReadOnlyList<string> conflictsWith)
        {
            ProductKey = productKey;
            ProductName = productName;
            ModuleKey = moduleKey;
            ModuleLabel = moduleLabel;
            ModuleDescription = moduleDescription;
            GroupKey = groupKey;
            GroupLabel = groupLabel;
            PermissionKey = permissionKey;
            Label = label;
            Description = description;
            RiskLevel = riskLevel;
            RequiresScope = requiresScope;
            SupportedScopeTypes = supportedScopeTypes;
            DependsOn = dependsOn;
            ConflictsWith = conflictsWith;
        }

        public string ProductKey { get; set; }

        public string ProductName { get; set; }

        public string ModuleKey { get; set; }

        public string ModuleLabel { get; set; }

        public string? ModuleDescription { get; set; }

        public string GroupKey { get; set; }

        public string GroupLabel { get; set; }

        public string PermissionKey { get; }

        public string Label { get; set; }

        public string? Description { get; set; }

        public string RiskLevel { get; set; }

        public bool RequiresScope { get; set; }

        public IReadOnlyList<string> SupportedScopeTypes { get; set; }

        public IReadOnlyList<string> DependsOn { get; set; }

        public IReadOnlyList<string> ConflictsWith { get; set; }

        public PermissionCatalogPermissionResponse ToResponse() =>
            new(
                PermissionKey,
                Label,
                Description,
                RiskLevel,
                RequiresScope,
                SupportedScopeTypes,
                DependsOn,
                ConflictsWith);
    }

    private sealed record CatalogPermissionSeed(
        string ProductKey,
        string ProductName,
        string ModuleKey,
        string ModuleLabel,
        string? ModuleDescription,
        string GroupKey,
        string GroupLabel,
        string PermissionKey,
        string Label,
        string? Description,
        string RiskLevel,
        bool RequiresScope,
        IReadOnlyList<string> SupportedScopeTypes,
        IReadOnlyList<string> DependsOn,
        IReadOnlyList<string> ConflictsWith);

    private static readonly CatalogPermissionSeed[] CatalogSeedDefinitions =
    [
        new("staffarr", "StaffArr", "roles", "Roles", "Cross-product role administration", "roles", "Roles", "staffarr.roles.read", "View roles", "View role definitions and assignments.", "medium", false, ["tenant"], [], []),
        new("staffarr", "StaffArr", "roles", "Roles", "Cross-product role administration", "roles", "Roles", "staffarr.roles.manage", "Manage roles", "Create, clone, archive, and edit roles.", "critical", false, ["tenant"], ["staffarr.roles.read"], []),
        new("staffarr", "StaffArr", "people", "People", "Workforce directory administration", "people", "People", "staffarr.people.read", "View people", "View workforce people and assignments.", "medium", false, ["tenant", "site", "department", "team"], [], []),
        new("staffarr", "StaffArr", "people", "People", "Workforce directory administration", "people", "People", "staffarr.people.manage", "Manage people", "Create and edit people records.", "high", false, ["tenant", "site", "department", "team"], ["staffarr.people.read"], []),
        new("staffarr", "StaffArr", "permissions", "Permissions", "Cross-product role-based permission administration", "permissions", "Permissions", "staffarr.permissions.assign", "Manage role permissions", "Assign permission templates to roles and role scopes; people inherit access through role assignments only.", "high", false, ["tenant"], ["staffarr.roles.read"], []),
        new("customarr", "CustomArr", "accounts", "Accounts", "Customer relationship records", "accounts", "Accounts", "customarr.accounts.read", "View accounts", "View tenant customer accounts and relationship status.", "medium", true, ["tenant", "site", "department", "record_set", "own_records"], [], []),
        new("customarr", "CustomArr", "accounts", "Accounts", "Customer relationship records", "accounts", "Accounts", "customarr.accounts.manage", "Manage accounts", "Create and update customer account records.", "high", true, ["tenant", "site", "department"], ["customarr.accounts.read"], []),
        new("customarr", "CustomArr", "locations", "Locations", "Customer location and access records", "locations", "Locations", "customarr.locations.manage", "Manage locations", "Manage customer locations, service access details, and location eligibility notes.", "high", true, ["tenant", "site", "location"], ["customarr.accounts.read"], []),
        new("customarr", "CustomArr", "contacts", "Contacts", "Customer contacts and authorizations", "contacts", "Contacts", "customarr.contacts.manage", "Manage contacts", "Manage customer contacts, authorizations, consent, and portal-contact scope.", "high", true, ["tenant", "site", "department", "own_records"], ["customarr.accounts.read"], []),
        new("customarr", "CustomArr", "leads", "Leads", "Customer prospect intake", "leads", "Leads", "customarr.leads.read", "View leads", "View CustomArr lead records.", "medium", true, ["tenant", "department", "team", "own_records"], [], []),
        new("customarr", "CustomArr", "leads", "Leads", "Customer prospect intake", "leads", "Leads", "customarr.leads.manage", "Manage leads", "Create and update CustomArr lead records.", "high", true, ["tenant", "department", "team"], ["customarr.leads.read"], []),
        new("customarr", "CustomArr", "leads", "Leads", "Customer prospect intake", "leads", "Leads", "customarr.leads.convert", "Convert leads", "Convert qualified leads into customer accounts and opportunities.", "high", true, ["tenant", "department", "team"], ["customarr.leads.manage", "customarr.accounts.manage"], []),
        new("customarr", "CustomArr", "opportunities", "Opportunities", "Commercial intent and opportunity pipeline", "opportunities", "Opportunities", "customarr.opportunities.read", "View opportunities", "View customer opportunity records.", "medium", true, ["tenant", "department", "team", "own_records"], [], []),
        new("customarr", "CustomArr", "opportunities", "Opportunities", "Commercial intent and opportunity pipeline", "opportunities", "Opportunities", "customarr.opportunities.manage", "Manage opportunities", "Create and update customer opportunity records.", "high", true, ["tenant", "department", "team"], ["customarr.opportunities.read"], []),
        new("customarr", "CustomArr", "opportunities", "Opportunities", "Commercial intent and opportunity pipeline", "opportunities", "Opportunities", "customarr.opportunities.handoff", "Handoff opportunities", "Mark opportunities won and request explicit downstream handoffs without creating execution records.", "critical", true, ["tenant", "department", "team"], ["customarr.opportunities.manage"], []),
        new("customarr", "CustomArr", "proposals", "Proposals", "Proposal snapshots and customer response", "proposals", "Proposals", "customarr.proposals.read", "View proposals", "View customer proposals and snapshot terms.", "medium", true, ["tenant", "department", "team", "own_records"], [], []),
        new("customarr", "CustomArr", "proposals", "Proposals", "Proposal snapshots and customer response", "proposals", "Proposals", "customarr.proposals.manage", "Manage proposals", "Create and update proposal snapshots.", "high", true, ["tenant", "department", "team"], ["customarr.proposals.read"], []),
        new("customarr", "CustomArr", "proposals", "Proposals", "Proposal snapshots and customer response", "proposals", "Proposals", "customarr.proposals.accept", "Accept proposals", "Record customer proposal acceptance and request explicit downstream handoffs.", "critical", true, ["tenant", "department", "team"], ["customarr.proposals.manage"], []),
        new("customarr", "CustomArr", "agreements", "Agreements", "Customer agreement metadata", "agreements", "Agreements", "customarr.agreements.manage", "Manage agreements", "Manage customer agreement metadata and RecordArr document references.", "high", true, ["tenant", "department", "record_set"], ["customarr.accounts.read"], []),
        new("customarr", "CustomArr", "cases", "Cases", "Customer relationship cases", "cases", "Cases", "customarr.cases.read", "View cases", "View CustomArr customer cases.", "medium", true, ["tenant", "department", "team", "own_records"], [], []),
        new("customarr", "CustomArr", "cases", "Cases", "Customer relationship cases", "cases", "Cases", "customarr.cases.manage", "Manage cases", "Create and update customer relationship cases.", "high", true, ["tenant", "department", "team"], ["customarr.cases.read"], []),
        new("customarr", "CustomArr", "operations", "Operations", "Customer eligibility, tasks, and portal access", "operations", "Operations", "customarr.eligibility.check", "Check eligibility", "Evaluate customer eligibility before downstream handoffs.", "high", true, ["tenant", "site", "department", "team"], ["customarr.accounts.read"], []),
        new("customarr", "CustomArr", "operations", "Operations", "Customer eligibility, tasks, and portal access", "operations", "Operations", "customarr.portal_access.manage", "Manage portal access", "Manage customer portal access records and NexArr identity references.", "critical", true, ["tenant", "department", "team"], ["customarr.contacts.manage"], []),
        new("customarr", "CustomArr", "imports", "Imports", "Imports, duplicate review, and merge", "imports", "Imports", "customarr.imports.read", "View imports", "View import batches, duplicate candidates, and merge review queues.", "medium", true, ["tenant", "department", "team"], [], []),
        new("customarr", "CustomArr", "imports", "Imports", "Imports, duplicate review, and merge", "imports", "Imports", "customarr.imports.manage", "Manage imports", "Create import batches, review duplicate candidates, and propose customer merges.", "critical", true, ["tenant", "department", "team"], ["customarr.imports.read", "customarr.accounts.manage"], []),
        new("customarr", "CustomArr", "integrations", "Integrations", "Customer integration references", "integrations", "Integrations", "customarr.integration_references.manage", "Manage integration references", "Manage customer external mappings and integration reference records.", "high", true, ["tenant", "department", "record_set"], ["customarr.accounts.read"], []),
        new("maintainarr", "MaintainArr", "assets", "Assets", "Asset administration and visibility", "assets", "Assets", "maintainarr.assets.view", "View assets", "View asset records and status.", "low", true, ["tenant", "site", "location", "assigned_assets"], [], []),
        new("maintainarr", "MaintainArr", "assets", "Assets", "Asset administration and visibility", "assets", "Assets", "maintainarr.assets.create", "Create assets", "Create asset records.", "medium", true, ["tenant", "site", "location"], ["maintainarr.assets.view"], []),
        new("maintainarr", "MaintainArr", "assets", "Assets", "Asset administration and visibility", "assets", "Assets", "maintainarr.assets.edit", "Edit assets", "Edit asset details.", "medium", true, ["tenant", "site", "location", "assigned_assets"], ["maintainarr.assets.view"], []),
        new("maintainarr", "MaintainArr", "assets", "Assets", "Asset administration and visibility", "assets", "Assets", "maintainarr.assets.view_costs", "View asset costs", "View cost and spend on assets.", "high", true, ["tenant", "site", "location"], ["maintainarr.assets.view"], []),
        new("maintainarr", "MaintainArr", "assets", "Assets", "Asset administration and visibility", "assets", "Assets", "maintainarr.assets.archive_delete", "Archive or delete assets", "Archive or delete asset records.", "critical", true, ["tenant", "site"], ["maintainarr.assets.edit"], []),
        new("maintainarr", "MaintainArr", "work_orders", "Work Orders", "Maintenance work execution", "work_orders", "Work Orders", "maintainarr.work_orders.create", "Create work orders", "Create work orders.", "medium", true, ["tenant", "site", "location", "assigned_assets"], [], []),
        new("maintainarr", "MaintainArr", "work_orders", "Work Orders", "Maintenance work execution", "work_orders", "Work Orders", "maintainarr.work_orders.edit", "Edit work orders", "Edit work order details.", "medium", true, ["tenant", "site", "location", "assigned_assets"], ["maintainarr.work_orders.create"], []),
        new("maintainarr", "MaintainArr", "work_orders", "Work Orders", "Maintenance work execution", "work_orders", "Work Orders", "maintainarr.work_orders.manage_labor", "Manage labor", "Manage labor entries and labor approvals.", "high", true, ["tenant", "site", "location"], ["maintainarr.work_orders.edit"], []),
        new("maintainarr", "MaintainArr", "work_orders", "Work Orders", "Maintenance work execution", "work_orders", "Work Orders", "maintainarr.work_orders.view_costs", "View work order costs", "View work order cost details.", "high", true, ["tenant", "site", "location"], ["maintainarr.work_orders.edit"], []),
        new("maintainarr", "MaintainArr", "inspections", "Inspections", "Inspection workflows", "inspections", "Inspections", "maintainarr.inspections.submit", "Submit inspections", "Submit asset inspections.", "medium", true, ["tenant", "site", "location", "assigned_assets"], [], []),
        new("maintainarr", "MaintainArr", "inspections", "Inspections", "Inspection workflows", "inspections", "Inspections", "maintainarr.inspections.manage_forms", "Manage inspection forms", "Create and manage inspection forms.", "critical", true, ["tenant", "site"], ["maintainarr.inspections.submit"], []),
        new("maintainarr", "MaintainArr", "fuel", "Fuel", "Fuel entries and usage", "fuel", "Fuel", "maintainarr.fuel.submit", "Submit fuel entries", "Submit fuel or fluid usage against assigned assets.", "medium", true, ["tenant", "site", "location", "assigned_assets"], [], []),
        new("maintainarr", "MaintainArr", "issues", "Issues", "Issue reporting", "issues", "Issues", "maintainarr.issues.submit", "Submit issues", "Report mechanical or operational issues.", "medium", true, ["tenant", "site", "location", "assigned_assets"], [], []),
        new("trainarr", "TrainArr", "programs", "Programs", "Training programs", "programs", "Programs", "trainarr.programs.manage", "Manage programs", "Manage training programs.", "high", true, ["tenant", "site", "department", "team"], [], []),
        new("trainarr", "TrainArr", "certificates", "Certificates", "Certificate issuance", "certificates", "Certificates", "trainarr.certificates.issue", "Issue certificates", "Issue training certificates.", "critical", true, ["tenant", "site", "department"], [], []),
        new("routarr", "RoutArr", "dispatch", "Dispatch", "Dispatch control", "dispatch", "Dispatch", "routarr.dispatch.assign_driver", "Assign driver", "Assign drivers to dispatch records.", "critical", true, ["tenant", "site", "department", "team"], [], []),
        new("loadarr", "LoadArr", "receiving", "Receiving", "Inbound receiving", "receiving", "Receiving", "loadarr.receiving.confirm", "Confirm receiving", "Confirm receiving events.", "medium", true, ["tenant", "site", "location"], [], []),
        new("supplyarr", "SupplyArr", "purchase_orders", "Purchase Orders", "Procurement approvals", "purchase_orders", "Purchase Orders", "supplyarr.purchase_orders.approve", "Approve purchase orders", "Approve purchase orders and releases.", "critical", true, ["tenant", "site", "department"], [], []),
        new("recordarr", "RecordArr", "documents", "Documents", "Controlled documents", "documents", "Documents", "recordarr.documents.view", "View documents", "View controlled documents.", "medium", true, ["tenant", "site", "department", "record_set", "own_records"], [], []),
        new("reportarr", "ReportArr", "reports", "Reports", "Reporting and exports", "reports", "Reports", "reportarr.reports.export", "Export reports", "Export operational reports.", "high", true, ["tenant", "site", "department"], [], [])
    ];
}
