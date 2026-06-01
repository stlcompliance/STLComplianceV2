using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetBulkImportService(
    MaintainArrDbContext db,
    AssetService assetService,
    FieldsetService fieldsetService,
    ControlledValueValidationService controlledValueValidationService,
    IMaintainArrAuditService audit)
{
    public const int MaxBatchSize = 100;

    private static readonly Dictionary<string, string> GlobalAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["freightliner"] = "freightliner",
        ["frtlnr"] = "freightliner",
        ["freightliner cascadia"] = "cascadia",
        ["disc"] = "disc",
        ["disc brake"] = "disc",
        ["air disc"] = "disc",
        ["drum"] = "drum",
        ["s-cam"] = "drum",
        ["air drum"] = "drum",
        ["super singles"] = "super_single",
        ["wide base"] = "super_single",
        ["wide-base singles"] = "super_single",
        ["duals"] = "duals",
        ["dual tires"] = "duals",
        ["dual wheels"] = "duals",
        ["cng"] = "cng",
        ["compressed natural gas"] = "cng",
        ["reefer"] = "reefer",
        ["refrigerated trailer"] = "reefer",
        ["fmcsa"] = "fmcsa",
        ["federal motor carrier safety administration"] = "fmcsa",
        ["osha"] = "osha",
        ["occupational safety and health administration"] = "osha",
        ["msha"] = "msha",
        ["mine safety and health administration"] = "msha",
        ["epa"] = "epa",
        ["environmental protection agency"] = "epa",
    };

    private static readonly HashSet<string> MultiValueFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "governingBodyKey",
        "rulepackApplicabilityKeys",
        "regulatoryAssetType",
        "complianceCategory",
        "requiredEvidenceType",
        "documentRequirementType",
        "inspectionRequirementType",
        "compatiblePartIds",
        "secondaryMeterTypes",
    };

    public async Task<AssetBulkImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
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

        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", cancellationToken);
        var batchId = Guid.NewGuid();
        var results = new List<AssetImportRowResult>();
        var batchTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var successCount = 0;
        var errorCount = 0;

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
                var values = BuildNormalizedValues(row);
                await EnsureTagAvailableAsync(tenantId, normalizedTag, cancellationToken);

                await controlledValueValidationService.ValidateFieldsetValuesAsync(
                    tenantId,
                    fieldset.Fields,
                    values,
                    actorPersonId,
                    "asset_import",
                    $"row_{index + 1}",
                    createPendingValues: !dryRun,
                    cancellationToken);

                if (dryRun)
                {
                    successCount++;
                    results.Add(new AssetImportRowResult(index, normalizedTag, "validated", null, null, null));
                    continue;
                }

                var created = await assetService.CreateV1Async(
                    tenantId,
                    actorUserId,
                    actorPersonId,
                    new AssetUpsertV1Request(
                        normalizedTag,
                        row.Name.Trim(),
                        string.IsNullOrWhiteSpace(row.Description) ? null : row.Description.Trim(),
                        values),
                    cancellationToken);

                successCount++;
                results.Add(new AssetImportRowResult(index, normalizedTag, "created", created.AssetId, null, null));
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
        if (string.IsNullOrWhiteSpace(row.AssetTag) || string.IsNullOrWhiteSpace(row.Name))
        {
            throw new StlApiException(
                "imports.validation",
                "assetTag and name are required.",
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

    private static IReadOnlyDictionary<string, object?> BuildNormalizedValues(AssetImportRowRequest row)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in row.Values)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
            {
                continue;
            }

            var fieldKey = pair.Key.Trim();
            var normalized = NormalizeValue(fieldKey, pair.Value!.Trim());

            if (MultiValueFields.Contains(fieldKey))
            {
                values[fieldKey] = SplitMultiValue(normalized)
                    .Select(value => NormalizeValue(fieldKey, value))
                    .ToList();
                continue;
            }

            values[fieldKey] = normalized;
        }

        if (!values.ContainsKey("assetClass") && !string.IsNullOrWhiteSpace(row.AssetClassKey))
        {
            values["assetClass"] = NormalizeValue("assetClass", row.AssetClassKey.Trim());
        }
        if (!values.ContainsKey("assetType") && !string.IsNullOrWhiteSpace(row.AssetTypeKey))
        {
            values["assetType"] = NormalizeValue("assetType", row.AssetTypeKey.Trim());
        }
        if (!values.ContainsKey("siteId") && !string.IsNullOrWhiteSpace(row.SiteRef))
        {
            values["siteId"] = row.SiteRef.Trim();
        }
        if (!values.ContainsKey("lifecycleStatus") && !string.IsNullOrWhiteSpace(row.LifecycleStatus))
        {
            values["lifecycleStatus"] = NormalizeValue("lifecycleStatus", row.LifecycleStatus.Trim());
        }
        if (!values.ContainsKey("lifecycleStatus"))
        {
            values["lifecycleStatus"] = "in_service";
        }
        if (!values.ContainsKey("assetStatus"))
        {
            values["assetStatus"] = "active";
        }
        if (!values.ContainsKey("criticality"))
        {
            values["criticality"] = "medium";
        }

        return values;
    }

    private static IReadOnlyList<string> SplitMultiValue(string value)
    {
        return value
            .Split([',', ';', '|'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }

    private static string NormalizeValue(string fieldKey, string value)
    {
        var lowered = value.Trim().ToLowerInvariant();
        if (GlobalAliases.TryGetValue(lowered, out var mapped))
        {
            return mapped;
        }

        if (string.Equals(fieldKey, "fuelType", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(lowered, "cng", StringComparison.OrdinalIgnoreCase))
            {
                return "cng";
            }
        }

        if (string.Equals(fieldKey, "trailerType", StringComparison.OrdinalIgnoreCase) && lowered == "reefer_trailer")
        {
            return "reefer";
        }

        if (string.Equals(fieldKey, "lifecycleStatus", StringComparison.OrdinalIgnoreCase))
        {
            if (lowered == "active")
            {
                return "in_service";
            }
            if (lowered == "inactive")
            {
                return "temporarily_inactive";
            }
        }

        return lowered
            .Replace('&', 'a')
            .Replace('/', '_')
            .Replace('-', '_')
            .Replace(' ', '_');
    }

    private static string NormalizeAssetTag(string value)
    {
        var tag = value.Trim().ToUpperInvariant();
        if (tag.Length == 0 || tag.Length > 64)
        {
            throw new StlApiException("imports.validation", "assetTag must be between 1 and 64 characters.", 400);
        }

        return tag;
    }
}
