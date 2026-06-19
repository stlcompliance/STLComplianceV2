using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class RoleManagementServiceTests
{
    [Fact]
    public async Task ListRolesAsync_seeds_full_access_templates_with_every_active_catalog_permission()
    {
        var options = new DbContextOptionsBuilder<StaffArrDbContext>()
            .UseInMemoryDatabase($"staffarr-role-management-{Guid.NewGuid():N}")
            .Options;

        await using var db = new StaffArrDbContext(options);
        var tenantId = Guid.NewGuid();
        var platformAdminRoleId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.StaffRoles.Add(new StaffRole
        {
            Id = platformAdminRoleId,
            TenantId = tenantId,
            Name = "Platform Admin",
            Description = "Platform Admin starter template.",
            RoleType = "system_template",
            IsSystem = true,
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now.AddDays(-1)
        });
        db.StaffRolePermissions.Add(new StaffRolePermission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleId = platformAdminRoleId,
            ProductKey = "staffarr",
            PermissionKey = "staffarr.people.read",
            Effect = "deny",
            CreatedAt = now.AddDays(-1)
        });
        db.PermissionTemplates.Add(new PermissionTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductKey = "maintainarr",
            PermissionKey = "maintainarr.work_order.release",
            Name = "Release Work Order",
            Description = "Release a work order for execution.",
            Status = "active",
            PermissionScope = "tenant",
            Sensitivity = "critical",
            LastSyncedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        });
        db.PermissionTemplates.Add(new PermissionTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductKey = "staffarr",
            PermissionKey = "staffarr.platform_admin.manage",
            Name = "Manage Platform Admin",
            Description = "NexArr-owned platform administrator permission.",
            Status = "active",
            PermissionScope = "tenant",
            Sensitivity = "critical",
            LastSyncedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        });
        db.PermissionTemplates.Add(new PermissionTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductKey = "maintainarr",
            PermissionKey = "maintainarr.hidden.deprecated",
            Name = "Deprecated Hidden Permission",
            Status = "inactive",
            PermissionScope = "tenant",
            Sensitivity = "standard",
            CreatedAt = now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();

        var audit = new NoOpStaffArrAuditService();
        var service = new RoleManagementService(db, audit, new StaffArrTenantSettingsService(db, audit));

        await service.ListRolesAsync(tenantId);

        var catalogs = await service.GetPermissionCatalogsAsync(tenantId, entitlements: []);
        var allPermissions = catalogs
            .SelectMany(catalog => catalog.Modules
                .SelectMany(module => module.PermissionGroups)
                .SelectMany(group => group.Permissions)
                .Select(permission => (catalog.ProductKey, PermissionKey: permission.Key)))
            .GroupBy(permission => $"{permission.ProductKey}|{permission.PermissionKey}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        Assert.Contains(allPermissions, permission =>
            permission.ProductKey == "maintainarr"
            && permission.PermissionKey == "maintainarr.work_order.release");
        Assert.DoesNotContain(allPermissions, permission =>
            permission.PermissionKey == "maintainarr.hidden.deprecated");

        var fullAccessRoles = await db.StaffRoles
            .Where(role => role.TenantId == tenantId
                && (role.Name == "Owner"
                    || role.Name == "Platform Admin"
                    || role.Name == TenantAdminPermissionInheritanceRules.TenantAdminSystemTemplateName))
            .ToListAsync();
        Assert.Equal(3, fullAccessRoles.Count);
        var ownerAndPlatformRoles = fullAccessRoles
            .Where(role => !TenantAdminPermissionInheritanceRules.IsTenantAdminSystemTemplateName(role.Name))
            .ToList();

        foreach (var role in ownerAndPlatformRoles)
        {
            var permissions = await db.StaffRolePermissions
                .Where(permission => permission.TenantId == tenantId && permission.RoleId == role.Id)
                .ToListAsync();
            var scopes = await db.StaffRoleScopes
                .Where(scope => scope.TenantId == tenantId && scope.RoleId == role.Id)
                .ToListAsync();

            Assert.Equal(allPermissions.Count, permissions.Count);
            Assert.DoesNotContain(permissions, permission =>
                permission.Effect.Equals("deny", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(scopes, scope =>
                scope.ScopeType == "tenant"
                && scope.ScopeRefId is null);

            foreach (var catalogPermission in allPermissions)
            {
                Assert.Contains(permissions, permission =>
                    permission.ProductKey == catalogPermission.ProductKey
                    && permission.PermissionKey == catalogPermission.PermissionKey
                    && permission.Effect == "allow");
            }
        }

        var tenantAdminRole = Assert.Single(
            fullAccessRoles,
            role => TenantAdminPermissionInheritanceRules.IsTenantAdminSystemTemplateName(role.Name));
        var tenantAdminPermissions = await db.StaffRolePermissions
            .Where(permission => permission.TenantId == tenantId && permission.RoleId == tenantAdminRole.Id)
            .ToListAsync();
        var tenantAdminScopes = await db.StaffRoleScopes
            .Where(scope => scope.TenantId == tenantId && scope.RoleId == tenantAdminRole.Id)
            .ToListAsync();
        var nonPlatformAdminPermissions = allPermissions
            .Where(permission => !TenantAdminPermissionInheritanceRules.IsPlatformAdminPermission(
                permission.ProductKey,
                permission.PermissionKey))
            .ToList();

        Assert.Equal(nonPlatformAdminPermissions.Count, tenantAdminPermissions.Count);
        Assert.DoesNotContain(tenantAdminPermissions, permission =>
            TenantAdminPermissionInheritanceRules.IsPlatformAdminPermission(
                permission.ProductKey,
                permission.PermissionKey));
        Assert.Contains(tenantAdminScopes, scope =>
            scope.ScopeType == "tenant"
            && scope.ScopeRefId is null);

        foreach (var catalogPermission in nonPlatformAdminPermissions)
        {
            Assert.Contains(tenantAdminPermissions, permission =>
                permission.ProductKey == catalogPermission.ProductKey
                && permission.PermissionKey == catalogPermission.PermissionKey
                && permission.Effect == "allow");
        }
    }

    [Fact]
    public async Task Standard_staff_roles_can_be_assigned_and_report_effective_permissions()
    {
        var options = new DbContextOptionsBuilder<StaffArrDbContext>()
            .UseInMemoryDatabase($"staffarr-standard-role-assignment-{Guid.NewGuid():N}")
            .Options;

        await using var db = new StaffArrDbContext(options);
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = tenantId,
            GivenName = "Standard",
            FamilyName = "Worker",
            DisplayName = "Standard Worker",
            PrimaryEmail = "standard.worker@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();

        var audit = new NoOpStaffArrAuditService();
        var service = new RoleManagementService(db, audit, new StaffArrTenantSettingsService(db, audit));

        var roles = await service.ListRolesAsync(tenantId);
        var technician = Assert.Single(roles, role => role.Name == "Technician");
        Assert.True(technician.IsSystem);
        Assert.True(technician.PermissionCount > 0);

        var assignments = await service.SetPersonRolesAsync(
            tenantId,
            actorUserId,
            personId,
            personId,
            new SetStaffPersonRolesRequest(
                [new SetStaffPersonRoleItemRequest(technician.RoleId, "tenant", null, null, null)]),
            CancellationToken.None);

        var assignment = Assert.Single(assignments);
        Assert.Equal(technician.RoleId, assignment.RoleId);
        Assert.True(assignment.RoleIsSystem);

        var projection = await service.ComputeEffectivePermissionProjectionAsync(tenantId, personId);
        var workOrderEdit = Assert.Single(
            projection.Permissions,
            permission => permission.PermissionKey == "maintainarr.work_orders.edit");
        var source = Assert.Single(workOrderEdit.Sources);
        Assert.Equal(technician.RoleId, source.RoleId);
        Assert.Equal("staffarr.standard.technician", source.RoleKey);
        Assert.Equal("Technician", source.RoleName);
        Assert.Equal("tenant", workOrderEdit.ScopeType);
        Assert.Null(workOrderEdit.ScopeValue);
    }

    [Fact]
    public async Task ListRolesAsync_collapses_duplicate_system_templates()
    {
        var options = new DbContextOptionsBuilder<StaffArrDbContext>()
            .UseInMemoryDatabase($"staffarr-duplicate-system-roles-{Guid.NewGuid():N}")
            .Options;

        await using var db = new StaffArrDbContext(options);
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var olderRoleId = Guid.NewGuid();
        var newerRoleId = Guid.NewGuid();

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = tenantId,
            GivenName = "Duplicate",
            FamilyName = "Tester",
            DisplayName = "Duplicate Tester",
            PrimaryEmail = "duplicate.tester@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.StaffRoles.Add(new StaffRole
        {
            Id = olderRoleId,
            TenantId = tenantId,
            Name = "Technician",
            Description = "Technician starter template.",
            RoleType = "system_template",
            IsSystem = true,
            IsArchived = false,
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now.AddDays(-1)
        });
        db.StaffRoles.Add(new StaffRole
        {
            Id = newerRoleId,
            TenantId = tenantId,
            Name = "Technician",
            Description = "Technician starter template.",
            RoleType = "system_template",
            IsSystem = true,
            IsArchived = false,
            CreatedAt = now,
            UpdatedAt = now
        });
        db.StaffRolePermissions.Add(new StaffRolePermission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleId = newerRoleId,
            ProductKey = "maintainarr",
            PermissionKey = "maintainarr.work_orders.create",
            Effect = "allow",
            CreatedAt = now
        });
        db.StaffRoleScopes.Add(new StaffRoleScope
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleId = newerRoleId,
            ScopeType = "tenant",
            ScopeRefSnapshot = "Entire tenant",
            CreatedAt = now
        });
        db.StaffPersonRoles.Add(new StaffPersonRole
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            RoleId = newerRoleId,
            AssignmentScopeType = "tenant",
            CreatedAt = now
        });
        await db.SaveChangesAsync();

        var audit = new NoOpStaffArrAuditService();
        var service = new RoleManagementService(db, audit, new StaffArrTenantSettingsService(db, audit));

        var roles = await service.ListRolesAsync(tenantId);
        var technician = Assert.Single(roles, role => role.Name == "Technician");
        Assert.Equal(olderRoleId, technician.RoleId);

        var persistedTechnicians = await db.StaffRoles
            .Where(role => role.TenantId == tenantId && role.Name == "Technician")
            .ToListAsync();
        Assert.Single(persistedTechnicians);
        Assert.Equal(olderRoleId, persistedTechnicians[0].Id);

        var technicianPermissions = await db.StaffRolePermissions
            .Where(permission => permission.TenantId == tenantId && permission.RoleId == olderRoleId)
            .ToListAsync();
        Assert.Contains(technicianPermissions, permission =>
            permission.ProductKey == "maintainarr"
            && permission.PermissionKey == "maintainarr.work_orders.create"
            && permission.Effect == "allow");

        var technicianScopes = await db.StaffRoleScopes
            .Where(scope => scope.TenantId == tenantId && scope.RoleId == olderRoleId)
            .ToListAsync();
        Assert.Contains(technicianScopes, scope =>
            scope.ScopeType == "tenant"
            && scope.ScopeRefId is null);

        var technicianAssignments = await db.StaffPersonRoles
            .Where(assignment => assignment.TenantId == tenantId && assignment.RoleId == olderRoleId)
            .ToListAsync();
        Assert.Contains(technicianAssignments, assignment =>
            assignment.PersonId == personId
            && assignment.AssignmentScopeType == "tenant"
            && assignment.AssignmentScopeRefId is null);
    }

    private sealed class NoOpStaffArrAuditService : IStaffArrAuditService
    {
        public Task<StaffArrAuditWriteResult> WriteAsync(
            string action,
            Guid tenantId,
            Guid? actorUserId,
            string targetType,
            string? targetId,
            string result,
            string? reasonCode = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new StaffArrAuditWriteResult(Guid.NewGuid(), DateTimeOffset.UtcNow));

        public Task<StaffArrAuditWriteResult> WriteWithMetadataAsync(
            string action,
            Guid tenantId,
            Guid? actorUserId,
            string targetType,
            string? targetId,
            string result,
            string? metadataJson,
            string? reasonCode = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new StaffArrAuditWriteResult(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }
}
