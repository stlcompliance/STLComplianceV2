using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Options;

namespace NexArr.Api.Services;

public static class PlatformSeeder
{
    // Test/demo identifiers are kept for test fixtures and explicit admin input flows.
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
        ("recordarr", "http://localhost:5184", "/launch"),
        ("fieldcompanion", "http://localhost:5181", "/launch")
    ];

    private static readonly string[] MaintainArrReferenceDatasetNames =
    [
        "Asset Class",
        "Asset Type",
        "Asset Subtype",
        "Asset Category",
        "Asset Status",
        "Lifecycle Status",
        "Criticality",
        "Maintenance Class",
        "Service Group",
        "Readiness Status",
        "Operational Status",
        "Availability Status",
        "Make",
        "Manufacturer",
        "Model",
        "Model Year",
        "Trim",
        "Series",
        "Body Style",
        "Body Type",
        "Configuration",
        "Cab Type",
        "Sleeper Cab",
        "Day Cab",
        "Drivetrain",
        "Axle Configuration",
        "Steer Axle Count",
        "Drive Axle Count",
        "Trailer Axle Count",
        "Single Drive Axle",
        "Tandem Drive Axle",
        "Lift Axle Equipped",
        "Tag Axle Equipped",
        "Pusher Axle Equipped",
        "Dual Rear Wheel",
        "Super Singles Equipped",
        "Duals Equipped",
        "Four Wheel Drive",
        "All Wheel Drive",
        "Two Wheel Drive",
        "Tire Configuration",
        "Fuel Type",
        "Secondary Fuel Type",
        "DEF Required",
        "Aftertreatment Type",
        "EV Battery Type",
        "Charging Connector Type",
        "Hybrid Type",
        "CNG Equipped",
        "LNG Equipped",
        "Propane Equipped",
        "Brake System Type",
        "Brake Type",
        "Air Brakes",
        "Hydraulic Brakes",
        "Electric Brakes",
        "Drum Brakes",
        "Disc Brakes",
        "ABS Required",
        "ABS Equipped",
        "Slack Adjuster Type",
        "Brake Chamber Type",
        "Parking Brake Type",
        "Trailer Type",
        "Trailer Body Type",
        "Trailer Door Type",
        "Roof Type",
        "Floor Type",
        "Landing Gear Type",
        "Kingpin Type",
        "Reefer Equipped",
        "Lift Gate Equipped",
        "Axle Spread Type",
        "Suspension Type",
        "Engine Make",
        "Engine Model",
        "Engine Family",
        "Emissions Level",
        "Transmission Make",
        "Transmission Model",
        "Transmission Type",
        "PTO Equipped",
        "PTO Type",
        "Drive Type",
        "Tire Size",
        "Wheel Size",
        "Wheel Material",
        "Tire Position Layout",
        "Steer Tire Size",
        "Drive Tire Size",
        "Trailer Tire Size",
        "Spare Tire Equipped",
        "Retread Allowed",
        "Tread Depth Minimum Rule",
        "Torque Spec",
        "Pressure Spec",
        "Primary Meter Type",
        "Secondary Meter Types",
        "Meter Type",
        "Meter Unit",
        "Usage Profile",
        "Meter Reading Source",
        "Meter Rollover Behavior",
        "PM Program",
        "PM Program Status",
        "PM Template",
        "PM Type",
        "PM Interval Type",
        "Inspection Template Category",
        "Inspection Template Owner Role",
        "Inspection Execution Mode",
        "Inspection Result Mode",
        "Inspection Readiness Impact",
        "Inspection Template",
        "Inspection Type",
        "Required Inspection Types",
        "Inspection Frequency Type",
        "Seasonal PM Group",
        "Regulatory Inspection Required",
        "Annual Inspection Required",
        "DVIR Required",
        "Pre Trip Required",
        "Post Trip Required",
        "Shop Inspection Required",
        "Defect Type",
        "Defect Category",
        "Defect System",
        "Defect Component",
        "Report Source",
        "Symptom",
        "Failure Mode",
        "Severity",
        "Priority",
        "Safety Critical",
        "Operating Restriction",
        "Operating Condition",
        "Side Position",
        "Deferral Code",
        "Repair Disposition",
        "Root Cause",
        "Corrective Action Type",
        "Work Order Type",
        "Work Order Priority",
        "Maintenance Type",
        "Repair Type",
        "Labor Category",
        "Downtime Reason",
        "Shop Status",
        "Bay",
        "Approval Status",
        "Completion Status",
        "Return To Service Status",
        "Document Type",
        "Document Status",
        "Verification Status",
        "Expiration Type",
        "Fluid Spec",
        "Oil Spec",
        "Coolant Spec",
        "DEF Spec",
        "Filter Spec",
        "Belt Spec",
        "Battery Spec",
        "Lamp Spec",
        "Brake Part Spec",
        "Tire Spec",
        "Wheel Spec",
        "Telematics Provider",
        "Diagnostic Protocol",
        "ELD Provider",
        "GPS Provider",
        "Fault Code Source",
        "Fault Code Standard",
        "Data Sync Status"
    ];

    private static readonly string[] ComplianceCoreReferenceDatasetNames =
    [
        "Governing Bodies",
        "Jurisdictions",
        "Regulation Sources",
        "Citation Types",
        "Requirement Types",
        "Evidence Types",
        "Applicability Subject Types",
        "Asset Compliance Categories",
        "Training Compliance Categories",
        "Document Compliance Categories",
        "Incident Compliance Categories",
        "Maintenance Compliance Categories",
        "Transportation Compliance Categories",
        "Inventory Compliance Categories",
        "Supplier Compliance Categories",
        "Customer Requirement Categories",
        "Exception Types",
        "Exemption Types",
        "Severity Levels",
        "Confidence Levels",
        "Retention Triggers"
    ];

    private static readonly (string Key, string Name)[] SupplyArrReferenceDatasetNames =
    [
        ("party", "Party"),
        ("part", "Part"),
        ("purchase_request", "Purchase Request"),
        ("purchase_order", "Purchase Order"),
        ("receipt", "Receipt"),
        ("warranty_claim", "Warranty Claim")
    ];

    public static async Task SeedAsync(
        NexArrDbContext db,
        IPasswordHasher passwordHasher,
        StlLaunchOptions? launchOptions = null,
        PlatformProductUrlsOptions? platformProductUrls = null,
        CancellationToken cancellationToken = default)
    {
        await SeedInfrastructureAsync(db, launchOptions, platformProductUrls, cancellationToken);
        await SeedDemoBusinessDataAsync(db, passwordHasher, cancellationToken);
    }

    public static async Task SeedInfrastructureAsync(
        NexArrDbContext db,
        StlLaunchOptions? launchOptions = null,
        PlatformProductUrlsOptions? platformProductUrls = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        await EnsureProductCatalogManifestColumnsAsync(db, cancellationToken);
        await EnsurePlatformSessionSettingsColumnsAsync(db, cancellationToken);
        await EnsureProductCatalogAsync(db, platformProductUrls, cancellationToken);
        await SeedReferenceDatasetsAsync(db, now, cancellationToken);
        await SeedLaunchProfilesAsync(db, launchOptions, now, cancellationToken);
        SeedSuiteShellCallbackAllowlist(db, now);
        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedDemoBusinessDataAsync(
        NexArrDbContext db,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

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

        SeedDemoTenantCallbackAllowlist(db, now);
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

    private static void SeedSuiteShellCallbackAllowlist(NexArrDbContext db, DateTimeOffset now)
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

        }
    }

    private static async Task SeedReferenceDatasetsAsync(
        NexArrDbContext db,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existingDatasets = await db.ReferenceDatasets.ToListAsync(cancellationToken);
        var existingByKey = existingDatasets.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in GetReferenceDatasetSeeds())
        {
            var key = seed.Key;
            var category = seed.Category;

            if (existingByKey.TryGetValue(key, out var existing))
            {
                existing.Name = seed.Name;
                existing.Category = category;
                existing.OwnerService = seed.OwnerService;
                existing.Status = ReferenceDatasetStatuses.Ready;
                existing.UpdatedAt = now;
                continue;
            }

            db.ReferenceDatasets.Add(new ReferenceDataset
            {
                Id = Guid.NewGuid(),
                Key = key,
                Name = seed.Name,
                Category = category,
                OwnerService = seed.OwnerService,
                Status = ReferenceDatasetStatuses.Ready,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void SeedDemoTenantCallbackAllowlist(NexArrDbContext db, DateTimeOffset now)
    {
        foreach (var product in Products)
        {
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

    private static IEnumerable<ReferenceDatasetSeed> GetReferenceDatasetSeeds()
    {
        foreach (var datasetName in MaintainArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"MaintainArr {datasetName}"),
                datasetName,
                ResolveMaintainArrReferenceDatasetCategory(datasetName),
                "MaintainArr");
        }

        foreach (var datasetName in ComplianceCoreReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"ComplianceCore {datasetName}"),
                datasetName,
                "compliance",
                "Compliance Core");
        }

        foreach (var dataset in SupplyArrReferenceDatasetNames)
        {
            yield return new ReferenceDatasetSeed(
                NormalizeKey($"SupplyArr {dataset.Name}"),
                dataset.Name,
                "supply",
                "SupplyArr");
        }
    }

    private static string ResolveMaintainArrReferenceDatasetCategory(string datasetName) =>
        datasetName switch
        {
            "PM Program" or "PM Program Status" or "PM Template" or "PM Type" or "PM Interval Type" => "maintenance",
            "Work Order Type" or "Work Order Priority" or "Maintenance Type" or "Repair Type" or "Labor Category" or "Downtime Reason" or "Shop Status" or "Bay" or "Approval Status" or "Completion Status" or "Return To Service Status" => "maintenance",
            "Inspection Template Category" or "Inspection Template Owner Role" or "Inspection Execution Mode" or "Inspection Result Mode" or "Inspection Readiness Impact" or "Inspection Template" or "Inspection Type" or "Required Inspection Types" or "Inspection Frequency Type" or "Seasonal PM Group" or "Regulatory Inspection Required" or "Annual Inspection Required" or "DVIR Required" or "Pre Trip Required" or "Post Trip Required" or "Shop Inspection Required" => "inspection",
            "Defect Type" or "Defect Category" or "Defect System" or "Defect Component" or "Report Source" or "Symptom" or "Failure Mode" or "Severity" or "Priority" or "Safety Critical" or "Operating Restriction" or "Operating Condition" or "Side Position" or "Deferral Code" or "Repair Disposition" or "Root Cause" or "Corrective Action Type" => "defect",
            "Document Type" or "Document Status" or "Verification Status" or "Expiration Type" => "document",
            "Fluid Spec" or "Oil Spec" or "Coolant Spec" or "DEF Spec" or "Filter Spec" or "Belt Spec" or "Battery Spec" or "Lamp Spec" or "Brake Part Spec" or "Tire Spec" or "Wheel Spec" => "spec",
            "Telematics Provider" or "Diagnostic Protocol" or "ELD Provider" or "GPS Provider" or "Fault Code Source" or "Fault Code Standard" or "Data Sync Status" => "telemetry",
            _ => "asset"
        };

    private static string NormalizeKey(string value) =>
        value.Trim().ToLowerInvariant().Replace(' ', '-');

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

    private sealed record ReferenceDatasetSeed(
        string Key,
        string Name,
        string Category,
        string OwnerService);
}
