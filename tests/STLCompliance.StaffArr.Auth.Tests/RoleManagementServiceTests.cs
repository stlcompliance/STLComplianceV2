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
    public async Task RoleTemplate_projection_tenant_admin_inherits_all_non_platform_admin_permissions()
    {
        var options = new DbContextOptionsBuilder<StaffArrDbContext>()
            .UseInMemoryDatabase($"staffarr-role-template-inheritance-{Guid.NewGuid():N}")
            .Options;

        await using var db = new StaffArrDbContext(options);
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = tenantId,
            GivenName = "Tenant",
            FamilyName = "Admin",
            DisplayName = "Tenant Admin",
            PrimaryEmail = "tenant.admin@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });
        db.PermissionTemplates.AddRange(
            new PermissionTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductKey = "staffarr",
                PermissionKey = "staffarr.people.read",
                Name = "Read People",
                Status = "active",
                PermissionScope = "tenant",
                Sensitivity = "standard",
                CreatedAt = now,
                UpdatedAt = now
            },
            new PermissionTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductKey = "maintainarr",
                PermissionKey = "maintainarr.work_order.release",
                Name = "Release Work Order",
                Status = "active",
                PermissionScope = "tenant",
                Sensitivity = "critical",
                CreatedAt = now,
                UpdatedAt = now
            },
            new PermissionTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductKey = "staffarr",
                PermissionKey = "staffarr.platform_admin.manage",
                Name = "Manage Platform Admin",
                Status = "active",
                PermissionScope = "tenant",
                Sensitivity = "critical",
                CreatedAt = now,
                UpdatedAt = now
            },
            new PermissionTemplate
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductKey = "staffarr",
                PermissionKey = "staffarr.inactive.read",
                Name = "Inactive Permission",
                Status = "inactive",
                PermissionScope = "tenant",
                Sensitivity = "standard",
                CreatedAt = now,
                UpdatedAt = now
            });
        db.RoleTemplates.Add(new RoleTemplate
        {
            Id = roleId,
            TenantId = tenantId,
            RoleKey = TenantAdminPermissionInheritanceRules.TenantAdminRoleKey,
            Name = "Tenant Admin",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });
        db.PersonRoleAssignments.Add(new PersonRoleAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            RoleTemplateId = roleId,
            ScopeType = "tenant",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();

        var service = new RoleTemplateService(db, new NoOpStaffArrAuditService());
        var projection = await service.ComputeEffectivePermissionProjectionAsync(tenantId, personId);

        Assert.Contains(projection.Permissions, permission =>
            permission.PermissionKey == "staffarr.people.read");
        Assert.Contains(projection.Permissions, permission =>
            permission.PermissionKey == "maintainarr.work_order.release");
        Assert.DoesNotContain(projection.Permissions, permission =>
            permission.PermissionKey == "staffarr.platform_admin.manage");
        Assert.DoesNotContain(projection.Permissions, permission =>
            permission.PermissionKey == "staffarr.inactive.read");
        Assert.All(projection.Permissions, permission =>
        {
            var source = Assert.Single(permission.Sources);
            Assert.Equal(TenantAdminPermissionInheritanceRules.TenantAdminRoleKey, source.RoleKey);
            Assert.Equal("tenant", permission.ScopeType);
            Assert.Null(permission.ScopeValue);
        });
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
