using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class RoleTemplateService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    private static readonly HashSet<string> AllowedScopeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "tenant",
        "site",
        "department",
        "team",
        "position"
    };

    public async Task<IReadOnlyList<PermissionTemplateSummaryResponse>> ListPermissionTemplatesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.PermissionTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.PermissionKey)
            .Select(x => new PermissionTemplateSummaryResponse(
                x.Id,
                x.PermissionKey,
                x.Name,
                x.Description,
                x.Status,
                x.ProductKey,
                x.PermissionScope,
                x.Sensitivity,
                x.LastSyncedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PermissionTemplateSummaryResponse> UpsertPermissionTemplateAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertPermissionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedPermissionKey = NormalizePermissionKey(request.PermissionKey);
        var normalizedName = NormalizeName(request.Name, "Permission template name");
        var normalizedDescription = NormalizeDescription(request.Description);

        var entity = await db.PermissionTemplates.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PermissionKey == normalizedPermissionKey,
            cancellationToken);

        if (entity is null)
        {
            entity = new PermissionTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PermissionKey = normalizedPermissionKey,
                Name = normalizedName,
                Description = normalizedDescription,
                Status = "active",
                ProductKey = "staffarr",
                PermissionScope = "tenant",
                Sensitivity = "standard",
                LastSyncedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.PermissionTemplates.Add(entity);
        }
        else
        {
            entity.Name = normalizedName;
            entity.Description = normalizedDescription;
            entity.Status = "active";
            entity.ProductKey = string.IsNullOrWhiteSpace(entity.ProductKey) ? "staffarr" : entity.ProductKey;
            entity.PermissionScope = string.IsNullOrWhiteSpace(entity.PermissionScope) ? "tenant" : entity.PermissionScope;
            entity.Sensitivity = string.IsNullOrWhiteSpace(entity.Sensitivity) ? "standard" : entity.Sensitivity;
            entity.LastSyncedAt ??= DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "permission_template.upsert",
            tenantId,
            actorUserId,
            "permission_template",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new PermissionTemplateSummaryResponse(
            entity.Id,
            entity.PermissionKey,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.ProductKey,
            entity.PermissionScope,
            entity.Sensitivity,
            entity.LastSyncedAt);
    }

    public async Task<IReadOnlyList<RoleTemplateResponse>> ListRoleTemplatesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var roles = await db.RoleTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        if (roles.Count == 0)
        {
            return [];
        }

        var roleIds = roles.Select(x => x.Id).ToArray();
        var mappings = await db.RoleTemplatePermissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.RoleTemplateId))
            .ToListAsync(cancellationToken);
        var permissionIds = mappings.Select(x => x.PermissionTemplateId).Distinct().ToArray();
        var permissionById = await db.PermissionTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && permissionIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return roles
            .Select(role =>
            {
                var roleMappings = mappings
                    .Where(mapping => mapping.RoleTemplateId == role.Id)
                    .OrderBy(mapping => mapping.ScopeType)
                    .ThenBy(mapping => mapping.ScopeValue)
                    .ThenBy(mapping => mapping.PermissionTemplateId)
                    .Select(mapping =>
                    {
                        permissionById.TryGetValue(mapping.PermissionTemplateId, out var permission);
                        return new RoleTemplatePermissionResponse(
                            mapping.Id,
                            mapping.PermissionTemplateId,
                            permission?.PermissionKey ?? mapping.PermissionTemplateId.ToString(),
                            permission?.Name ?? "Unknown permission",
                            mapping.ScopeType,
                            mapping.ScopeValue,
                            permission?.ProductKey ?? "unknown",
                            permission?.Sensitivity ?? "unknown",
                            permission?.LastSyncedAt);
                    })
                    .ToList();

                return new RoleTemplateResponse(
                    role.Id,
                    role.RoleKey,
                    role.Name,
                    role.Description,
                    role.Status,
                    roleMappings,
                    role.CreatedAt,
                    role.UpdatedAt);
            })
            .ToList();
    }

    public async Task<RoleTemplateResponse> CreateRoleTemplateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateRoleTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRoleKey = NormalizeRoleKey(request.RoleKey);
        var normalizedName = NormalizeName(request.Name, "Role template name");
        var normalizedDescription = NormalizeDescription(request.Description);

        var duplicate = await db.RoleTemplates.AnyAsync(
            x => x.TenantId == tenantId && x.RoleKey == normalizedRoleKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "role_template.duplicate",
                "A role template with this role key already exists.",
                409);
        }

        var roleTemplate = new RoleTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleKey = normalizedRoleKey,
            Name = normalizedName,
            Description = normalizedDescription,
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.RoleTemplates.Add(roleTemplate);
        await db.SaveChangesAsync(cancellationToken);
        await ReplaceRolePermissionMappingsAsync(tenantId, roleTemplate.Id, request.Permissions, cancellationToken);
        await WriteRoleTemplatePermissionHistoryEventsAsync(
            tenantId,
            actorUserId,
            roleTemplate.Id,
            cancellationToken);
        await audit.WriteAsync(
            "role_template.create",
            tenantId,
            actorUserId,
            "role_template",
            roleTemplate.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetRoleTemplateAsync(tenantId, roleTemplate.Id, cancellationToken);
    }

    public async Task<RoleTemplateResponse> UpdateRoleTemplateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid roleTemplateId,
        UpdateRoleTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var roleTemplate = await db.RoleTemplates.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == roleTemplateId,
            cancellationToken);
        if (roleTemplate is null)
        {
            throw new StlApiException("role_template.not_found", "Role template was not found.", 404);
        }

        roleTemplate.Name = NormalizeName(request.Name, "Role template name");
        roleTemplate.Description = NormalizeDescription(request.Description);
        roleTemplate.Status = NormalizeStatus(request.Status, "role template");
        roleTemplate.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await ReplaceRolePermissionMappingsAsync(tenantId, roleTemplate.Id, request.Permissions, cancellationToken);
        await WriteRoleTemplatePermissionHistoryEventsAsync(
            tenantId,
            actorUserId,
            roleTemplate.Id,
            cancellationToken);
        await audit.WriteAsync(
            "role_template.update",
            tenantId,
            actorUserId,
            "role_template",
            roleTemplate.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetRoleTemplateAsync(tenantId, roleTemplateId, cancellationToken);
    }

    public async Task<IReadOnlyList<PersonRoleAssignmentResponse>> ListPersonRoleAssignmentsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        return await QueryPersonRoleAssignmentsAsync(tenantId, personId, cancellationToken);
    }

    public async Task<EffectivePermissionProjectionResponse> ComputeEffectivePermissionProjectionAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        var asOf = DateTimeOffset.UtcNow;
        var assignments = await db.PersonRoleAssignments
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && x.Status == "active"
                && (x.ExpiresAt == null || x.ExpiresAt > asOf))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        if (assignments.Count == 0)
        {
            return new EffectivePermissionProjectionResponse(personId, asOf, []);
        }

        var roleIds = assignments.Select(x => x.RoleTemplateId).Distinct().ToArray();
        var roleById = await db.RoleTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var mappings = await db.RoleTemplatePermissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.RoleTemplateId))
            .ToListAsync(cancellationToken);
        var permissionIds = mappings.Select(x => x.PermissionTemplateId).Distinct().ToArray();
        var permissionById = await db.PermissionTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && permissionIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        IReadOnlyList<PermissionTemplate> tenantAdminPermissionTemplates = roleById.Values.Any(role =>
            string.Equals(role.Status, "active", StringComparison.OrdinalIgnoreCase)
            && TenantAdminPermissionInheritanceRules.IsTenantAdminRoleKey(role.RoleKey))
            ? (await db.PermissionTemplates
                    .AsNoTracking()
                    .Where(x =>
                        x.TenantId == tenantId
                        && x.Status == "active")
                    .OrderBy(x => x.PermissionKey)
                    .ToListAsync(cancellationToken))
                .Where(x => !TenantAdminPermissionInheritanceRules.IsPlatformAdminPermission(x.ProductKey, x.PermissionKey))
                .ToList()
            : [];

        var effectiveRows = new List<(
            string PermissionKey,
            string PermissionName,
            string ScopeType,
            string? ScopeValue,
            EffectivePermissionSourceResponse Source)>();
        foreach (var assignment in assignments)
        {
            if (!roleById.TryGetValue(assignment.RoleTemplateId, out var roleTemplate))
            {
                continue;
            }

            if (!string.Equals(roleTemplate.Status, "active", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TenantAdminPermissionInheritanceRules.IsTenantAdminRoleKey(roleTemplate.RoleKey))
            {
                foreach (var permissionTemplate in tenantAdminPermissionTemplates)
                {
                    var inheritedScopeType = assignment.ScopeType;
                    var inheritedScopeValue = assignment.ScopeValue;
                    if (string.Equals(inheritedScopeType, "tenant", StringComparison.OrdinalIgnoreCase))
                    {
                        inheritedScopeValue = null;
                    }

                    effectiveRows.Add((
                        permissionTemplate.PermissionKey,
                        permissionTemplate.Name,
                        inheritedScopeType,
                        inheritedScopeValue,
                        new EffectivePermissionSourceResponse(
                            assignment.Id,
                            assignment.RoleTemplateId,
                            roleTemplate.RoleKey,
                            roleTemplate.Name,
                            assignment.Status,
                            assignment.ScopeType,
                            assignment.ScopeValue,
                            assignment.CreatedAt)));
                }
            }

            var roleMappings = mappings.Where(x => x.RoleTemplateId == assignment.RoleTemplateId);
            foreach (var mapping in roleMappings)
            {
                if (!permissionById.TryGetValue(mapping.PermissionTemplateId, out var permissionTemplate)
                    || !string.Equals(permissionTemplate.Status, "active", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var scopeType = mapping.ScopeType;
                var scopeValue = mapping.ScopeValue;
                if (!string.Equals(assignment.ScopeType, "tenant", StringComparison.OrdinalIgnoreCase))
                {
                    scopeType = assignment.ScopeType;
                    scopeValue = assignment.ScopeValue;
                }

                effectiveRows.Add((
                    permissionTemplate.PermissionKey,
                    permissionTemplate.Name,
                    scopeType,
                    scopeValue,
                    new EffectivePermissionSourceResponse(
                        assignment.Id,
                        assignment.RoleTemplateId,
                        roleTemplate.RoleKey,
                        roleTemplate.Name,
                        assignment.Status,
                        assignment.ScopeType,
                        assignment.ScopeValue,
                        assignment.CreatedAt)));
            }
        }

        var permissions = effectiveRows
            .GroupBy(x => $"{x.PermissionKey}|{x.ScopeType}|{x.ScopeValue}")
            .OrderBy(x => x.Key)
            .Select(group =>
            {
                var first = group.First();
                return new EffectivePermissionResponse(
                    first.PermissionKey,
                    first.PermissionName,
                    first.ScopeType,
                    first.ScopeValue,
                    group
                        .Select(entry => entry.Source)
                        .OrderByDescending(source => source.AssignedAt)
                        .ToList());
            })
            .ToList();

        return new EffectivePermissionProjectionResponse(personId, asOf, permissions);
    }

    public async Task<IReadOnlyList<PermissionHistoryTimelineEntryResponse>> ListPermissionHistoryTimelineAsync(
        Guid tenantId,
        Guid personId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        var boundedLimit = Math.Clamp(limit, 1, 500);

        return await db.PermissionHistoryEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.OccurredAt)
            .Take(boundedLimit)
            .Select(x => new PermissionHistoryTimelineEntryResponse(
                x.Id,
                x.PersonId,
                x.AssignmentId,
                x.RoleTemplateId,
                x.PermissionTemplateId,
                x.ActorUserId,
                x.EventType,
                x.AssignmentStatus,
                x.RoleKey,
                x.RoleName,
                x.PermissionKey,
                x.PermissionName,
                x.ScopeType,
                x.ScopeValue,
                x.OccurredAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonRoleAssignmentResponse> CreatePersonRoleAssignmentAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        CreatePersonRoleAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        await EnsureRoleTemplateExistsAsync(tenantId, request.RoleTemplateId, requireActive: true, cancellationToken);
        ValidateExpiresAt(request.ExpiresAt);
        var (scopeType, scopeValue) = await NormalizeScopeAsync(
            tenantId,
            request.ScopeType,
            request.ScopeValue,
            mustReferenceActiveOrgUnit: true,
            cancellationToken);

        var duplicateExists = await db.PersonRoleAssignments.AnyAsync(
            x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && x.RoleTemplateId == request.RoleTemplateId
                && x.ScopeType == scopeType
                && x.ScopeValue == scopeValue
                && x.ExpiresAt == request.ExpiresAt,
            cancellationToken);
        if (duplicateExists)
        {
            throw new StlApiException(
                "role_assignment.duplicate",
                "An identical role assignment already exists for this person.",
                409);
        }

        var initialStatus = await DetermineInitialAssignmentStatusAsync(tenantId, request.RoleTemplateId, cancellationToken);

        var assignment = new PersonRoleAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            RoleTemplateId = request.RoleTemplateId,
            ScopeType = scopeType,
            ScopeValue = scopeValue,
            Status = initialStatus,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.PersonRoleAssignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);
        await WritePermissionHistoryEventsForAssignmentAsync(
            tenantId,
            actorUserId,
            assignment,
            "assignment_created",
            cancellationToken);

        await audit.WriteAsync(
            "person_role_assignment.create",
            tenantId,
            actorUserId,
            "person_role_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return (await QueryPersonRoleAssignmentsAsync(tenantId, personId, cancellationToken))
            .First(x => x.AssignmentId == assignment.Id);
    }

    public async Task<PersonRoleAssignmentResponse> UpdatePersonRoleAssignmentStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        Guid assignmentId,
        UpdatePersonRoleAssignmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        var assignment = await db.PersonRoleAssignments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PersonId == personId && x.Id == assignmentId,
            cancellationToken);
        if (assignment is null)
        {
            throw new StlApiException("role_assignment.not_found", "Role assignment was not found.", 404);
        }

        assignment.Status = NormalizeStatus(request.Status, "role assignment");
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await WritePermissionHistoryEventsForAssignmentAsync(
            tenantId,
            actorUserId,
            assignment,
            "assignment_status_updated",
            cancellationToken);
        await audit.WriteAsync(
            "person_role_assignment.status_update",
            tenantId,
            actorUserId,
            "person_role_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return (await QueryPersonRoleAssignmentsAsync(tenantId, personId, cancellationToken))
            .First(x => x.AssignmentId == assignmentId);
    }

    private async Task<RoleTemplateResponse> GetRoleTemplateAsync(Guid tenantId, Guid roleTemplateId, CancellationToken cancellationToken)
    {
        var roles = await ListRoleTemplatesAsync(tenantId, cancellationToken);
        var role = roles.FirstOrDefault(x => x.RoleTemplateId == roleTemplateId);
        return role ?? throw new StlApiException("role_template.not_found", "Role template was not found.", 404);
    }

    private async Task ReplaceRolePermissionMappingsAsync(
        Guid tenantId,
        Guid roleTemplateId,
        IReadOnlyList<RoleTemplatePermissionInput> permissionInputs,
        CancellationToken cancellationToken)
    {
        var normalizedInputs = new List<(Guid PermissionTemplateId, string ScopeType, string? ScopeValue)>(permissionInputs.Count);
        foreach (var input in permissionInputs)
        {
            await EnsurePermissionTemplateExistsAsync(tenantId, input.PermissionTemplateId, cancellationToken);
            var normalizedScope = await NormalizeScopeAsync(
                tenantId,
                input.ScopeType,
                input.ScopeValue,
                mustReferenceActiveOrgUnit: false,
                cancellationToken);
            normalizedInputs.Add((input.PermissionTemplateId, normalizedScope.ScopeType, normalizedScope.ScopeValue));
        }

        var duplicateInput = normalizedInputs
            .GroupBy(x => $"{x.PermissionTemplateId:N}|{x.ScopeType}|{x.ScopeValue}")
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateInput is not null)
        {
            throw new StlApiException("role_template.validation", "Duplicate permission mapping entries are not allowed.", 400);
        }

        var existingMappings = await db.RoleTemplatePermissions
            .Where(x => x.TenantId == tenantId && x.RoleTemplateId == roleTemplateId)
            .ToListAsync(cancellationToken);
        db.RoleTemplatePermissions.RemoveRange(existingMappings);
        await db.SaveChangesAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var newMappings = normalizedInputs.Select(input => new RoleTemplatePermission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleTemplateId = roleTemplateId,
            PermissionTemplateId = input.PermissionTemplateId,
            ScopeType = input.ScopeType,
            ScopeValue = input.ScopeValue,
            CreatedAt = now,
            UpdatedAt = now
        });
        db.RoleTemplatePermissions.AddRange(newMappings);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<PersonRoleAssignmentResponse>> QueryPersonRoleAssignmentsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var asOf = DateTimeOffset.UtcNow;
        var assignments = await db.PersonRoleAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        if (assignments.Count == 0)
        {
            return [];
        }

        var roleIds = assignments.Select(x => x.RoleTemplateId).Distinct().ToArray();
        var roleById = await db.RoleTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && roleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return assignments
            .Select(assignment =>
            {
                roleById.TryGetValue(assignment.RoleTemplateId, out var roleTemplate);
                return new PersonRoleAssignmentResponse(
                    assignment.Id,
                    assignment.PersonId,
                    assignment.RoleTemplateId,
                    roleTemplate?.RoleKey ?? assignment.RoleTemplateId.ToString(),
                    roleTemplate?.Name ?? "Unknown role",
                    assignment.ScopeType,
                    assignment.ScopeValue,
                    assignment.Status,
                    GetEffectiveStatus(assignment, asOf),
                    assignment.ExpiresAt,
                    assignment.CreatedAt,
                    assignment.UpdatedAt);
            })
            .ToList();
    }

    private async Task WriteRoleTemplatePermissionHistoryEventsAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid roleTemplateId,
        CancellationToken cancellationToken)
    {
        var asOf = DateTimeOffset.UtcNow;
        var activeAssignments = await db.PersonRoleAssignments
            .Where(x =>
                x.TenantId == tenantId
                && x.RoleTemplateId == roleTemplateId
                && x.Status == "active"
                && (x.ExpiresAt == null || x.ExpiresAt > asOf))
            .ToListAsync(cancellationToken);
        foreach (var assignment in activeAssignments)
        {
            await WritePermissionHistoryEventsForAssignmentAsync(
                tenantId,
                actorUserId,
                assignment,
                "role_template_permissions_updated",
                cancellationToken);
        }
    }

    private async Task WritePermissionHistoryEventsForAssignmentAsync(
        Guid tenantId,
        Guid actorUserId,
        PersonRoleAssignment assignment,
        string eventType,
        CancellationToken cancellationToken)
    {
        var role = await db.RoleTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == assignment.RoleTemplateId,
                cancellationToken);
        if (role is null)
        {
            return;
        }

        var mappings = await db.RoleTemplatePermissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RoleTemplateId == assignment.RoleTemplateId)
            .ToListAsync(cancellationToken);
        if (mappings.Count == 0)
        {
            return;
        }

        var permissionIds = mappings.Select(x => x.PermissionTemplateId).Distinct().ToArray();
        var permissionById = await db.PermissionTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && permissionIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var events = new List<PermissionHistoryEvent>(mappings.Count);
        foreach (var mapping in mappings)
        {
            if (!permissionById.TryGetValue(mapping.PermissionTemplateId, out var permission))
            {
                continue;
            }

            var scopeType = mapping.ScopeType;
            var scopeValue = mapping.ScopeValue;
            if (!string.Equals(assignment.ScopeType, "tenant", StringComparison.OrdinalIgnoreCase))
            {
                scopeType = assignment.ScopeType;
                scopeValue = assignment.ScopeValue;
            }

            events.Add(new PermissionHistoryEvent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PersonId = assignment.PersonId,
                AssignmentId = assignment.Id,
                RoleTemplateId = role.Id,
                PermissionTemplateId = permission.Id,
                ActorUserId = actorUserId,
                EventType = eventType,
                AssignmentStatus = assignment.Status,
                RoleKey = role.RoleKey,
                RoleName = role.Name,
                PermissionKey = permission.PermissionKey,
                PermissionName = permission.Name,
                ScopeType = scopeType,
                ScopeValue = scopeValue,
                OccurredAt = now
            });
        }

        if (events.Count == 0)
        {
            return;
        }

        db.PermissionHistoryEvents.AddRange(events);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string GetEffectiveStatus(PersonRoleAssignment assignment, DateTimeOffset asOf)
    {
        if (!string.Equals(assignment.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return assignment.Status;
        }

        if (assignment.ExpiresAt is DateTimeOffset expiresAt && expiresAt <= asOf)
        {
            return "expired";
        }

        return assignment.Status;
    }

    private static void ValidateExpiresAt(DateTimeOffset? expiresAt)
    {
        if (expiresAt is not DateTimeOffset value)
        {
            return;
        }

        if (value <= DateTimeOffset.UtcNow)
        {
            throw new StlApiException(
                "role_assignment.validation",
                "Role assignment expiration must be in the future.",
                400);
        }
    }

    private async Task EnsurePersonExistsAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }
    }

    private async Task EnsureRoleTemplateExistsAsync(
        Guid tenantId,
        Guid roleTemplateId,
        bool requireActive,
        CancellationToken cancellationToken)
    {
        var role = await db.RoleTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == roleTemplateId, cancellationToken);
        if (role is null)
        {
            throw new StlApiException("role_template.not_found", "Role template was not found.", 404);
        }

        if (requireActive && !string.Equals(role.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("role_template.inactive", "Role template must be active for assignment.", 409);
        }
    }

    private async Task EnsurePermissionTemplateExistsAsync(Guid tenantId, Guid permissionTemplateId, CancellationToken cancellationToken)
    {
        var exists = await db.PermissionTemplates.AnyAsync(
            x => x.TenantId == tenantId && x.Id == permissionTemplateId && x.Status == "active",
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException(
                "permission_template.not_found",
                "Permission template was not found or inactive.",
                404);
        }
    }

    private async Task<(string ScopeType, string? ScopeValue)> NormalizeScopeAsync(
        Guid tenantId,
        string scopeType,
        string? scopeValue,
        bool mustReferenceActiveOrgUnit,
        CancellationToken cancellationToken)
    {
        var normalizedScopeType = string.IsNullOrWhiteSpace(scopeType)
            ? throw new StlApiException("scope.validation", "Scope type is required.", 400)
            : scopeType.Trim().ToLowerInvariant();
        if (!AllowedScopeTypes.Contains(normalizedScopeType))
        {
            throw new StlApiException("scope.validation", "Scope type is invalid.", 400);
        }

        if (normalizedScopeType == "tenant")
        {
            return ("tenant", null);
        }

        if (!Guid.TryParse(scopeValue, out var orgUnitId))
        {
            throw new StlApiException(
                "scope.validation",
                "Scope value must be a valid org unit identifier for non-tenant scopes.",
                400);
        }

        var orgUnit = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == orgUnitId)
            .Select(x => new { x.UnitType, x.Status })
            .FirstOrDefaultAsync(cancellationToken);
        if (orgUnit is null)
        {
            throw new StlApiException("scope.not_found", "Scope org unit was not found.", 404);
        }

        if (!string.Equals(orgUnit.UnitType, normalizedScopeType, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "scope.validation",
                $"Scope type {normalizedScopeType} requires matching org unit type.",
                409);
        }

        if (mustReferenceActiveOrgUnit && !string.Equals(orgUnit.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("scope.inactive", "Scope org unit must be active.", 409);
        }

        return (normalizedScopeType, orgUnitId.ToString());
    }

    private static string NormalizeRoleKey(string roleKey)
    {
        if (string.IsNullOrWhiteSpace(roleKey))
        {
            throw new StlApiException("role_template.validation", "Role key is required.", 400);
        }

        var normalized = roleKey.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 64)
        {
            throw new StlApiException("role_template.validation", "Role key must be between 3 and 64 characters.", 400);
        }

        if (normalized.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '_' || ch == '.')))
        {
            throw new StlApiException("role_template.validation", "Role key contains invalid characters.", 400);
        }

        return normalized;
    }

    private static string NormalizePermissionKey(string permissionKey)
    {
        if (string.IsNullOrWhiteSpace(permissionKey))
        {
            throw new StlApiException("permission_template.validation", "Permission key is required.", 400);
        }

        var normalized = permissionKey.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 128)
        {
            throw new StlApiException("permission_template.validation", "Permission key length is invalid.", 400);
        }

        if (normalized.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '.')))
        {
            throw new StlApiException(
                "permission_template.validation",
                "Permission key contains invalid characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("validation", $"{fieldName} is required.", 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > 128)
        {
            throw new StlApiException("validation", $"{fieldName} must be 128 characters or less.", 400);
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 512)
        {
            throw new StlApiException("validation", "Description must be 512 characters or less.", 400);
        }

        return normalized;
    }

    private static string NormalizeStatus(string status, string resourceName)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new StlApiException("validation", "Status is required.", 400);
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (normalized is not ("active" or "inactive"))
        {
            throw new StlApiException("validation", $"{resourceName} status must be active or inactive.", 400);
        }

        return normalized;
    }

    private async Task<string> DetermineInitialAssignmentStatusAsync(
        Guid tenantId,
        Guid roleTemplateId,
        CancellationToken cancellationToken)
    {
        var sensitivePermissions = await db.RoleTemplatePermissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RoleTemplateId == roleTemplateId)
            .Join(
                db.PermissionTemplates.AsNoTracking(),
                mapping => mapping.PermissionTemplateId,
                permission => permission.Id,
                (mapping, permission) => permission.Sensitivity)
            .AnyAsync(sensitivity =>
                !string.Equals(sensitivity, "standard", StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        return sensitivePermissions ? "pending_review" : "active";
    }
}
