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

    private static readonly ProductSeed[] Products =
    [
        new("shared-worker", "STL Shared Worker", 5, "platform", "Platform Engineering", "worker", "", "", "stl:shared-worker:worker"),
        new("nexarr-worker", "NexArr Worker", 8, "platform", "Platform Engineering", "worker", "", "", "stl:nexarr-worker:worker"),
        new("nexarr", "NexArr", 10, "platform", "Platform Engineering", "available", "http://localhost:5101", "http://localhost:5101/health/ready", "stl:nexarr:api"),
        new("staffarr", "StaffArr", 20, "workforce", "People Operations", "available", "http://localhost:5102", "http://localhost:5102/health/ready", "stl:staffarr:api"),
        new("trainarr", "TrainArr", 30, "training", "Learning and Qualification", "available", "http://localhost:5103", "http://localhost:5103/health/ready", "stl:trainarr:api"),
        new("maintainarr", "MaintainArr", 40, "maintenance", "Maintenance Operations", "available", "http://localhost:5104", "http://localhost:5104/health/ready", "stl:maintainarr:api"),
        new("routarr", "RoutArr", 50, "transportation", "Transportation Operations", "available", "http://localhost:5105", "http://localhost:5105/health/ready", "stl:routarr:api"),
        new("supplyarr", "SupplyArr", 60, "supply-chain", "Procurement Operations", "available", "http://localhost:5106", "http://localhost:5106/health/ready", "stl:supplyarr:api"),
        new("compliancecore", "Compliance Core", 70, "compliance", "Compliance Platform", "available", "http://localhost:5107", "http://localhost:5107/health/ready", "stl:compliancecore:api"),
        new("loadarr", "LoadArr", 75, "warehouse", "Warehouse Operations", "available", "http://localhost:5108", "http://localhost:5108/health/ready", "stl:loadarr:api"),
        new("assurarr", "AssurArr", 76, "assurance", "Quality and Assurance", "available", "http://localhost:5109", "http://localhost:5109/health/ready", "stl:assurarr:api"),
        new("reportarr", "ReportArr", 77, "analytics", "Analytics and Reporting", "available", "http://localhost:5111", "http://localhost:5111/health/ready", "stl:reportarr:api"),
        new("recordarr", "RecordArr", 78, "records", "Records and Evidence", "available", "http://localhost:5110", "http://localhost:5110/health/ready", "stl:recordarr:api"),
        new("fieldcompanion", "Field Companion", 80, "field-execution", "Platform Engineering", "available", "", "", "stl:fieldcompanion:frontend")
    ];

    private static readonly (string ProductKey, string BaseUrl, string LaunchPath)[] DefaultLaunchProfiles =
    [
        ("nexarr", "http://localhost:5101", "/"),
        ("staffarr", "http://localhost:5175", "/launch"),
        ("trainarr", "http://localhost:5176", "/launch"),
        ("maintainarr", "http://localhost:5178", "/launch"),
        ("routarr", "http://localhost:5180", "/launch"),
        ("supplyarr", "http://localhost:5179", "/launch"),
        ("compliancecore", "http://localhost:5177", "/launch"),
        ("loadarr", "http://localhost:5182", "/launch"),
        ("assurarr", "http://localhost:5183", "/launch"),
        ("reportarr", "http://localhost:5185", "/launch"),
        ("recordarr", "http://localhost:5184", "/handoff"),
        ("fieldcompanion", "http://localhost:5181", "/launch")
    ];

    public static async Task SeedAsync(
        NexArrDbContext db,
        IPasswordHasher passwordHasher,
        StlLaunchOptions? launchOptions = null,
        PlatformProductUrlsOptions? platformProductUrls = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        await EnsureProductCatalogManifestColumnsAsync(db, cancellationToken);
        await EnsurePlatformSessionSettingsColumnsAsync(db, cancellationToken);
        await EnsureProductCatalogAsync(db, platformProductUrls, cancellationToken);
        await SeedLaunchProfilesAsync(db, launchOptions, now, cancellationToken);

        if (!await db.Tenants.AnyAsync(t => t.Id == DemoTenantId, cancellationToken))
        {
            db.Tenants.Add(new Tenant
            {
                Id = DemoTenantId,
                Slug = "demo-stl",
                DisplayName = "STL Demo Tenant",
                Status = TenantStatuses.Active,
                CreatedAt = now,
                ModifiedAt = now
            });
        }

        if (!await db.Users.AnyAsync(u => u.Id == DemoAdminUserId, cancellationToken))
        {
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
        }

        if (!await db.TenantMemberships.AnyAsync(
                m => m.TenantId == DemoTenantId && m.UserId == DemoAdminUserId,
                cancellationToken))
        {
            db.TenantMemberships.Add(new TenantMembership
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333301"),
                TenantId = DemoTenantId,
                UserId = DemoAdminUserId,
                RoleKey = "platform_admin",
                IsActive = true,
                CreatedAt = now
            });
        }

        await EnsureDemoOwnerRoleAsync(db, cancellationToken);

        if (!await db.Users.AnyAsync(u => u.Id == DemoTenantAdminUserId, cancellationToken))
        {
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
        }

        if (!await db.TenantMemberships.AnyAsync(
                m => m.TenantId == DemoTenantId && m.UserId == DemoTenantAdminUserId,
                cancellationToken))
        {
            db.TenantMemberships.Add(new TenantMembership
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333302"),
                TenantId = DemoTenantId,
                UserId = DemoTenantAdminUserId,
                RoleKey = "tenant_admin",
                IsActive = true,
                CreatedAt = now
            });
        }

        foreach (var product in Products)
        {
            if (await db.Entitlements.AnyAsync(
                    e => e.TenantId == DemoTenantId && e.ProductKey == product.Key,
                    cancellationToken))
            {
                continue;
            }

            db.Entitlements.Add(new TenantProductEntitlement
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantId,
                ProductKey = product.Key,
                Status = EntitlementStatuses.Active,
                GrantedAt = now
            });
        }

        foreach (var product in Products)
        {
            if (await db.TenantProductLicenses.AnyAsync(
                    l => l.TenantId == DemoTenantId && l.ProductKey == product.Key,
                    cancellationToken))
            {
                continue;
            }

            db.TenantProductLicenses.Add(new TenantProductLicense
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantId,
                ProductKey = product.Key,
                Status = LicenseStatuses.Active,
                ValidFrom = now.AddYears(-1),
                ValidTo = now.AddYears(1),
                CreatedAt = now,
                ModifiedAt = now,
            });
        }

        SeedCallbackAllowlist(db, now);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureDemoOwnerRoleAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken)
    {
        var hasOwner = await db.PlatformRoleAssignments.AnyAsync(
            x => x.UserId == DemoAdminUserId
                 && x.TenantId == null
                 && x.RoleKey == "platform_owner",
            cancellationToken);
        if (hasOwner)
        {
            return;
        }

        db.PlatformRoleAssignments.Add(new PlatformRoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = DemoAdminUserId,
            TenantId = null,
            RoleKey = "platform_owner",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = DemoAdminUserId,
        });
    }

    private static async Task EnsureProductCatalogManifestColumnsAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken)
    {
        if (!db.Database.IsRelational())
        {
            return;
        }

        // Keep startup resilient when a deployed DB has older `product_catalog` shape.
        await db.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE product_catalog
                ADD COLUMN IF NOT EXISTS "ProductCategory" character varying(64) NOT NULL DEFAULT 'operations',
                ADD COLUMN IF NOT EXISTS "ProductOwner" character varying(128) NOT NULL DEFAULT 'STL Compliance',
                ADD COLUMN IF NOT EXISTS "ProductStatus" character varying(32) NOT NULL DEFAULT 'available',
                ADD COLUMN IF NOT EXISTS "CanonicalCallbackPath" character varying(128) NOT NULL DEFAULT '/auth/nexarr/callback',
                ADD COLUMN IF NOT EXISTS "ApiBaseUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "HealthUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "ServiceAudience" character varying(128) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "MarketingUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "DocumentationUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "SupportUrl" character varying(512) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "EnvironmentKey" character varying(64) NOT NULL DEFAULT 'local',
                ADD COLUMN IF NOT EXISTS "EntitlementDependencyRules" character varying(2048) NOT NULL DEFAULT '',
                ADD COLUMN IF NOT EXISTS "ProductDependencyMetadata" character varying(2048) NOT NULL DEFAULT '';
            """,
            cancellationToken);
    }

    private static async Task EnsurePlatformSessionSettingsColumnsAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken)
    {
        if (!db.Database.IsRelational())
        {
            return;
        }

        // Keep login and password-policy reads resilient when a deployed DB
        // is missing the newer session-settings columns.
        await db.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE nexarr_platform_session_settings
                ADD COLUMN IF NOT EXISTS "RequirePlatformAdminMfa" boolean NULL,
                ADD COLUMN IF NOT EXISTS "PasswordMinLength" integer NOT NULL DEFAULT 12,
                ADD COLUMN IF NOT EXISTS "RequirePasswordComplexity" boolean NOT NULL DEFAULT true;
            """,
            cancellationToken);
    }

    private static async Task EnsureProductCatalogAsync(
        NexArrDbContext db,
        PlatformProductUrlsOptions? platformProductUrls,
        CancellationToken cancellationToken)
    {
        foreach (var seed in Products)
        {
            var effectiveSeed = ApplyConfiguredProductUrl(seed, platformProductUrls);
            var product = await db.ProductCatalog
                .FirstOrDefaultAsync(p => p.ProductKey == effectiveSeed.Key, cancellationToken);
            if (product is null)
            {
                db.ProductCatalog.Add(CreateProduct(effectiveSeed));
                continue;
            }

            ApplyMissingManifestMetadata(product, effectiveSeed);
        }

        static ProductCatalogItem CreateProduct(ProductSeed seed) =>
            new()
            {
                ProductKey = seed.Key,
                DisplayName = seed.Name,
                ProductCategory = seed.Category,
                ProductOwner = seed.Owner,
                ProductStatus = seed.Status,
                SortOrder = seed.Order,
                IsActive = true,
                CanonicalCallbackPath = "/auth/nexarr/callback",
                ApiBaseUrl = seed.ApiBaseUrl,
                HealthUrl = seed.HealthUrl,
                ServiceAudience = seed.ServiceAudience,
                MarketingUrl = $"https://stlcompliance.com/products/{seed.Key}",
                DocumentationUrl = $"https://stlcompliance.com/docs/{seed.Key}",
                SupportUrl = "https://stlcompliance.com/support",
                EnvironmentKey = ResolveEnvironmentKey(seed.ApiBaseUrl),
                EntitlementDependencyRules = seed.Key is "shared-worker" or "nexarr-worker"
                    ? "internal-platform-worker"
                    : "tenant-product-entitlement-required",
                ProductDependencyMetadata = ResolveDependencyMetadata(seed.Key),
            };

        static void ApplyMissingManifestMetadata(ProductCatalogItem product, ProductSeed seed)
        {
            if (string.IsNullOrWhiteSpace(product.ProductCategory))
            {
                product.ProductCategory = seed.Category;
            }

            if (seed.Key is "fieldcompanion"
                && product.DisplayName.Equals("fieldcompanion App", StringComparison.OrdinalIgnoreCase))
            {
                product.DisplayName = seed.Name;
            }

            if (string.IsNullOrWhiteSpace(product.ProductOwner))
            {
                product.ProductOwner = seed.Owner;
            }

            if (string.IsNullOrWhiteSpace(product.ProductStatus))
            {
                product.ProductStatus = seed.Status;
            }

            if (string.IsNullOrWhiteSpace(product.CanonicalCallbackPath))
            {
                product.CanonicalCallbackPath = "/auth/nexarr/callback";
            }

            if (string.IsNullOrWhiteSpace(product.ApiBaseUrl))
            {
                product.ApiBaseUrl = seed.ApiBaseUrl;
            }
            else if (IsLocalUrl(product.ApiBaseUrl) && !IsLocalUrl(seed.ApiBaseUrl))
            {
                product.ApiBaseUrl = seed.ApiBaseUrl;
            }

            if (string.IsNullOrWhiteSpace(product.HealthUrl))
            {
                product.HealthUrl = seed.HealthUrl;
            }
            else if (IsLocalUrl(product.HealthUrl) && !IsLocalUrl(seed.HealthUrl))
            {
                product.HealthUrl = seed.HealthUrl;
            }

            if (string.IsNullOrWhiteSpace(product.ServiceAudience))
            {
                product.ServiceAudience = seed.ServiceAudience;
            }

            if (string.IsNullOrWhiteSpace(product.MarketingUrl))
            {
                product.MarketingUrl = $"https://stlcompliance.com/products/{seed.Key}";
            }

            if (string.IsNullOrWhiteSpace(product.DocumentationUrl))
            {
                product.DocumentationUrl = $"https://stlcompliance.com/docs/{seed.Key}";
            }

            if (string.IsNullOrWhiteSpace(product.SupportUrl))
            {
                product.SupportUrl = "https://stlcompliance.com/support";
            }

            if (string.IsNullOrWhiteSpace(product.EnvironmentKey))
            {
                product.EnvironmentKey = "local";
            }
            else if (product.EnvironmentKey.Equals("local", StringComparison.OrdinalIgnoreCase)
                && ShouldUseProductionEnvironmentKey(seed.ApiBaseUrl))
            {
                product.EnvironmentKey = "production";
            }

            if (string.IsNullOrWhiteSpace(product.EntitlementDependencyRules))
            {
                product.EntitlementDependencyRules = seed.Key is "shared-worker" or "nexarr-worker"
                    ? "internal-platform-worker"
                    : "tenant-product-entitlement-required";
            }

            if (string.IsNullOrWhiteSpace(product.ProductDependencyMetadata))
            {
                product.ProductDependencyMetadata = ResolveDependencyMetadata(seed.Key);
            }
        }
    }

    private static ProductSeed ApplyConfiguredProductUrl(
        ProductSeed seed,
        PlatformProductUrlsOptions? platformProductUrls)
    {
        var configuredBaseUrl = seed.Key switch
        {
            "nexarr" => platformProductUrls?.NexArrBaseUrl,
            "staffarr" => platformProductUrls?.StaffArrBaseUrl,
            "trainarr" => platformProductUrls?.TrainArrBaseUrl,
            "maintainarr" => platformProductUrls?.MaintainArrBaseUrl,
            "routarr" => platformProductUrls?.RoutArrBaseUrl,
            "supplyarr" => platformProductUrls?.SupplyArrBaseUrl,
            "compliancecore" => platformProductUrls?.ComplianceCoreBaseUrl,
            "loadarr" => platformProductUrls?.LoadArrBaseUrl,
            "assurarr" => platformProductUrls?.AssurArrBaseUrl,
            "reportarr" => platformProductUrls?.ReportArrBaseUrl,
            "recordarr" => platformProductUrls?.RecordArrBaseUrl,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            return seed;
        }

        var normalizedBaseUrl = configuredBaseUrl.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(normalizedBaseUrl) || string.Equals(normalizedBaseUrl, seed.ApiBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return seed;
        }

        return seed with
        {
            ApiBaseUrl = normalizedBaseUrl,
            HealthUrl = $"{normalizedBaseUrl}/health/ready"
        };
    }

    private static bool IsLocalUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && (url.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            || url.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || url.Contains("[::1]", StringComparison.OrdinalIgnoreCase));

    private static string ResolveEnvironmentKey(string? apiBaseUrl) =>
        ShouldUseProductionEnvironmentKey(apiBaseUrl) ? "production" : "local";

    private static bool ShouldUseProductionEnvironmentKey(string? apiBaseUrl) =>
        !string.IsNullOrWhiteSpace(apiBaseUrl) && !IsLocalUrl(apiBaseUrl);

    private static string ResolveDependencyMetadata(string productKey) =>
        productKey switch
        {
            "nexarr" => "identity,tenant,entitlement,launch,service-token",
            "staffarr" => "nexarr,trainarr,compliancecore",
            "trainarr" => "nexarr,staffarr,compliancecore",
            "maintainarr" => "nexarr,staffarr,trainarr,supplyarr,compliancecore",
            "routarr" => "nexarr,staffarr,trainarr,maintainarr,supplyarr,compliancecore",
            "supplyarr" => "nexarr,staffarr,maintainarr,routarr,trainarr,compliancecore",
            "compliancecore" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr",
            "loadarr" => "nexarr,staffarr,supplyarr,routarr",
            "assurarr" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr,compliancecore,loadarr,recordarr",
            "reportarr" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr,compliancecore,loadarr,recordarr,assurarr",
            "recordarr" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr,compliancecore,loadarr",
            "fieldcompanion" => "nexarr,staffarr,trainarr,maintainarr,routarr,supplyarr",
            _ => "nexarr"
        };

    private static async Task SeedLaunchProfilesAsync(
        NexArrDbContext db,
        StlLaunchOptions? launchOptions,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        foreach (var profile in DefaultLaunchProfiles)
        {
            var configured = launchOptions?.Products.GetValueOrDefault(profile.ProductKey);
            var baseUrl = configured?.BaseUrl ?? profile.BaseUrl;
            var launchPath = configured?.LaunchPath ?? profile.LaunchPath;
            var existing = await db.LaunchProfiles
                .FirstOrDefaultAsync(p => p.ProductKey == profile.ProductKey, cancellationToken);

            if (existing is not null)
            {
                if (configured is not null
                    && (!string.Equals(existing.BaseUrl, baseUrl, StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(existing.LaunchPath, launchPath, StringComparison.OrdinalIgnoreCase)))
                {
                    existing.BaseUrl = baseUrl;
                    existing.LaunchPath = launchPath;
                    existing.IsActive = true;
                    existing.ModifiedAt = now;
                }

                continue;
            }

            db.LaunchProfiles.Add(new ProductLaunchProfile
            {
                ProductKey = profile.ProductKey,
                BaseUrl = baseUrl,
                LaunchPath = launchPath,
                IsActive = true,
                ModifiedAt = now
            });
        }
    }

    private static readonly string[] SuiteShellOrigins =
    [
        "http://localhost:5173",
        "http://localhost:5174",
        "http://localhost:5175"
    ];

    public static Task EnsureDevSuiteShellOriginsAsync(
        NexArrDbContext db,
        CancellationToken cancellationToken = default) =>
        EnsureSuiteShellOriginsAsync(db, SuiteShellOrigins, cancellationToken);

    public static async Task EnsureSuiteShellOriginsAsync(
        NexArrDbContext db,
        IEnumerable<string> origins,
        CancellationToken cancellationToken = default)
    {
        foreach (var product in Products)
        {
            foreach (var origin in origins
                         .Select(NormalizeCallbackOrigin)
                         .Where(static value => !string.IsNullOrWhiteSpace(value))
                         .Select(static value => value!))
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

    private static string? NormalizeCallbackOrigin(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim().TrimEnd('/');
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return $"{uri.Scheme}://{uri.Authority}";
        }

        return trimmed;
    }

    private static void SeedCallbackAllowlist(NexArrDbContext db, DateTimeOffset now)
    {
        foreach (var product in Products)
        {
            foreach (var suiteShellOrigin in SuiteShellOrigins
                         .Select(NormalizeCallbackOrigin)
                         .Where(static value => !string.IsNullOrWhiteSpace(value))
                         .Select(static value => value!))
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

            var launchProfileIndex = Array.FindIndex(
                DefaultLaunchProfiles,
                p => string.Equals(p.ProductKey, product.Key, StringComparison.Ordinal));
            if (launchProfileIndex < 0)
            {
                continue;
            }

            var launchProfile = DefaultLaunchProfiles[launchProfileIndex];
            var launchProfileOrigin = NormalizeCallbackOrigin(launchProfile.BaseUrl);
            if (string.IsNullOrWhiteSpace(launchProfileOrigin))
            {
                continue;
            }

            db.CallbackAllowlist.Add(new ProductCallbackAllowlistEntry
            {
                Id = Guid.NewGuid(),
                ProductKey = product.Key,
                TenantId = DemoTenantId,
                UrlPattern = launchProfileOrigin,
                PatternType = CallbackPatternTypes.Origin,
                IsActive = true,
                CreatedAt = now,
                ModifiedAt = now
            });
        }
    }

    private sealed record ProductSeed(
        string Key,
        string Name,
        int Order,
        string Category,
        string Owner,
        string Status,
        string ApiBaseUrl,
        string HealthUrl,
        string ServiceAudience);
}
