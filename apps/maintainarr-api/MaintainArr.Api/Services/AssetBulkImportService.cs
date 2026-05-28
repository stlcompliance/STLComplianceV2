using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetBulkImportService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public const int MaxBatchSize = 100;

    private static readonly HashSet<string> AllowedLifecycleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive",
        "retired",
        "out_of_service",
    };

    public async Task<AssetBulkImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        IReadOnlyList<AssetImportRowRequest> assets,
        bool dryRun,
        string phase,
        CancellationToken cancellationToken = default)
    {
        if (assets is null || assets.Count == 0)
        {
            throw new StlApiException("imports.validation", "At least one asset row is required.", 400);
        }

        if (assets.Count > MaxBatchSize)
        {
            throw new StlApiException(
                "imports.validation",
                $"Bulk import supports at most {MaxBatchSize} rows per request.",
                400);
        }

        var batchId = Guid.NewGuid();
        var results = new List<AssetImportRowResult>();
        var batchTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var successCount = 0;
        var errorCount = 0;
        var typeCache = await BuildAssetTypeCacheAsync(tenantId, cancellationToken);

        for (var index = 0; index < assets.Count; index++)
        {
            var row = assets[index];
            var normalizedTag = NormalizeAssetTag(row.AssetTag);

            if (!batchTags.Add(normalizedTag))
            {
                errorCount++;
                results.Add(new AssetImportRowResult(
                    index,
                    normalizedTag,
                    "error",
                    null,
                    "assets.duplicate_tag",
                    "Duplicate asset tag within the import batch."));
                continue;
            }

            try
            {
                ValidateRow(row);
                var assetType = ResolveAssetType(typeCache, row.AssetClassKey, row.AssetTypeKey);
                await EnsureTagAvailableAsync(tenantId, normalizedTag, cancellationToken);

                if (dryRun)
                {
                    successCount++;
                    results.Add(new AssetImportRowResult(
                        index,
                        normalizedTag,
                        "validated",
                        null,
                        null,
                        null));
                    continue;
                }

                var now = DateTimeOffset.UtcNow;
                var entity = new Asset
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetTypeId = assetType.Id,
                    AssetTag = normalizedTag,
                    Name = NormalizeName(row.Name),
                    Description = NormalizeDescription(row.Description),
                    LifecycleStatus = row.LifecycleStatus.Trim().ToLowerInvariant(),
                    SiteRef = NormalizeSiteRef(row.SiteRef),
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                db.Assets.Add(entity);
                await db.SaveChangesAsync(cancellationToken);

                successCount++;
                results.Add(new AssetImportRowResult(
                    index,
                    normalizedTag,
                    "created",
                    entity.Id,
                    null,
                    null));

                await audit.WriteAsync(
                    "asset.create",
                    tenantId,
                    actorUserId,
                    "asset",
                    entity.Id.ToString(),
                    "success",
                    reasonCode: "bulk_import",
                    cancellationToken: cancellationToken);
            }
            catch (StlApiException ex)
            {
                errorCount++;
                results.Add(new AssetImportRowResult(
                    index,
                    normalizedTag,
                    "error",
                    null,
                    ex.Code,
                    ex.Message));
            }
        }

        var batchStatus = errorCount == 0
            ? MaintainArrImportBatchStatuses.Completed
            : successCount == 0
                ? MaintainArrImportBatchStatuses.Failed
                : MaintainArrImportBatchStatuses.Partial;

        var batch = new MaintainArrImportBatch
        {
            Id = batchId,
            TenantId = tenantId,
            ImportType = MaintainArrImportTypes.Assets,
            Phase = phase,
            DryRun = dryRun,
            Status = batchStatus,
            TotalRows = assets.Count,
            SuccessCount = successCount,
            ErrorCount = errorCount,
            CreatedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
        };

        db.MaintainArrImportBatches.Add(batch);
        await db.SaveChangesAsync(cancellationToken);

        var auditAction = dryRun
            ? "maintainarr.imports.assets.validate"
            : "maintainarr.imports.assets.commit";

        await audit.WriteAsync(
            auditAction,
            tenantId,
            actorUserId,
            "import_batch",
            batchId.ToString(),
            batchStatus,
            reasonCode: $"{successCount}/{assets.Count}",
            cancellationToken: cancellationToken);

        return new AssetBulkImportResponse(
            batchId,
            MaintainArrImportTypes.Assets,
            phase,
            dryRun,
            assets.Count,
            successCount,
            errorCount,
            results);
    }

    private static void ValidateRow(AssetImportRowRequest row)
    {
        if (string.IsNullOrWhiteSpace(row.AssetClassKey)
            || string.IsNullOrWhiteSpace(row.AssetTypeKey)
            || string.IsNullOrWhiteSpace(row.AssetTag)
            || string.IsNullOrWhiteSpace(row.Name))
        {
            throw new StlApiException(
                "imports.validation",
                "assetClassKey, assetTypeKey, assetTag, and name are required.",
                400);
        }

        if (!AllowedLifecycleStatuses.Contains(row.LifecycleStatus.Trim()))
        {
            throw new StlApiException(
                "imports.validation",
                "lifecycleStatus must be active, inactive, retired, or out_of_service.",
                400);
        }
    }

    private async Task EnsureTagAvailableAsync(
        Guid tenantId,
        string assetTag,
        CancellationToken cancellationToken)
    {
        var exists = await db.Assets.AnyAsync(
            x => x.TenantId == tenantId && x.AssetTag == assetTag,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "assets.duplicate_tag",
                "An asset with this tag already exists.",
                409);
        }
    }

    private static AssetType ResolveAssetType(
        IReadOnlyDictionary<(string ClassKey, string TypeKey), AssetType> cache,
        string classKey,
        string typeKey)
    {
        var normalizedClass = classKey.Trim().ToLowerInvariant();
        var normalizedType = typeKey.Trim().ToLowerInvariant();
        if (!cache.TryGetValue((normalizedClass, normalizedType), out var assetType))
        {
            throw new StlApiException(
                "imports.asset_type_not_found",
                $"Asset type '{typeKey}' in class '{classKey}' was not found or is inactive.",
                400);
        }

        return assetType;
    }

    private async Task<Dictionary<(string ClassKey, string TypeKey), AssetType>> BuildAssetTypeCacheAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var types = await db.AssetTypes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "active")
            .Join(
                db.AssetClasses.AsNoTracking().Where(c => c.TenantId == tenantId && c.Status == "active"),
                type => type.AssetClassId,
                assetClass => assetClass.Id,
                (type, assetClass) => new { type, assetClass })
            .ToListAsync(cancellationToken);

        return types.ToDictionary(
            x => (x.assetClass.ClassKey.Trim().ToLowerInvariant(), x.type.TypeKey.Trim().ToLowerInvariant()),
            x => x.type);
    }

    private static string NormalizeAssetTag(string value)
    {
        var tag = value.Trim();
        if (tag.Length == 0 || tag.Length > 64)
        {
            throw new StlApiException("imports.validation", "assetTag must be between 1 and 64 characters.", 400);
        }

        return tag;
    }

    private static string NormalizeName(string value)
    {
        var name = value.Trim();
        if (name.Length == 0 || name.Length > 256)
        {
            throw new StlApiException("imports.validation", "name must be between 1 and 256 characters.", 400);
        }

        return name;
    }

    private static string NormalizeDescription(string value) => value?.Trim() ?? string.Empty;

    private static string? NormalizeSiteRef(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var site = value.Trim();
        return site.Length > 128
            ? throw new StlApiException("imports.validation", "siteRef must be 128 characters or fewer.", 400)
            : site;
    }
}
