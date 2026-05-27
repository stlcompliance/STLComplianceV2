using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetClassService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    public async Task<IReadOnlyList<AssetClassResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.AssetClasses
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetClassResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateAssetClassRequest request,
        CancellationToken cancellationToken = default)
    {
        var classKey = NormalizeKey(request.ClassKey, "Class key");
        var name = NormalizeName(request.Name, "Class name");
        var description = NormalizeDescription(request.Description);

        var exists = await db.AssetClasses.AnyAsync(
            x => x.TenantId == tenantId && x.ClassKey == classKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "asset_classes.duplicate",
                "An asset class with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AssetClass
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClassKey = classKey,
            Name = name,
            Description = description,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.AssetClasses.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset_class.create",
            tenantId,
            actorUserId,
            "asset_class",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<AssetClassResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetClassId,
        UpdateAssetClassRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AssetClasses.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == assetClassId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("asset_classes.not_found", "Asset class was not found.", 404);
        }

        entity.Name = NormalizeName(request.Name, "Class name");
        entity.Description = NormalizeDescription(request.Description);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset_class.update",
            tenantId,
            actorUserId,
            "asset_class",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<AssetClassResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetClassId,
        UpdateAssetClassStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await db.AssetClasses.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == assetClassId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("asset_classes.not_found", "Asset class was not found.", 404);
        }

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset_class.status_update",
            tenantId,
            actorUserId,
            "asset_class",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<AssetClass> GetActiveClassAsync(
        Guid tenantId,
        Guid assetClassId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AssetClasses.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == assetClassId && x.Status == "active",
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("asset_classes.not_found", "Asset class was not found.", 404);
        }

        return entity;
    }

    private static AssetClassResponse Map(AssetClass entity) =>
        new(
            entity.Id,
            entity.ClassKey,
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
                "asset_classes.validation",
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
                "asset_classes.validation",
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
                "asset_classes.validation",
                "Status must be active or inactive.",
                400);
        }

        return normalized;
    }
}
