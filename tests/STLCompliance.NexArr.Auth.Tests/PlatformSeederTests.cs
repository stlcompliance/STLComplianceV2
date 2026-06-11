using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
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
            ("field-companion-task-type", "Task Type", "Field Companion"),
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
    public async Task SeedAsync_backfills_missing_demo_product_access_for_existing_installs()
    {
        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseInMemoryDatabase($"platform-seeder-tests-{Guid.NewGuid():N}")
            .Options;

        await using var db = new NexArrDbContext(options);
        var hasher = new BcryptPasswordHasher();

        await PlatformSeeder.SeedAsync(db, hasher);

        var assurarrEntitlement = await db.Entitlements.SingleAsync(
            entitlement =>
                entitlement.TenantId == PlatformSeeder.DemoTenantId
                && entitlement.ProductKey == "assurarr");
        var assurarrLicense = await db.TenantProductLicenses.SingleAsync(
            license =>
                license.TenantId == PlatformSeeder.DemoTenantId
                && license.ProductKey == "assurarr");

        db.Entitlements.Remove(assurarrEntitlement);
        db.TenantProductLicenses.Remove(assurarrLicense);
        await db.SaveChangesAsync();

        await PlatformSeeder.SeedAsync(db, hasher);

        Assert.True(await db.Entitlements.AnyAsync(
            entitlement =>
                entitlement.TenantId == PlatformSeeder.DemoTenantId
                && entitlement.ProductKey == "assurarr"));
        Assert.True(await db.TenantProductLicenses.AnyAsync(
            license =>
                license.TenantId == PlatformSeeder.DemoTenantId
                && license.ProductKey == "assurarr"));
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
}
