using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public static class PlatformSeeder
{
    public static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid DemoAdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222201");

    public static readonly Guid DemoTenantAdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222202");

    public const string DemoAdminEmail = "admin@demo.stl";
    public const string DemoTenantAdminEmail = "tenant-admin@demo.stl";
    public const string DemoAdminPassword = "ChangeMe!Demo2026";

    private static readonly (string Key, string Name, int Order)[] Products =
    [
        ("nexarr", "NexArr", 10),
        ("staffarr", "StaffArr", 20),
        ("trainarr", "TrainArr", 30),
        ("maintainarr", "MaintainArr", 40),
        ("routarr", "RoutArr", 50),
        ("supplyarr", "SupplyArr", 60),
        ("compliancecore", "Compliance Core", 70)
    ];

    public static async Task SeedAsync(NexArrDbContext db, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        if (await db.ProductCatalog.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        foreach (var product in Products)
        {
            db.ProductCatalog.Add(new ProductCatalogItem
            {
                ProductKey = product.Key,
                DisplayName = product.Name,
                SortOrder = product.Order,
                IsActive = true
            });
        }

        db.Tenants.Add(new Tenant
        {
            Id = DemoTenantId,
            Slug = "demo-stl",
            DisplayName = "STL Demo Tenant",
            Status = TenantStatuses.Active,
            CreatedAt = now,
            ModifiedAt = now
        });

        db.Users.Add(new PlatformUser
        {
            Id = DemoAdminUserId,
            Email = DemoAdminEmail,
            DisplayName = "Demo Platform Admin",
            IsActive = true,
            IsPlatformAdmin = true,
            CreatedAt = now,
            ModifiedAt = now,
            Credential = new UserCredential
            {
                UserId = DemoAdminUserId,
                PasswordHash = passwordHasher.Hash(DemoAdminPassword),
                PasswordChangedAt = now
            }
        });

        db.TenantMemberships.Add(new TenantMembership
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333301"),
            TenantId = DemoTenantId,
            UserId = DemoAdminUserId,
            RoleKey = "platform_admin",
            IsActive = true,
            CreatedAt = now
        });

        db.Users.Add(new PlatformUser
        {
            Id = DemoTenantAdminUserId,
            Email = DemoTenantAdminEmail,
            DisplayName = "Demo Tenant Admin",
            IsActive = true,
            IsPlatformAdmin = false,
            CreatedAt = now,
            ModifiedAt = now,
            Credential = new UserCredential
            {
                UserId = DemoTenantAdminUserId,
                PasswordHash = passwordHasher.Hash(DemoAdminPassword),
                PasswordChangedAt = now
            }
        });

        db.TenantMemberships.Add(new TenantMembership
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333302"),
            TenantId = DemoTenantId,
            UserId = DemoTenantAdminUserId,
            RoleKey = "tenant_admin",
            IsActive = true,
            CreatedAt = now
        });

        foreach (var product in Products)
        {
            db.Entitlements.Add(new TenantProductEntitlement
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantId,
                ProductKey = product.Key,
                Status = EntitlementStatuses.Active,
                GrantedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
