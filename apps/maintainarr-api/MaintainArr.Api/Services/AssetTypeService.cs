using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetTypeService(
    MaintainArrDbContext db,
    AssetClassService assetClassService,
    IMaintainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    public async Task<IReadOnlyList<AssetTypeResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.AssetTypes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Join(
                db.AssetClasses.AsNoTracking(),
                type => type.AssetClassId,
                assetClass => assetClass.Id,
                (type, assetClass) => new { type, assetClass })
            .Where(x => x.assetClass.TenantId == tenantId)
            .OrderBy(x => x.assetClass.Name)
            .ThenBy(x => x.type.Name)
            .Select(x => Map(x.type, x.assetClass))
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetTypeResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateAssetTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var assetClass = await assetClassService.GetActiveClassAsync(tenantId, request.AssetClassId, cancellationToken);
        var typeKey = NormalizeKey(request.TypeKey, "Type key");
        var name = NormalizeName(request.Name, "Type name");
        var description = NormalizeDescription(request.Description);

        var exists = await db.AssetTypes.AnyAsync(
            x => x.TenantId == tenantId && x.TypeKey == typeKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "asset_types.duplicate",
                "An asset type with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetClassId = assetClass.Id,
            TypeKey = typeKey,
            Name = name,
            Description = description,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.AssetTypes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset_type.create",
            tenantId,
            actorUserId,
            "asset_type",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity, assetClass);
    }

    public async Task<AssetTypeResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetTypeId,
        UpdateAssetTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AssetTypes
            .Include(x => x.AssetClass)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetTypeId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("asset_types.not_found", "Asset type was not found.", 404);
        }

        entity.Name = NormalizeName(request.Name, "Type name");
        entity.Description = NormalizeDescription(request.Description);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset_type.update",
            tenantId,
            actorUserId,
            "asset_type",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity, entity.AssetClass);
    }

    public async Task<AssetTypeResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetTypeId,
        UpdateAssetTypeStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await db.AssetTypes
            .Include(x => x.AssetClass)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetTypeId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("asset_types.not_found", "Asset type was not found.", 404);
        }

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset_type.status_update",
            tenantId,
            actorUserId,
            "asset_type",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity, entity.AssetClass);
    }

    public async Task<AssetType> GetActiveTypeAsync(
        Guid tenantId,
        Guid assetTypeId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AssetTypes
            .Include(x => x.AssetClass)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == assetTypeId && x.Status == "active",
                cancellationToken);
        if (entity is null || entity.AssetClass.Status != "active")
        {
            throw new StlApiException("asset_types.not_found", "Asset type was not found.", 404);
        }

        return entity;
    }

    private static AssetTypeResponse Map(AssetType entity, AssetClass assetClass) =>
        new(
            entity.Id,
            entity.AssetClassId,
            assetClass.ClassKey,
            assetClass.Name,
            entity.TypeKey,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.CreatedAt);

    private static string NormalizeKey(string value, string label)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 128)
        {
            throw new StlApiException(
                "asset_types.validation",
                $"{label} must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string value, string label)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "asset_types.validation",
                $"{label} must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "asset_types.validation",
                "Status must be active or inactive.",
                400);
        }

        return normalized;
    }
}
