using STLCompliance.Shared.Integration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using NexArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

internal static class MaintainArrTestSites
{
    internal static readonly Guid DefaultStaffArrSiteOrgUnitId = Guid.Parse("5f0b49a9-7c67-4ce1-a0e9-3e7e226d3992");
    internal const string DefaultStaffArrSiteName = "Central Maintenance Site";

    internal static async Task SeedCachedStaffArrSiteAsync(
        WebApplicationFactory<global::MaintainArr.Api.Program> factory,
        Guid? siteOrgUnitId = null,
        string? siteName = null)
    {
        var orgUnitId = siteOrgUnitId ?? DefaultStaffArrSiteOrgUnitId;
        var label = string.IsNullOrWhiteSpace(siteName) ? DefaultStaffArrSiteName : siteName;

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();

        db.ReferenceCacheEntries.Add(new ReferenceCacheEntry
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            SourceOfTruth = "StaffArr",
            ReferenceKey = "sites",
            ExternalKey = orgUnitId.ToString("D"),
            ExternalId = orgUnitId.ToString("D"),
            Label = label,
            Description = null,
            MetadataJson = "{}",
            IsActive = true,
            LastSyncedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();
    }
}
