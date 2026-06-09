using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;

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

        Assert.True(datasets.Count > 20);
        Assert.Contains(datasets, dataset =>
            dataset.Key == "maintainarr-asset-class"
            && dataset.Name == "Asset Class"
            && dataset.OwnerService == "MaintainArr"
            && dataset.Status == ReferenceDatasetStatuses.Ready);

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
}
