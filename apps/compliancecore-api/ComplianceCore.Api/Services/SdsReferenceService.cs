using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class SdsReferenceService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<SdsReferenceResponse>> ListAsync(
        Guid tenantId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = db.SdsReferences.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        var rows = await query
            .OrderBy(x => x.SdsKey)
            .GroupJoin(
                db.MaterialKeys.AsNoTracking(),
                sds => sds.MaterialKeyId,
                material => material.Id,
                (sds, materials) => new { sds, material = materials.FirstOrDefault() })
            .ToListAsync(cancellationToken);

        return rows.Select(x => MapResponse(x.sds, x.material?.Key)).ToList();
    }

    public async Task<SdsReferenceResponse> GetAsync(
        Guid tenantId,
        Guid sdsReferenceId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, sdsReferenceId, cancellationToken);
        var materialKey = entity.MaterialKeyId.HasValue
            ? await db.MaterialKeys.AsNoTracking()
                .Where(x => x.Id == entity.MaterialKeyId.Value)
                .Select(x => x.Key)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        return MapResponse(entity, materialKey);
    }

    public async Task<SdsReferenceResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateSdsReferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var sdsKey = NormalizeKey(request.SdsKey);
        var exists = await db.SdsReferences.AnyAsync(
            x => x.TenantId == tenantId && x.SdsKey == sdsKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "sds.duplicate",
                "An SDS reference with this key already exists.",
                409);
        }

        var materialKeyId = await ResolveMaterialKeyIdAsync(tenantId, request.MaterialKeyId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = new SdsReference
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SdsKey = sdsKey,
            MaterialKeyId = materialKeyId,
            ProductName = NormalizeOptional(request.ProductName),
            Manufacturer = NormalizeOptional(request.Manufacturer),
            DocumentUrl = NormalizeOptional(request.DocumentUrl),
            RevisionDate = request.RevisionDate,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.SdsReferences.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "sds_reference.create",
            tenantId,
            actorUserId,
            "sds_reference",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<SdsReferenceResponse> UpdateAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid sdsReferenceId,
        UpdateSdsReferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, sdsReferenceId, cancellationToken);
        if (request.MaterialKeyId.HasValue)
        {
            entity.MaterialKeyId = await ResolveMaterialKeyIdAsync(
                tenantId,
                request.MaterialKeyId,
                cancellationToken);
        }

        if (request.ProductName is not null)
        {
            entity.ProductName = NormalizeOptional(request.ProductName);
        }

        if (request.Manufacturer is not null)
        {
            entity.Manufacturer = NormalizeOptional(request.Manufacturer);
        }

        if (request.DocumentUrl is not null)
        {
            entity.DocumentUrl = NormalizeOptional(request.DocumentUrl);
        }

        if (request.RevisionDate.HasValue)
        {
            entity.RevisionDate = request.RevisionDate;
        }

        if (request.IsActive.HasValue)
        {
            entity.IsActive = request.IsActive.Value;
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "sds_reference.update",
            tenantId,
            actorUserId,
            "sds_reference",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private async Task<SdsReference> LoadAsync(
        Guid tenantId,
        Guid sdsReferenceId,
        CancellationToken cancellationToken)
    {
        var entity = await db.SdsReferences.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == sdsReferenceId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("sds.not_found", "SDS reference was not found.", 404);
        }

        return entity;
    }

    private async Task<Guid?> ResolveMaterialKeyIdAsync(
        Guid tenantId,
        Guid? materialKeyId,
        CancellationToken cancellationToken)
    {
        if (!materialKeyId.HasValue)
        {
            return null;
        }

        var exists = await db.MaterialKeys.AnyAsync(
            x => x.TenantId == tenantId && x.Id == materialKeyId.Value && x.IsActive,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("material_keys.not_found", "Material key was not found.", 404);
        }

        return materialKeyId;
    }

    private static SdsReferenceResponse MapResponse(SdsReference entity, string? materialKey) =>
        new(
            entity.Id,
            entity.SdsKey,
            entity.MaterialKeyId,
            materialKey,
            entity.ProductName,
            entity.Manufacturer,
            entity.DocumentUrl,
            entity.RevisionDate,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeKey(string key)
    {
        var normalized = key.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 64)
        {
            throw new StlApiException(
                "sds.validation",
                "SDS key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeOptional(string? value) => value?.Trim() ?? string.Empty;
}
