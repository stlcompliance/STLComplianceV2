using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetService(
    MaintainArrDbContext db,
    AssetTypeService assetTypeService,
    IMaintainArrAuditService audit,
    FieldsetService fieldsetService,
    ControlledValueValidationService controlledValueValidationService)
{
    private static readonly HashSet<string> AllowedLifecycleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive",
        "retired",
        "out_of_service"
    };

    public async Task<AssetResponse> CreateV1Async(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        AssetUpsertV1Request request,
        CancellationToken cancellationToken = default)
    {
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", cancellationToken);
        await controlledValueValidationService.ValidateFieldsetValuesAsync(
            tenantId,
            fieldset.Fields,
            request.Values,
            actorPersonId,
            "asset",
            "new",
            cancellationToken);

        var assetTypeKey = request.Values.TryGetValue("assetType", out var rawType) ? rawType?.ToString() : null;
        if (string.IsNullOrWhiteSpace(assetTypeKey))
        {
            throw new StlApiException("assets.validation", "assetType is required.", 400);
        }

        var assetType = await db.AssetTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.TypeKey == assetTypeKey.Trim().ToLowerInvariant() && x.Status == "active", cancellationToken)
            ?? throw new StlApiException("assets.validation", $"assetType '{assetTypeKey}' is invalid.", 400);

        var created = await CreateAsync(
            tenantId,
            actorUserId,
            new CreateAssetRequest(assetType.Id, request.AssetTag, request.Name, request.Description ?? string.Empty, null),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var kvp in request.Values)
        {
            db.AssetCustomFieldValues.Add(new AssetCustomFieldValue
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = created.AssetId,
                FieldKey = kvp.Key,
                ValueJson = System.Text.Json.JsonSerializer.Serialize(kvp.Value),
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return created;
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
        await controlledValueValidationService.ValidateFieldsetValuesAsync(
            tenantId,
            fieldset.Fields,
            request.Values,
            actorPersonId,
            "asset",
            assetId.ToString(),
            cancellationToken);

        var updated = await UpdateAsync(
            tenantId,
            actorUserId,
            assetId,
            new UpdateAssetRequest(request.Name, request.Description ?? string.Empty, null),
            cancellationToken);

        var existing = await db.AssetCustomFieldValues.Where(x => x.TenantId == tenantId && x.AssetId == assetId).ToListAsync(cancellationToken);
        db.AssetCustomFieldValues.RemoveRange(existing);
        var now = DateTimeOffset.UtcNow;
        foreach (var kvp in request.Values)
        {
            db.AssetCustomFieldValues.Add(new AssetCustomFieldValue
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                FieldKey = kvp.Key,
                ValueJson = System.Text.Json.JsonSerializer.Serialize(kvp.Value),
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
        await db.SaveChangesAsync(cancellationToken);
        return updated;
    }

    public async Task<AssetFieldContextResponse> GetFieldContextAsync(Guid tenantId, Guid assetId, CancellationToken cancellationToken = default)
    {
        var values = await db.AssetCustomFieldValues.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        var map = values.ToDictionary(x => x.FieldKey, x => System.Text.Json.JsonSerializer.Deserialize<object>(x.ValueJson));
        var display = values.ToDictionary(x => x.FieldKey, x => x.ValueJson);
        return new AssetFieldContextResponse(assetId, map, display);
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
        var assetType = await assetTypeService.GetActiveTypeAsync(tenantId, request.AssetTypeId, cancellationToken);
        var assetTag = NormalizeAssetTag(request.AssetTag);
        var name = NormalizeName(request.Name, "Asset name");
        var description = NormalizeDescription(request.Description);
        var siteRef = NormalizeSiteRef(request.SiteRef);

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
            LifecycleStatus = "active",
            SiteRef = siteRef,
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
        entity.SiteRef = NormalizeSiteRef(request.SiteRef);
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
            entity.CreatedAt,
            entity.UpdatedAt);

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
                "Lifecycle status must be active, inactive, retired, or out_of_service.",
                400);
        }

        return normalized;
    }
}
