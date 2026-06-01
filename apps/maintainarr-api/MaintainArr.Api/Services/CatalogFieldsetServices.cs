using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public interface IExternalReferenceAdapter
{
    string SourceType { get; }
    string SourceOfTruth { get; }
    Task<IReadOnlyList<ReferenceOptionResponse>> GetOptionsAsync(Guid tenantId, string referenceKey, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid tenantId, string referenceKey, string key, CancellationToken cancellationToken);
}

public sealed class ComplianceCoreReferenceAdapter(MaintainArrDbContext db) : IExternalReferenceAdapter
{
    public string SourceType => "compliancecore_reference";
    public string SourceOfTruth => "Compliance Core";

    public Task<IReadOnlyList<ReferenceOptionResponse>> GetOptionsAsync(Guid tenantId, string referenceKey, CancellationToken cancellationToken) =>
        LoadFromCacheAsync(tenantId, referenceKey, cancellationToken);

    public Task<bool> ExistsAsync(Guid tenantId, string referenceKey, string key, CancellationToken cancellationToken) =>
        db.ReferenceCacheEntries.AnyAsync(
            x => x.TenantId == tenantId
                 && x.SourceOfTruth == SourceOfTruth
                 && x.ReferenceKey == referenceKey
                 && x.ExternalKey == key
                 && x.IsActive,
            cancellationToken);

    private async Task<IReadOnlyList<ReferenceOptionResponse>> LoadFromCacheAsync(Guid tenantId, string referenceKey, CancellationToken cancellationToken)
    {
        return await db.ReferenceCacheEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SourceOfTruth == SourceOfTruth && x.ReferenceKey == referenceKey && x.IsActive)
            .OrderBy(x => x.Label)
            .Select(x => new ReferenceOptionResponse(
                x.ExternalKey,
                x.ExternalId,
                x.Label,
                SourceType,
                SourceOfTruth,
                "stable_key",
                "mirrored_label",
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}

public sealed class StaffArrReferenceAdapter(MaintainArrDbContext db) : IExternalReferenceAdapter
{
    public string SourceType => "staffarr_reference";
    public string SourceOfTruth => "StaffArr";

    public Task<IReadOnlyList<ReferenceOptionResponse>> GetOptionsAsync(Guid tenantId, string referenceKey, CancellationToken cancellationToken) =>
        LoadFromCacheAsync(tenantId, referenceKey, cancellationToken);

    public Task<bool> ExistsAsync(Guid tenantId, string referenceKey, string key, CancellationToken cancellationToken) =>
        db.ReferenceCacheEntries.AnyAsync(
            x => x.TenantId == tenantId
                 && x.SourceOfTruth == SourceOfTruth
                 && x.ReferenceKey == referenceKey
                 && (x.ExternalId == key || x.ExternalKey == key)
                 && x.IsActive,
            cancellationToken);

    private async Task<IReadOnlyList<ReferenceOptionResponse>> LoadFromCacheAsync(Guid tenantId, string referenceKey, CancellationToken cancellationToken)
    {
        return await db.ReferenceCacheEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SourceOfTruth == SourceOfTruth && x.ReferenceKey == referenceKey && x.IsActive)
            .OrderBy(x => x.Label)
            .Select(x => new ReferenceOptionResponse(
                x.ExternalKey,
                x.ExternalId,
                x.Label,
                SourceType,
                SourceOfTruth,
                "id",
                "mirroredDisplayName",
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}

public sealed class SupplyArrReferenceAdapter(MaintainArrDbContext db) : IExternalReferenceAdapter
{
    public string SourceType => "supplyarr_reference";
    public string SourceOfTruth => "SupplyArr";

    public async Task<IReadOnlyList<ReferenceOptionResponse>> GetOptionsAsync(Guid tenantId, string referenceKey, CancellationToken cancellationToken)
    {
        return await db.ReferenceCacheEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SourceOfTruth == SourceOfTruth && x.ReferenceKey == referenceKey && x.IsActive)
            .OrderBy(x => x.Label)
            .Select(x => new ReferenceOptionResponse(
                x.ExternalKey,
                x.ExternalId,
                x.Label,
                SourceType,
                SourceOfTruth,
                "id",
                "mirroredDisplayName",
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid tenantId, string referenceKey, string key, CancellationToken cancellationToken) =>
        db.ReferenceCacheEntries.AnyAsync(
            x => x.TenantId == tenantId
                 && x.SourceOfTruth == SourceOfTruth
                 && x.ReferenceKey == referenceKey
                 && (x.ExternalId == key || x.ExternalKey == key)
                 && x.IsActive,
            cancellationToken);
}

public sealed class CatalogService(
    MaintainArrDbContext db,
    CatalogSeedService catalogSeedService)
{
    public async Task<IReadOnlyList<CatalogResponse>> ListAsync(Guid tenantId, string[]? keys, CancellationToken cancellationToken)
    {
        await catalogSeedService.EnsureSeededForTenantAsync(tenantId, cancellationToken);

        var query = db.CatalogDefinitions.AsNoTracking().Where(x => x.TenantId == tenantId && x.IsActive);
        if (keys is { Length: > 0 })
        {
            query = query.Where(x => keys.Contains(x.Key));
        }

        var catalogs = await query.OrderBy(x => x.Label).ToListAsync(cancellationToken);
        if (catalogs.Count == 0)
        {
            return [];
        }

        var catalogIds = catalogs.Select(x => x.Id).ToList();
        var options = await db.CatalogOptions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && catalogIds.Contains(x.CatalogId) && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Label)
            .ToListAsync(cancellationToken);

        var deps = await db.CatalogOptionDependencies.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return catalogs.Select(c => new CatalogResponse(
                c.Key,
                c.Label,
                c.Description,
                c.Owner,
                c.Scope,
                c.IsSystem,
                c.IsTenantExtendable,
                c.IsActive,
                options.Where(o => o.CatalogId == c.Id)
                    .Select(o => new CatalogOptionResponse(
                        o.Key,
                        o.Label,
                        o.Description,
                        o.SortOrder,
                        options.FirstOrDefault(x => x.Id == o.ParentOptionId)?.Key,
                        o.IsActive,
                        deps.Where(d => d.CatalogOptionId == o.Id).ToDictionary(d => d.DependsOnCatalogKey, d => d.DependsOnOptionKey),
                        ParseObjectDict(o.MetadataJson)))
                    .ToList()))
            .ToList();
    }

    public async Task<CatalogResponse> GetAsync(Guid tenantId, string key, CancellationToken cancellationToken)
    {
        await catalogSeedService.EnsureSeededForTenantAsync(tenantId, cancellationToken);
        var catalog = (await ListAsync(tenantId, [key], cancellationToken)).FirstOrDefault();
        return catalog ?? throw new StlApiException("catalog.not_found", $"Catalog '{key}' was not found.", 404);
    }

    public async Task<CatalogOptionResponse> UpsertOptionAsync(Guid tenantId, string catalogKey, string optionKey, UpsertCatalogOptionRequest request, CancellationToken cancellationToken)
    {
        var catalog = await db.CatalogDefinitions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == catalogKey, cancellationToken)
            ?? throw new StlApiException("catalog.not_found", $"Catalog '{catalogKey}' was not found.", 404);

        var option = await db.CatalogOptions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.CatalogId == catalog.Id && x.Key == optionKey, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (option is null)
        {
            option = new CatalogOption
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CatalogId = catalog.Id,
                Key = request.Key.Trim(),
                Label = request.Label.Trim(),
                Description = request.Description.Trim(),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                MetadataJson = request.Metadata is null ? "{}" : JsonSerializer.Serialize(request.Metadata),
                IsSystem = false,
                IsTenantSpecific = true,
                OptionTenantId = tenantId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.CatalogOptions.Add(option);
        }
        else
        {
            option.Key = request.Key.Trim();
            option.Label = request.Label.Trim();
            option.Description = request.Description.Trim();
            option.SortOrder = request.SortOrder;
            option.IsActive = request.IsActive;
            option.MetadataJson = request.Metadata is null ? "{}" : JsonSerializer.Serialize(request.Metadata);
            option.UpdatedAt = now;
        }

        var existingDeps = await db.CatalogOptionDependencies
            .Where(x => x.TenantId == tenantId && x.CatalogOptionId == option.Id)
            .ToListAsync(cancellationToken);
        if (existingDeps.Count > 0)
        {
            db.CatalogOptionDependencies.RemoveRange(existingDeps);
        }

        if (request.Dependency is not null)
        {
            foreach (var pair in request.Dependency)
            {
                db.CatalogOptionDependencies.Add(new CatalogOptionDependency
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CatalogOptionId = option.Id,
                    DependsOnCatalogKey = pair.Key,
                    DependsOnOptionKey = pair.Value,
                    RuleJson = "{}",
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new CatalogOptionResponse(
            option.Key,
            option.Label,
            option.Description,
            option.SortOrder,
            null,
            option.IsActive,
            request.Dependency,
            ParseObjectDict(option.MetadataJson));
    }

    private static IReadOnlyDictionary<string, object?>? ParseObjectDict(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            return null;
        }

        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
    }
}
