using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Options;

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

    private static readonly (string ProductKey, string BaseUrl, string LaunchPath)[] DefaultLaunchProfiles =
    [
        ("nexarr", "http://localhost:5101", "/"),
        ("staffarr", "http://localhost:5102", "/launch"),
        ("trainarr", "http://localhost:5103", "/launch"),
        ("maintainarr", "http://localhost:5104", "/launch"),
        ("routarr", "http://localhost:5105", "/launch"),
        ("supplyarr", "http://localhost:5106", "/launch"),
        ("compliancecore", "http://localhost:5107", "/launch")
    ];

    public static async Task SeedAsync(
        NexArrDbContext db,
        IPasswordHasher passwordHasher,
        StlLaunchOptions? launchOptions = null,
        CancellationToken cancellationToken = default)
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

        SeedLaunchProfiles(db, launchOptions, now);
        SeedCallbackAllowlist(db, now);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void SeedLaunchProfiles(NexArrDbContext db, StlLaunchOptions? launchOptions, DateTimeOffset now)
    {
        foreach (var profile in DefaultLaunchProfiles)
        {
            var configured = launchOptions?.Products.GetValueOrDefault(profile.ProductKey);
            db.LaunchProfiles.Add(new ProductLaunchProfile
            {
                ProductKey = profile.ProductKey,
                BaseUrl = configured?.BaseUrl ?? profile.BaseUrl,
                LaunchPath = configured?.LaunchPath ?? profile.LaunchPath,
                IsActive = true,
                ModifiedAt = now
            });
        }
    }

    private static readonly string[] SuiteShellOrigins =
    [
        "http://localhost:5173",
        "http://localhost:5174"
    ];

    public static async Task EnsureDevSuiteShellOriginsAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken = default)
    {
        foreach (var product in Products)
        {
            foreach (var origin in SuiteShellOrigins)
            {
                var exists = await db.CallbackAllowlist.AnyAsync(
                    e => e.ProductKey == product.Key
                        && e.TenantId == null
                        && e.UrlPattern == origin
                        && e.IsActive,
                    cancellationToken);
                if (exists)
                {
                    continue;
                }

                var now = DateTimeOffset.UtcNow;
                db.CallbackAllowlist.Add(new ProductCallbackAllowlistEntry
                {
                    Id = Guid.NewGuid(),
                    ProductKey = product.Key,
                    TenantId = null,
                    UrlPattern = origin,
                    PatternType = CallbackPatternTypes.Origin,
                    IsActive = true,
                    CreatedAt = now,
                    ModifiedAt = now
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void SeedCallbackAllowlist(NexArrDbContext db, DateTimeOffset now)
    {
        foreach (var product in Products)
        {
            foreach (var suiteShellOrigin in SuiteShellOrigins)
            {
                db.CallbackAllowlist.Add(new ProductCallbackAllowlistEntry
                {
                    Id = Guid.NewGuid(),
                    ProductKey = product.Key,
                    TenantId = null,
                    UrlPattern = suiteShellOrigin,
                    PatternType = CallbackPatternTypes.Origin,
                    IsActive = true,
                    CreatedAt = now,
                    ModifiedAt = now
                });
            }

            var apiOrigin = DefaultLaunchProfiles.First(p => p.ProductKey == product.Key).BaseUrl;
            db.CallbackAllowlist.Add(new ProductCallbackAllowlistEntry
            {
                Id = Guid.NewGuid(),
                ProductKey = product.Key,
                TenantId = DemoTenantId,
                UrlPattern = apiOrigin,
                PatternType = CallbackPatternTypes.Origin,
                IsActive = true,
                CreatedAt = now,
                ModifiedAt = now
            });
        }
    }
}
