using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class PlatformSeederTests
{
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
