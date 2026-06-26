using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using System.Reflection;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class PlatformSeederTests
{
    [Fact]
    public async Task SeedInfrastructureAsync_seeds_maintainarr_reference_datasets_idempotently()
    {
        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseInMemoryDatabase($"platform-seeder-reference-data-tests-{Guid.NewGuid():N}")
            .Options;

        await using var db = new NexArrDbContext(options);

        await PlatformSeeder.SeedInfrastructureAsync(db);

        var datasets = await db.ReferenceDatasets
            .OrderBy(x => x.Key)
            .Select(x => new { x.Key, x.Name, x.OwnerService, x.Status })
            .ToListAsync();

        Assert.True(datasets.Count > 250);

        var expectedDatasets = new[]
        {
            ("maintainarr-asset-class", "Asset Class", "MaintainArr"),
            ("compliancecore-governing-bodies", "Governing Bodies", "Compliance Core"),
            ("staffarr-person-status", "Person Status", "StaffArr"),
            ("trainarr-program-type", "Program Type", "TrainArr"),
            ("routarr-dispatch-status", "Dispatch Status", "RoutArr"),
            ("loadarr-receipt-status", "Receipt Status", "LoadArr"),
            ("assurarr-nonconformance-type", "Nonconformance Type", "AssurArr"),
            ("recordarr-document-type", "Document Type", "RecordArr"),
            ("reportarr-dataset-type", "Dataset Type", "ReportArr"),
            ("fieldcompanion-task-type", "Task Type", "Field Companion"),
            ("supplyarr-party", "Party", "SupplyArr"),
        };

        foreach (var (key, name, ownerService) in expectedDatasets)
        {
            Assert.Contains(datasets, dataset =>
                dataset.Key == key
                && dataset.Name == name
                && dataset.OwnerService == ownerService
                && dataset.Status == ReferenceDatasetStatuses.Ready);
        }

        var countAfterFirstSeed = datasets.Count;
        await PlatformSeeder.SeedInfrastructureAsync(db);

        Assert.Equal(countAfterFirstSeed, await db.ReferenceDatasets.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_does_not_recreate_legacy_demo_product_access_records_for_existing_installs()
    {
        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseInMemoryDatabase($"platform-seeder-tests-{Guid.NewGuid():N}")
            .Options;

        await using var db = new NexArrDbContext(options);
        var hasher = new BcryptPasswordHasher();

        await PlatformSeeder.SeedAsync(db, hasher);

        await PlatformSeeder.SeedAsync(db, hasher);

        var productKeys = await db.ProductCatalog
            .Select(product => product.ProductKey)
            .ToListAsync();
        Assert.Contains("customarr", productKeys);
        Assert.Contains("ordarr", productKeys);

        Assert.Empty(await db.Entitlements
            .Where(entitlement => entitlement.TenantId == PlatformSeeder.DemoTenantId)
            .ToListAsync());
        Assert.Empty(await db.TenantProductLicenses
            .Where(license => license.TenantId == PlatformSeeder.DemoTenantId)
            .ToListAsync());

        var access = new FixedSuiteProductAccessService(db);
        var accessibleToTenantUsers = await access.ListAccessibleProductKeysAsync(isPlatformAdmin: false);
        var accessibleToPlatformAdmins = await access.ListAccessibleProductKeysAsync(isPlatformAdmin: true);

        Assert.DoesNotContain("compliancecore", accessibleToTenantUsers, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("compliancecore", accessibleToPlatformAdmins, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("shared-worker", accessibleToTenantUsers, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("shared-worker", accessibleToPlatformAdmins, StringComparer.OrdinalIgnoreCase);

        var expectedOrdinaryProducts = await db.ProductCatalog
            .Where(product => product.IsActive && product.ProductStatus != "worker" && product.ProductKey != "compliancecore")
            .OrderBy(product => product.SortOrder)
            .Select(product => product.ProductKey)
            .ToListAsync();
        Assert.Equal(expectedOrdinaryProducts, accessibleToTenantUsers);
    }

    [Fact]
    public async Task EnsureProvisionedAsync_seeds_customarr_product_catalog_and_service_clients()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new NexArrDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SERVICE_TOKEN_SIGNING_KEY"] = "test-integration-token-signing-key-1234567890",
                ["SERVICE_TOKEN_ISSUER"] = "stl-test-issuer",
                ["SERVICE_TOKEN_AUDIENCE"] = "stl-test-audience",
            })
            .Build();

        var service = new IntegrationTokenBootstrapService(
            db,
            configuration,
            Options.Create(new StlServiceTokenOptions()),
            NullLogger<IntegrationTokenBootstrapService>.Instance);

        await service.EnsureProvisionedAsync();

        var customarrProduct = await db.ProductCatalog
            .AsNoTracking()
            .SingleAsync(product => product.ProductKey == "customarr");
        Assert.Equal("CustomArr", customarrProduct.DisplayName);

        var customarrClient = await db.ServiceClients
            .AsNoTracking()
            .SingleAsync(client => client.SourceProductKey == "customarr");
        Assert.StartsWith("bootstrap-handoff-customarr", customarrClient.ClientKey, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EnsureProvisionedAsync_backfills_missing_product_catalog_entries_when_catalog_already_has_rows()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new NexArrDbContext(options);
        await db.Database.EnsureCreatedAsync();

        db.ProductCatalog.Add(new ProductCatalogItem
        {
            ProductKey = "customarr",
            DisplayName = "CustomArr",
            ProductCategory = "customer-relationship",
            ProductOwner = "Customer Operations",
            ProductStatus = "available",
            SortOrder = 55,
            IsActive = true,
            CanonicalCallbackPath = "/auth/nexarr/callback",
            ApiBaseUrl = "http://localhost:5111",
            HealthUrl = "http://localhost:5111/health/ready",
            ServiceAudience = "stl:customarr:api",
            MarketingUrl = "https://stlcompliance.com/products/customarr",
            DocumentationUrl = "https://stlcompliance.com/docs/customarr",
            SupportUrl = "https://stlcompliance.com/support",
            EnvironmentKey = "local",
            AvailabilityDependencyRules = "tenant-product-availability-required",
            ProductDependencyMetadata = "{\"dependsOn\":[]}"
        });
        await db.SaveChangesAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SERVICE_TOKEN_SIGNING_KEY"] = "test-integration-token-signing-key-1234567890",
                ["SERVICE_TOKEN_ISSUER"] = "stl-test-issuer",
                ["SERVICE_TOKEN_AUDIENCE"] = "stl-test-audience",
            })
            .Build();

        var service = new IntegrationTokenBootstrapService(
            db,
            configuration,
            Options.Create(new StlServiceTokenOptions()),
            NullLogger<IntegrationTokenBootstrapService>.Instance);

        await service.EnsureProvisionedAsync();

        var productKeys = await db.ProductCatalog
            .AsNoTracking()
            .Select(product => product.ProductKey)
            .ToListAsync();

        foreach (var profileSourceProductKey in StlIntegrationTokenCatalog.All
            .Select(profile => profile.SourceProductKey)
            .Distinct(StringComparer.Ordinal))
        {
            Assert.Contains(profileSourceProductKey, productKeys);
        }

        Assert.Contains("staffarr", productKeys);
        Assert.Contains("maintainarr", productKeys);
        Assert.Contains("ordarr", productKeys);
        Assert.Contains("ledgarr", productKeys);
        Assert.Contains("shared-worker", productKeys);
    }

    [Fact]
    public async Task Staffarr_api_nexarr_service_token_includes_person_create_and_login_scopes()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new NexArrDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SERVICE_TOKEN_SIGNING_KEY"] = "test-integration-token-signing-key-1234567890",
                ["SERVICE_TOKEN_ISSUER"] = "stl-test-issuer",
                ["SERVICE_TOKEN_AUDIENCE"] = "stl-test-audience",
            })
            .Build();

        var service = new IntegrationTokenBootstrapService(
            db,
            configuration,
            Options.Create(new StlServiceTokenOptions()),
            NullLogger<IntegrationTokenBootstrapService>.Instance);

        var tokens = await service.GetTokensForConsumerAsync("staffarr-api");

        Assert.True(tokens.TryGetValue("NexArr__ServiceToken", out var token));

        var validator = new StlServiceTokenValidator(
            configuration,
            Options.Create(new StlServiceTokenOptions()));
        var validated = validator.ValidateOrThrow(
            token,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "staffarr",
                RequiredTargetProduct = "nexarr",
                TenantId = Guid.Empty,
                RequiredActionScope = "nexarr.identities.create",
            });

        Assert.Equal("staffarr", validated.SourceProductKey);
        Assert.Contains("nexarr", validated.AllowedProductKeys);
        Assert.Contains("nexarr.identities.create", validated.ActionScope);
        Assert.Contains("nexarr.users.login_disable", validated.ActionScope);
        Assert.Contains("nexarr.users.login_enable", validated.ActionScope);
    }

    [Fact]
    public async Task SeedFirstAdminAsync_bootstraps_tenant_without_recreating_legacy_product_access_records()
    {
        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseInMemoryDatabase($"platform-seeder-bootstrap-tests-{Guid.NewGuid():N}")
            .Options;

        await using var db = new NexArrDbContext(options);
        var hasher = new BcryptPasswordHasher();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:FirstAdminEmail"] = "root@demo.stl",
                ["Seed:FirstAdminPassword"] = "ChangeMe!Bootstrap2026"
            })
            .Build();

        await PlatformSeeder.SeedInfrastructureAsync(db);

        var adminId = await PlatformSeeder.SeedFirstAdminAsync(
            db,
            hasher,
            configuration,
            new TestWebHostEnvironment("Production"));

        Assert.NotNull(adminId);

        var tenant = Assert.Single(await db.Tenants.ToListAsync());
        var membership = Assert.Single(await db.TenantMemberships.ToListAsync());
        Assert.Equal(tenant.Id, membership.TenantId);
        Assert.Equal(adminId, membership.UserId);
        Assert.Equal("platform_admin", membership.RoleKey);

        Assert.Empty(await db.Entitlements
            .Where(entitlement => entitlement.TenantId == tenant.Id)
            .ToListAsync());
        Assert.Empty(await db.TenantProductLicenses
            .Where(license => license.TenantId == tenant.Id)
            .ToListAsync());

        var access = new FixedSuiteProductAccessService(db);
        var accessibleToTenantUsers = await access.ListAccessibleProductKeysAsync(isPlatformAdmin: false);
        var accessibleToPlatformAdmins = await access.ListAccessibleProductKeysAsync(isPlatformAdmin: true);

        Assert.DoesNotContain("compliancecore", accessibleToTenantUsers, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("compliancecore", accessibleToPlatformAdmins, StringComparer.OrdinalIgnoreCase);

        var expectedTenantProducts = await db.ProductCatalog
            .Where(product => product.IsActive && product.ProductStatus != "worker" && product.ProductKey != "compliancecore")
            .OrderBy(product => product.SortOrder)
            .Select(product => product.ProductKey)
            .ToListAsync();
        var expectedPlatformAdminProducts = await db.ProductCatalog
            .Where(product => product.IsActive && product.ProductStatus != "worker")
            .OrderBy(product => product.SortOrder)
            .Select(product => product.ProductKey)
            .ToListAsync();

        Assert.Equal(expectedTenantProducts, accessibleToTenantUsers);
        Assert.Equal(expectedPlatformAdminProducts, accessibleToPlatformAdmins);
    }

    [Fact]
    public async Task SeedMasterReferenceDataUpsert_uses_source_keys_for_crosswalks()
    {
        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseInMemoryDatabase($"platform-seeder-crosswalk-tests-{Guid.NewGuid():N}")
            .Options;

        await using var db = new NexArrDbContext(options);
        var sourceId = Guid.NewGuid();
        var firstEntity = new ReferenceEntity
        {
            Id = Guid.NewGuid(),
            DatasetId = Guid.NewGuid(),
            EntityType = "audit_type",
            CanonicalKey = "legacy_internal_process_audit",
            DisplayName = "Legacy Internal Process Audit",
            Status = ReferenceEntityStatuses.Active,
            NormalizedFieldsJson = "{}",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        var targetDataset = new ReferenceDataset
        {
            Id = Guid.NewGuid(),
            Key = "assurarr-audit-type",
            Name = "Audit Type",
            Category = "assurance",
            OwnerService = "AssurArr",
            Status = ReferenceDatasetStatuses.Ready,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.ReferenceDatasets.Add(targetDataset);
        db.ReferenceEntities.Add(firstEntity);
        db.ReferenceCrosswalks.Add(new ReferenceCrosswalk
        {
            Id = Guid.NewGuid(),
            ReferenceEntityId = firstEntity.Id,
            ExternalSystem = "master-reference-csv",
            ExternalKey = "assurarr-audit-type:internal_process_audit",
            SourceId = sourceId,
            Confidence = 0.90m,
            Status = ReferenceCrosswalkStatuses.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var staged = new StagingRecord
        {
            Id = Guid.NewGuid(),
            TargetDatasetId = targetDataset.Id,
            ProposedEntityType = "audit_type",
            ProposedCanonicalKey = "internal_process_audit",
            Confidence = 0.90m,
            RawPayloadJson = "{}",
            NormalizedPayloadJson = """
                {
                  "sourceKey": "assurarr-audit-type:internal_process_audit",
                  "displayName": "Internal Process Audit"
                }
                """,
            Status = ReferenceStagingStatuses.NeedsReview,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var upsertMethod = typeof(PlatformSeeder).GetMethod(
            "UpsertReferenceEntityFromSeedAsync",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(upsertMethod);

        var task = (Task<ReferenceEntity>)upsertMethod!.Invoke(
            null,
            [db, staged, targetDataset, "master-reference-csv", sourceId, DateTimeOffset.UtcNow, CancellationToken.None])!;

        var entity = await task;

        var crosswalks = await db.ReferenceCrosswalks
            .Where(x => x.ExternalSystem == "master-reference-csv")
            .ToListAsync();

        Assert.Single(crosswalks);
        Assert.Equal("assurarr-audit-type:internal_process_audit", crosswalks[0].ExternalKey);
        Assert.Equal(entity.Id, crosswalks[0].ReferenceEntityId);
    }

    private sealed class TestWebHostEnvironment(string environmentName) : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "STLCompliance.NexArr.Auth.Tests";

        public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
