using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PartCatalogService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    public async Task<IReadOnlyList<PartCatalogResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var catalogs = await db.PartCatalogs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return catalogs.Select(Map).ToList();
    }

    public async Task<PartCatalogResponse> GetAsync(
        Guid tenantId,
        Guid catalogId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadCatalogAsync(tenantId, catalogId, cancellationToken);
        return Map(entity);
    }

    public async Task<PartCatalogResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePartCatalogRequest request,
        CancellationToken cancellationToken = default)
    {
        var catalogKey = NormalizeCatalogKey(request.CatalogKey);
        var exists = await db.PartCatalogs.AnyAsync(
            x => x.TenantId == tenantId && x.CatalogKey == catalogKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "catalogs.duplicate",
                "A part catalog with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new PartCatalog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CatalogKey = catalogKey,
            Name = NormalizeName(request.Name),
            Description = NormalizeDescription(request.Description),
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PartCatalogs.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part_catalog.create",
            tenantId,
            actorUserId,
            "part_catalog",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<PartCatalogResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid catalogId,
        UpdatePartCatalogRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PartCatalogs.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == catalogId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("catalogs.not_found", "Part catalog was not found.", 404);
        }

        entity.Name = NormalizeName(request.Name);
        entity.Description = NormalizeDescription(request.Description);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part_catalog.update",
            tenantId,
            actorUserId,
            "part_catalog",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<PartCatalogResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid catalogId,
        UpdatePartCatalogStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await db.PartCatalogs.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == catalogId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("catalogs.not_found", "Part catalog was not found.", 404);
        }

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part_catalog.status_update",
            tenantId,
            actorUserId,
            "part_catalog",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    private async Task<PartCatalog> LoadCatalogAsync(
        Guid tenantId,
        Guid catalogId,
        CancellationToken cancellationToken)
    {
        var entity = await db.PartCatalogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == catalogId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("catalogs.not_found", "Part catalog was not found.", 404);
        }

        return entity;
    }

    private static PartCatalogResponse Map(PartCatalog entity) =>
        new(
            entity.Id,
            entity.CatalogKey,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeCatalogKey(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 128)
        {
            throw new StlApiException(
                "catalogs.validation",
                "Catalog key must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 256)
        {
            throw new StlApiException(
                "catalogs.validation",
                "Name must be between 2 and 256 characters.",
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
                "catalogs.validation",
                "Status must be active or inactive.",
                400);
        }

        return normalized;
    }
}
