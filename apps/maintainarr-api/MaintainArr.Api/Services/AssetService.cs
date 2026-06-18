using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using System.Text.Json;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetService(
    MaintainArrDbContext db,
    AssetTypeService assetTypeService,
    IMaintainArrAuditService audit,
    FieldsetService fieldsetService,
    ControlledValueValidationService controlledValueValidationService,
    StaffArrSiteReferenceService staffArrSites,
    MaintainArrTenantSettingsService tenantSettings)
{
    private static readonly HashSet<string> SpecFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "make",
        "manufacturer",
        "model",
        "modelYear",
        "series",
        "trim",
        "configuration",
        "cabType",
        "bodyType",
        "drivetrain",
        "axleConfiguration",
        "tireConfiguration",
        "fuelType",
        "aftertreatmentType",
        "hybridType",
        "brakeType",
        "brakeSystemType",
        "trailerType",
        "meterType",
        "primaryMeterType",
        "meterUnit",
        "usageProfile",
        "telematicsProvider",
        "diagnosticProtocol",
        "faultCodeStandard",
    };

    private static readonly HashSet<string> ComponentFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "engineMake",
        "engineModel",
        "transmissionMake",
        "transmissionModel",
        "tireSize",
        "wheelSize",
        "wheelMaterial",
        "suspensionType",
        "parkingBrakeType",
    };

    private static readonly HashSet<string> ComplianceFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "governingBodyKey",
        "rulepackApplicabilityKeys",
        "regulatoryAssetType",
        "complianceCategory",
        "requiredEvidenceType",
        "documentRequirementType",
        "inspectionRequirementType",
    };

    private static readonly HashSet<string> AllowedLifecycleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "ordered",
        "received",
        "in_service",
        "temporarily_inactive",
        "pending_disposal",
        "disposed",
        "retired",
        "active",
        "inactive",
        "pending_inspection",
        "out_of_service",
    };

    public async Task<AssetResponse> CreateV1Async(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        AssetUpsertV1Request request,
        CancellationToken cancellationToken = default)
    {
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", cancellationToken);
        var values = BuildAssetUpsertValues(request);
        await controlledValueValidationService.ValidateFieldsetValuesAsync(
            tenantId,
            fieldset.Fields,
            values,
            actorPersonId,
            "asset",
            "new",
            createPendingValues: true,
            cancellationToken);

        var assetTypeKey = values.TryGetValue("assetType", out var rawType) ? rawType?.ToString() : null;
        var assetClassKey = values.TryGetValue("assetClass", out var rawClass) ? rawClass?.ToString() : null;
        if (settings.Assets.RequireAssetClassOnCreate && string.IsNullOrWhiteSpace(assetClassKey))
        {
            throw new StlApiException("assets.asset_class_required", "Asset class is required by MaintainArr tenant settings.", 400);
        }

        if (string.IsNullOrWhiteSpace(assetTypeKey))
        {
            throw new StlApiException("assets.validation", "assetType is required.", 400);
        }

        var assetType = await ResolveOrCreateAssetTypeProjectionAsync(
            tenantId,
            assetClassKey,
            assetTypeKey,
            cancellationToken);

        EnsureVinOrSerialIfRequired(settings, values);
        var assetTag = await ResolveAssetTagAsync(
            tenantId,
            ExtractFirst(values, "unitNumber") ?? ExtractFirst(values, "assetNumber") ?? request.AssetTag,
            settings.Assets.AssetNumberingMode,
            settings.Assets.AssetNumberPrefix,
            cancellationToken);
        var name = NormalizeNameOrFallback(ExtractFirst(values, "displayName") ?? request.Name, assetTag);
        var description = NormalizeDescription(ExtractFirst(values, "description") ?? request.Description);

        var exists = await db.Assets.AnyAsync(x => x.TenantId == tenantId && x.AssetTag == assetTag, cancellationToken);
        if (exists)
        {
            throw new StlApiException("assets.duplicate_tag", "An asset with this tag already exists.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var site = await ResolveAssetSiteAsync(
            tenantId,
            null,
            values.TryGetValue("siteId", out var siteIdRaw) ? siteIdRaw?.ToString() : null,
            cancellationToken);
        if (settings.Assets.RequireSiteOnAssetCreate && site is null)
        {
            throw new StlApiException("assets.site_required", "A StaffArr site is required by MaintainArr tenant settings.", 400);
        }

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetTypeId = assetType.Id,
            AssetTag = assetTag,
            Name = name,
            Description = description,
            LifecycleStatus = values.TryGetValue("lifecycleStatus", out var lifecycleRaw)
                && !string.IsNullOrWhiteSpace(lifecycleRaw?.ToString())
                ? NormalizeLifecycleStatus(lifecycleRaw!.ToString()!)
                : NormalizeLifecycleStatus(settings.Assets.DefaultAssetStatus),
            SiteRef = site?.OrgUnitId.ToString("D"),
            StaffarrSiteOrgUnitId = site?.OrgUnitId,
            StaffarrSiteNameSnapshot = site?.Name ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Assets.Add(asset);
        await db.SaveChangesAsync(cancellationToken);

        await UpsertControlledFieldStoresAsync(tenantId, asset.Id, values, now, cancellationToken);

        await audit.WriteAsync(
            "asset.create",
            tenantId,
            actorUserId,
            "asset",
            asset.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(asset, assetType, assetType.AssetClass);
    }

    public async Task<AssetResponse> UpdateV1Async(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid assetId,
        AssetUpsertV1Request request,
        CancellationToken cancellationToken = default)
    {
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "edit", cancellationToken);
        var values = BuildAssetUpsertValues(request);
        await controlledValueValidationService.ValidateFieldsetValuesAsync(
            tenantId,
            fieldset.Fields,
            values,
            actorPersonId,
            "asset",
            assetId.ToString(),
            createPendingValues: true,
            cancellationToken);

        var asset = await db.Assets
            .Include(x => x.AssetType)
            .ThenInclude(x => x.AssetClass)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken)
            ?? throw new StlApiException("assets.not_found", "Asset was not found.", 404);

        var assetTypeKey = values.TryGetValue("assetType", out var rawType) ? rawType?.ToString() : null;
        var assetClassKey = values.TryGetValue("assetClass", out var rawClass) ? rawClass?.ToString() : null;
        if (!string.IsNullOrWhiteSpace(assetTypeKey))
        {
            var projectedType = await ResolveOrCreateAssetTypeProjectionAsync(
                tenantId,
                assetClassKey,
                assetTypeKey,
                cancellationToken);
            asset.AssetTypeId = projectedType.Id;
        }

        var nextAssetTag = NormalizeAssetTag(ExtractFirst(values, "unitNumber") ?? ExtractFirst(values, "assetNumber") ?? request.AssetTag);
        if (!string.Equals(asset.AssetTag, nextAssetTag, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await db.Assets.AnyAsync(
                x => x.TenantId == tenantId && x.Id != assetId && x.AssetTag == nextAssetTag,
                cancellationToken);
            if (duplicate)
            {
                throw new StlApiException("assets.duplicate_tag", "An asset with this tag already exists.", 409);
            }

            asset.AssetTag = nextAssetTag;
        }

        asset.Name = NormalizeNameOrFallback(ExtractFirst(values, "displayName") ?? request.Name, asset.Name);
        asset.Description = NormalizeDescription(ExtractFirst(values, "description") ?? request.Description);
        if (values.TryGetValue("siteId", out var siteIdRaw))
        {
            var site = await ResolveAssetSiteAsync(tenantId, null, siteIdRaw?.ToString(), cancellationToken);
            asset.SiteRef = site?.OrgUnitId.ToString("D");
            asset.StaffarrSiteOrgUnitId = site?.OrgUnitId;
            asset.StaffarrSiteNameSnapshot = site?.Name ?? string.Empty;
        }
        if (values.TryGetValue("lifecycleStatus", out var lifecycleRaw) && !string.IsNullOrWhiteSpace(lifecycleRaw?.ToString()))
        {
            asset.LifecycleStatus = NormalizeLifecycleStatus(lifecycleRaw!.ToString()!);
        }
        asset.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        await UpsertControlledFieldStoresAsync(tenantId, assetId, values, now, cancellationToken);

        await audit.WriteAsync(
            "asset.update",
            tenantId,
            actorUserId,
            "asset",
            asset.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        var resolvedAssetType = await db.AssetTypes.AsNoTracking()
            .Include(x => x.AssetClass)
            .FirstAsync(x => x.Id == asset.AssetTypeId, cancellationToken);

        return Map(asset, resolvedAssetType, resolvedAssetType.AssetClass);
    }

    public async Task<AssetFieldContextResponse> GetFieldContextAsync(Guid tenantId, Guid assetId, CancellationToken cancellationToken = default)
    {
        var values = await db.AssetCustomFieldValues.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "edit", cancellationToken);
        var fieldByKey = fieldset.Fields.ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);
        var entries = new List<AssetFieldContextValueResponse>();

        foreach (var row in values.OrderBy(x => x.FieldKey))
        {
            var storedValue = JsonSerializer.Deserialize<object>(row.ValueJson);
            var displayValue = BuildDisplayValue(storedValue, fieldByKey.TryGetValue(row.FieldKey, out var field) ? field : null);
            entries.Add(new AssetFieldContextValueResponse(
                row.FieldKey,
                storedValue,
                displayValue,
                field?.Source ?? "maintainarr_catalog",
                field?.SourceOfTruth ?? "MaintainArr"));
        }

        return new AssetFieldContextResponse(assetId, entries);
    }

    public async Task<IReadOnlyList<AssetSearchResponse>> SearchAsync(
        Guid tenantId,
        string? query,
        string? status,
        string? siteRef,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim().ToLowerInvariant();
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToLowerInvariant();
        var normalizedSiteRef = string.IsNullOrWhiteSpace(siteRef) ? null : siteRef.Trim().ToLowerInvariant();
        var take = Math.Clamp(limit, 1, 50);

        var assetQuery = db.Assets
            .AsNoTracking()
            .Include(x => x.AssetType)
            .ThenInclude(x => x.AssetClass)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            assetQuery = assetQuery.Where(x => x.LifecycleStatus == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(normalizedSiteRef))
        {
            assetQuery = assetQuery.Where(x =>
                (x.SiteRef != null && x.SiteRef.ToLower().Contains(normalizedSiteRef))
                || x.StaffarrSiteNameSnapshot.ToLower().Contains(normalizedSiteRef));
        }

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            var needle = normalizedQuery;
            assetQuery = assetQuery.Where(x =>
                x.AssetTag.ToLower().Contains(needle)
                || x.Name.ToLower().Contains(needle)
                || x.Description.ToLower().Contains(needle)
                || x.LifecycleStatus.ToLower().Contains(needle)
                || x.StaffarrSiteNameSnapshot.ToLower().Contains(needle)
                || x.AssetType.TypeKey.ToLower().Contains(needle)
                || x.AssetType.Name.ToLower().Contains(needle)
                || x.AssetType.AssetClass.ClassKey.ToLower().Contains(needle)
                || x.AssetType.AssetClass.Name.ToLower().Contains(needle)
                || db.AssetCustomFieldValues.Any(field =>
                    field.TenantId == tenantId
                    && field.AssetId == x.Id
                    && field.ValueJson.ToLower().Contains(needle))
                || db.AssetSpecs.Any(field =>
                    field.TenantId == tenantId
                    && field.AssetId == x.Id
                    && field.ValueJson.ToLower().Contains(needle))
                || db.AssetComponents.Any(field =>
                    field.TenantId == tenantId
                    && field.AssetId == x.Id
                    && field.ValueJson.ToLower().Contains(needle)));
        }

        var assets = await assetQuery
            .OrderBy(x => x.AssetTag)
            .ThenBy(x => x.Name)
            .Take(take)
            .ToListAsync(cancellationToken);

        if (assets.Count == 0)
        {
            return [];
        }

        var assetIds = assets.Select(x => x.Id).ToArray();
        var openDefectCounts = await db.Defects
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && assetIds.Contains(x.AssetId)
                && (x.Status == DefectStatuses.Open
                    || x.Status == DefectStatuses.Acknowledged
                    || x.Status == DefectStatuses.InRepair))
            .GroupBy(x => x.AssetId)
            .Select(x => new { AssetId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.AssetId, x => x.Count, cancellationToken);

        var openWorkOrderCounts = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && assetIds.Contains(x.AssetId)
                && WorkOrderStatuses.Active.Contains(x.Status))
            .GroupBy(x => x.AssetId)
            .Select(x => new { AssetId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.AssetId, x => x.Count, cancellationToken);

        var readinessByAssetId = await db.AssetReadinessStates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToDictionaryAsync(x => x.AssetId, x => x.ReadinessStatusKey, cancellationToken);

        return assets
            .Select(asset => new AssetSearchResponse(
                asset.Id,
                asset.AssetTypeId,
                asset.AssetType.TypeKey,
                asset.AssetType.Name,
                asset.AssetType.AssetClass.ClassKey,
                asset.AssetType.AssetClass.Name,
                asset.AssetTag,
                asset.Name,
                asset.Description,
                asset.LifecycleStatus,
                asset.SiteRef,
                asset.StaffarrSiteOrgUnitId,
                asset.StaffarrSiteNameSnapshot,
                openDefectCounts.GetValueOrDefault(asset.Id, 0),
                openWorkOrderCounts.GetValueOrDefault(asset.Id, 0),
                readinessByAssetId.GetValueOrDefault(asset.Id, "unknown"),
                asset.CreatedAt,
                asset.UpdatedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<AssetResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Join(
                db.AssetTypes.AsNoTracking(),
                asset => asset.AssetTypeId,
                type => type.Id,
                (asset, type) => new { asset, type })
            .Join(
                db.AssetClasses.AsNoTracking(),
                joined => joined.type.AssetClassId,
                assetClass => assetClass.Id,
                (joined, assetClass) => new { joined.asset, joined.type, assetClass })
            .Where(x => x.type.TenantId == tenantId && x.assetClass.TenantId == tenantId)
            .OrderBy(x => x.asset.AssetTag)
            .Select(x => Map(x.asset, x.type, x.assetClass))
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetResponse> GetAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var result = await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == assetId)
            .Join(
                db.AssetTypes.AsNoTracking(),
                asset => asset.AssetTypeId,
                type => type.Id,
                (asset, type) => new { asset, type })
            .Join(
                db.AssetClasses.AsNoTracking(),
                joined => joined.type.AssetClassId,
                assetClass => assetClass.Id,
                (joined, assetClass) => new { joined.asset, joined.type, assetClass })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        return Map(result.asset, result.type, result.assetClass);
    }

    public async Task<AssetResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        var assetType = await assetTypeService.GetActiveTypeAsync(tenantId, request.AssetTypeId, cancellationToken);
        var assetTag = await ResolveAssetTagAsync(
            tenantId,
            request.AssetTag,
            settings.Assets.AssetNumberingMode,
            settings.Assets.AssetNumberPrefix,
            cancellationToken);
        var name = NormalizeName(request.Name, "Asset name");
        var description = NormalizeDescription(request.Description);
        var site = await ResolveAssetSiteAsync(tenantId, request.StaffarrSiteOrgUnitId, request.SiteRef, cancellationToken);
        var siteRef = site?.OrgUnitId.ToString("D");
        if (settings.Assets.RequireSiteOnAssetCreate && site is null)
        {
            throw new StlApiException("assets.site_required", "A StaffArr site is required by MaintainArr tenant settings.", 400);
        }

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

        var now = DateTimeOffset.UtcNow;
        var entity = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetTypeId = assetType.Id,
            AssetTag = assetTag,
            Name = name,
            Description = description,
            LifecycleStatus = NormalizeLifecycleStatus(settings.Assets.DefaultAssetStatus),
            SiteRef = siteRef,
            StaffarrSiteOrgUnitId = site?.OrgUnitId,
            StaffarrSiteNameSnapshot = site?.Name ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Assets.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset.create",
            tenantId,
            actorUserId,
            "asset",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity, assetType, assetType.AssetClass);
    }

    public async Task<AssetResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetId,
        UpdateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.Assets
            .Include(x => x.AssetType)
            .ThenInclude(x => x.AssetClass)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        entity.Name = NormalizeName(request.Name, "Asset name");
        entity.Description = NormalizeDescription(request.Description);
        var site = await ResolveAssetSiteAsync(tenantId, request.StaffarrSiteOrgUnitId, request.SiteRef, cancellationToken);
        entity.SiteRef = site?.OrgUnitId.ToString("D");
        entity.StaffarrSiteOrgUnitId = site?.OrgUnitId;
        entity.StaffarrSiteNameSnapshot = site?.Name ?? string.Empty;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset.update",
            tenantId,
            actorUserId,
            "asset",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity, entity.AssetType, entity.AssetType.AssetClass);
    }

    public async Task<AssetResponse> UpdateLifecycleStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetId,
        UpdateAssetLifecycleStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var lifecycleStatus = NormalizeLifecycleStatus(request.LifecycleStatus);
        var entity = await db.Assets
            .Include(x => x.AssetType)
            .ThenInclude(x => x.AssetClass)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        entity.LifecycleStatus = lifecycleStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset.lifecycle_update",
            tenantId,
            actorUserId,
            "asset",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity, entity.AssetType, entity.AssetType.AssetClass);
    }

    private static AssetResponse Map(Asset entity, AssetType assetType, AssetClass assetClass) =>
        new(
            entity.Id,
            entity.AssetTypeId,
            assetType.TypeKey,
            assetType.Name,
            assetClass.ClassKey,
            assetClass.Name,
            entity.AssetTag,
            entity.Name,
            entity.Description,
            entity.LifecycleStatus,
            entity.SiteRef,
            entity.StaffarrSiteOrgUnitId,
            entity.StaffarrSiteNameSnapshot,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static AssetSearchResponse MapSearch(
        Asset entity,
        AssetType assetType,
        AssetClass assetClass,
        int openDefectCount,
        int openWorkOrderCount,
        string readinessStatus) =>
        new(
            entity.Id,
            entity.AssetTypeId,
            assetType.TypeKey,
            assetType.Name,
            assetClass.ClassKey,
            assetClass.Name,
            entity.AssetTag,
            entity.Name,
            entity.Description,
            entity.LifecycleStatus,
            entity.SiteRef,
            entity.StaffarrSiteOrgUnitId,
            entity.StaffarrSiteNameSnapshot,
            openDefectCount,
            openWorkOrderCount,
            readinessStatus,
            entity.CreatedAt,
            entity.UpdatedAt);

    private async Task<MaintainArrStaffArrSite?> ResolveAssetSiteAsync(
        Guid tenantId,
        Guid? staffarrSiteOrgUnitId,
        string? legacySiteAlias,
        CancellationToken cancellationToken)
    {
        if (staffarrSiteOrgUnitId is Guid siteId)
        {
            return await staffArrSites.RequireActiveSiteAsync(tenantId, siteId, cancellationToken);
        }

        return await staffArrSites.ResolveOptionalSiteAsync(tenantId, NormalizeSiteRef(legacySiteAlias), cancellationToken);
    }

    private static IReadOnlyDictionary<string, object?> BuildAssetUpsertValues(AssetUpsertV1Request request)
    {
        var values = new Dictionary<string, object?>(request.Values, StringComparer.OrdinalIgnoreCase);
        if (!values.ContainsKey("unitNumber") && !string.IsNullOrWhiteSpace(request.AssetTag))
        {
            values["unitNumber"] = request.AssetTag.Trim();
        }

        if (!values.ContainsKey("displayName") && !string.IsNullOrWhiteSpace(request.Name))
        {
            values["displayName"] = request.Name.Trim();
        }

        if (!values.ContainsKey("description") && !string.IsNullOrWhiteSpace(request.Description))
        {
            values["description"] = request.Description.Trim();
        }

        if (!values.ContainsKey("assetStatus"))
        {
            values["assetStatus"] = "active";
        }

        if (!values.ContainsKey("criticality"))
        {
            values["criticality"] = "medium";
        }

        if (values.TryGetValue("lifecycleStatus", out var lifecycleRaw)
            && lifecycleRaw?.ToString() is { } lifecycleStatus)
        {
            var normalized = lifecycleStatus.Trim().ToLowerInvariant();
            values["lifecycleStatus"] = normalized switch
            {
                "active" => "in_service",
                "inactive" => "temporarily_inactive",
                _ => normalized
            };
        }

        return values;
    }

    private async Task<string> ResolveAssetTagAsync(
        Guid tenantId,
        string? requestedAssetTag,
        string numberingMode,
        string? prefix,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedAssetTag))
        {
            return NormalizeAssetTag(requestedAssetTag);
        }

        if (!string.Equals(numberingMode, "auto", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "assets.validation",
                "Asset tag is required when asset numbering is manual.",
                400);
        }

        var normalizedPrefix = string.IsNullOrWhiteSpace(prefix) ? "AST" : prefix.Trim().ToUpperInvariant();
        var dayKey = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        for (var index = 1; index <= 9999; index++)
        {
            var candidate = NormalizeAssetTag($"{normalizedPrefix}-{dayKey}-{index:D4}");
            var exists = await db.Assets.AnyAsync(
                x => x.TenantId == tenantId && x.AssetTag == candidate,
                cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        throw new StlApiException(
            "assets.numbering_exhausted",
            "Asset numbering sequence is exhausted for today.",
            409);
    }

    private static void EnsureVinOrSerialIfRequired(
        MaintainArrTenantSettingsDto settings,
        IReadOnlyDictionary<string, object?> values)
    {
        if (!settings.Assets.RequireVinOrSerial)
        {
            return;
        }

        var hasIdentifier = HasAnyValue(
            values,
            "vin",
            "vehicleIdentificationNumber",
            "serial",
            "serialNumber",
            "unitSerialNumber");

        if (!hasIdentifier)
        {
            throw new StlApiException(
                "assets.vin_or_serial_required",
                "VIN or serial number is required by MaintainArr tenant settings.",
                400);
        }
    }

    private static bool HasAnyValue(IReadOnlyDictionary<string, object?> values, params string[] keys) =>
        keys.Any(key => !string.IsNullOrWhiteSpace(ExtractFirst(values, key)));

    private static string NormalizeAssetTag(string value)
    {
        var trimmed = value.Trim().ToUpperInvariant();
        if (trimmed.Length < 2 || trimmed.Length > 64)
        {
            throw new StlApiException(
                "assets.validation",
                "Asset tag must be between 2 and 64 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeName(string value, string label)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "assets.validation",
                $"{label} must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeNameOrFallback(string? value, string fallback)
    {
        var candidate = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return NormalizeName(candidate, "Asset name");
    }

    private static string NormalizeDescription(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string? NormalizeSiteRef(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 128)
        {
            throw new StlApiException(
                "assets.validation",
                "Site reference must be 128 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeLifecycleStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedLifecycleStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "assets.validation",
                "Lifecycle status is invalid for this asset.",
                400);
        }

        return normalized;
    }

    private async Task UpsertControlledFieldStoresAsync(
        Guid tenantId,
        Guid assetId,
        IReadOnlyDictionary<string, object?> values,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existingCustom = await db.AssetCustomFieldValues
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);
        if (existingCustom.Count > 0)
        {
            db.AssetCustomFieldValues.RemoveRange(existingCustom);
        }

        var existingSpecs = await db.AssetSpecs.Where(x => x.TenantId == tenantId && x.AssetId == assetId).ToListAsync(cancellationToken);
        if (existingSpecs.Count > 0)
        {
            db.AssetSpecs.RemoveRange(existingSpecs);
        }

        var existingComponents = await db.AssetComponents.Where(x => x.TenantId == tenantId && x.AssetId == assetId).ToListAsync(cancellationToken);
        if (existingComponents.Count > 0)
        {
            db.AssetComponents.RemoveRange(existingComponents);
        }

        var existingCompliance = await db.AssetComplianceStates.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);
        if (existingCompliance is not null)
        {
            db.AssetComplianceStates.Remove(existingCompliance);
        }

        foreach (var kvp in values)
        {
            var serialized = JsonSerializer.Serialize(kvp.Value);
            db.AssetCustomFieldValues.Add(new AssetCustomFieldValue
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                FieldKey = kvp.Key,
                ValueJson = serialized,
                CreatedAt = now,
                UpdatedAt = now,
            });

            if (SpecFieldKeys.Contains(kvp.Key))
            {
                db.AssetSpecs.Add(new AssetSpec
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    SpecKey = kvp.Key,
                    ValueJson = serialized,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }

            if (ComponentFieldKeys.Contains(kvp.Key))
            {
                db.AssetComponents.Add(new AssetComponent
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    ComponentKey = kvp.Key,
                    ValueJson = serialized,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
        }

        var complianceState = BuildComplianceState(tenantId, assetId, values, now);
        if (complianceState is not null)
        {
            db.AssetComplianceStates.Add(complianceState);
        }

        var readinessState = BuildReadinessState(tenantId, assetId, values, now);
        if (readinessState is not null)
        {
            var existingReadiness = await db.AssetReadinessStates.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.AssetId == assetId,
                cancellationToken);
            if (existingReadiness is not null)
            {
                db.AssetReadinessStates.Remove(existingReadiness);
            }

            db.AssetReadinessStates.Add(readinessState);
        }

        var locationHistoryRow = BuildLocationHistory(tenantId, assetId, values, now);
        if (locationHistoryRow is not null)
        {
            db.AssetLocationHistory.Add(locationHistoryRow);
        }

        var assignmentRows = BuildAssignmentHistory(tenantId, assetId, values, now);
        if (assignmentRows.Count > 0)
        {
            db.AssetAssignmentHistory.AddRange(assignmentRows);
        }

        var statusRows = BuildStatusHistory(tenantId, assetId, values, now);
        if (statusRows.Count > 0)
        {
            db.AssetStatusHistory.AddRange(statusRows);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static AssetComplianceState? BuildComplianceState(
        Guid tenantId,
        Guid assetId,
        IReadOnlyDictionary<string, object?> values,
        DateTimeOffset now)
    {
        var governingBody = ExtractStrings(values.TryGetValue("governingBodyKey", out var governingBodyRaw) ? governingBodyRaw : null);
        var rulepacks = ExtractStrings(values.TryGetValue("rulepackApplicabilityKeys", out var rulepacksRaw) ? rulepacksRaw : null);
        var categories = ExtractStrings(values.TryGetValue("complianceCategory", out var categoriesRaw) ? categoriesRaw : null);

        var hasAny = governingBody.Count > 0 || rulepacks.Count > 0 || categories.Count > 0;
        if (!hasAny)
        {
            return null;
        }

        return new AssetComplianceState
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = assetId,
            GoverningBodyKeysJson = JsonSerializer.Serialize(governingBody),
            RulepackApplicabilityKeysJson = JsonSerializer.Serialize(rulepacks),
            ComplianceCategoryKeysJson = JsonSerializer.Serialize(categories),
            UpdatedAt = now,
        };
    }

    private static AssetReadinessState? BuildReadinessState(
        Guid tenantId,
        Guid assetId,
        IReadOnlyDictionary<string, object?> values,
        DateTimeOffset now)
    {
        var readinessStatus = ExtractFirst(values, "readinessStatus");
        var operationalStatus = ExtractFirst(values, "operationalStatus");
        var availabilityStatus = ExtractFirst(values, "availabilityStatus");

        if (string.IsNullOrWhiteSpace(readinessStatus)
            && string.IsNullOrWhiteSpace(operationalStatus)
            && string.IsNullOrWhiteSpace(availabilityStatus))
        {
            return null;
        }

        return new AssetReadinessState
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = assetId,
            ReadinessStatusKey = string.IsNullOrWhiteSpace(readinessStatus) ? "blocked" : readinessStatus!,
            OperationalStatusKey = string.IsNullOrWhiteSpace(operationalStatus) ? "unknown" : operationalStatus!,
            AvailabilityStatusKey = string.IsNullOrWhiteSpace(availabilityStatus) ? "unavailable" : availabilityStatus!,
            Basis = "fieldset_controlled_values",
            Notes = null,
            UpdatedAt = now,
        };
    }

    private static AssetLocationHistory? BuildLocationHistory(
        Guid tenantId,
        Guid assetId,
        IReadOnlyDictionary<string, object?> values,
        DateTimeOffset now)
    {
        var siteId = ExtractFirst(values, "siteId");
        var homeLocationId = ExtractFirst(values, "homeLocationId");
        var currentLocationId = ExtractFirst(values, "currentLocationId");
        var yard = ExtractFirst(values, "yard");
        var bay = ExtractFirst(values, "bay");
        var parkingSpot = ExtractFirst(values, "parkingSpot");

        if (string.IsNullOrWhiteSpace(siteId)
            && string.IsNullOrWhiteSpace(homeLocationId)
            && string.IsNullOrWhiteSpace(currentLocationId)
            && string.IsNullOrWhiteSpace(yard)
            && string.IsNullOrWhiteSpace(bay)
            && string.IsNullOrWhiteSpace(parkingSpot))
        {
            return null;
        }

        return new AssetLocationHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = assetId,
            SiteId = siteId,
            StaffarrSiteOrgUnitId = Guid.TryParse(siteId, out var staffarrSiteOrgUnitId) ? staffarrSiteOrgUnitId : null,
            HomeLocationId = homeLocationId,
            CurrentLocationId = currentLocationId,
            Yard = yard,
            Bay = bay,
            ParkingSpot = parkingSpot,
            ChangedByPersonId = null,
            EffectiveAt = now,
            CreatedAt = now,
        };
    }

    private static IReadOnlyList<AssetAssignmentHistory> BuildAssignmentHistory(
        Guid tenantId,
        Guid assetId,
        IReadOnlyDictionary<string, object?> values,
        DateTimeOffset now)
    {
        var fields = new[]
        {
            "assignedPersonId",
            "responsiblePersonId",
            "operatorPersonId",
            "driverPersonId",
            "ownerPersonId",
            "custodianPersonId",
            "maintenanceSupervisorPersonId",
            "defaultTechnicianPersonId",
            "lastInspectedByPersonId",
            "lastServicedByPersonId",
            "outOfServiceByPersonId",
            "returnedToServiceByPersonId",
        };

        var rows = new List<AssetAssignmentHistory>();
        foreach (var field in fields)
        {
            var personId = ExtractFirst(values, field);
            if (string.IsNullOrWhiteSpace(personId))
            {
                continue;
            }

            rows.Add(new AssetAssignmentHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                AssignmentFieldKey = field,
                PersonId = personId!,
                ChangedByPersonId = null,
                EffectiveAt = now,
                CreatedAt = now,
            });
        }

        return rows;
    }

    private static IReadOnlyList<AssetStatusHistory> BuildStatusHistory(
        Guid tenantId,
        Guid assetId,
        IReadOnlyDictionary<string, object?> values,
        DateTimeOffset now)
    {
        var fields = new[] { "assetStatus", "lifecycleStatus", "readinessStatus", "operationalStatus", "availabilityStatus" };
        var rows = new List<AssetStatusHistory>();
        foreach (var field in fields)
        {
            var value = ExtractFirst(values, field);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            rows.Add(new AssetStatusHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                StatusFieldKey = field,
                StatusValueKey = value!,
                ChangedByPersonId = null,
                Notes = null,
                ChangedAt = now,
                CreatedAt = now,
            });
        }

        return rows;
    }

    private async Task<AssetType> ResolveOrCreateAssetTypeProjectionAsync(
        Guid tenantId,
        string? assetClassKey,
        string assetTypeKey,
        CancellationToken cancellationToken)
    {
        var normalizedTypeKey = assetTypeKey.Trim().ToLowerInvariant();
        var existing = await db.AssetTypes
            .Include(x => x.AssetClass)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.TypeKey == normalizedTypeKey && x.Status == "active", cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        if (string.IsNullOrWhiteSpace(assetClassKey))
        {
            throw new StlApiException("assets.validation", "assetClass is required when assetType projection does not yet exist.", 400);
        }

        var normalizedClassKey = assetClassKey.Trim().ToLowerInvariant();
        var assetClass = await db.AssetClasses.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.ClassKey == normalizedClassKey,
            cancellationToken);
        if (assetClass is null)
        {
            assetClass = new AssetClass
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClassKey = normalizedClassKey,
                Name = HumanizeKey(normalizedClassKey),
                Description = string.Empty,
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            db.AssetClasses.Add(assetClass);
            await db.SaveChangesAsync(cancellationToken);
        }

        var projectedType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetClassId = assetClass.Id,
            TypeKey = normalizedTypeKey,
            Name = HumanizeKey(normalizedTypeKey),
            Description = string.Empty,
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            AssetClass = assetClass,
        };

        db.AssetTypes.Add(projectedType);
        await db.SaveChangesAsync(cancellationToken);
        return projectedType;
    }

    private static string BuildDisplayValue(object? storedValue, FieldMetadataResponse? field)
    {
        if (storedValue is null)
        {
            return string.Empty;
        }

        if (field?.Options is null || field.Options.Count == 0)
        {
            return storedValue.ToString() ?? string.Empty;
        }

        var optionMap = field.Options.ToDictionary(x => x.Key, x => x.Label, StringComparer.OrdinalIgnoreCase);
        var keys = ExtractStrings(storedValue);
        if (keys.Count == 0)
        {
            return storedValue.ToString() ?? string.Empty;
        }

        var labels = keys.Select(x => optionMap.TryGetValue(x, out var label) ? label : x).ToList();
        return string.Join(", ", labels);
    }

    private static IReadOnlyList<string> ExtractStrings(object? raw)
    {
        if (raw is null)
        {
            return [];
        }

        if (raw is string text)
        {
            return string.IsNullOrWhiteSpace(text) ? [] : [text.Trim()];
        }

        if (raw is JsonElement json)
        {
            return json.ValueKind switch
            {
                JsonValueKind.Array => json.EnumerateArray()
                    .Select(x => x.ToString().Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList(),
                JsonValueKind.String => string.IsNullOrWhiteSpace(json.GetString()) ? [] : [json.GetString()!.Trim()],
                JsonValueKind.Null => [],
                _ => [json.ToString().Trim()],
            };
        }

        if (raw is IEnumerable<object?> list)
        {
            return list
                .Select(x => x?.ToString()?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
        }

        return [raw.ToString()!.Trim()];
    }

    private static string? ExtractFirst(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            return null;
        }

        var valuesList = ExtractStrings(raw);
        return valuesList.Count == 0 ? null : valuesList[0];
    }

    private static string HumanizeKey(string key)
    {
        var replaced = key.Replace('_', ' ');
        return string.Join(' ', replaced
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
    }
}
