using Microsoft.EntityFrameworkCore;
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

        var service = new RoleManagementService(db, new NoOpStaffArrAuditService());

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
                && (role.Name == "Owner" || role.Name == "Platform Admin"))
            .ToListAsync();
        Assert.Equal(2, fullAccessRoles.Count);

        foreach (var role in fullAccessRoles)
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
    }
}
