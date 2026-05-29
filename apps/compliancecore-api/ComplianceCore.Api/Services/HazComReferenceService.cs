using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class HazComReferenceService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<HazComReferenceResponse>> ListAsync(
        Guid tenantId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var query = db.HazComReferences.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.HazComKey)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<HazComReferenceResponse> GetAsync(
        Guid tenantId,
        Guid hazComReferenceId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, hazComReferenceId, cancellationToken);
        return MapResponse(entity);
    }

    public async Task<HazComReferenceResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateHazComReferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var hazComKey = NormalizeKey(request.HazComKey);
        var exists = await db.HazComReferences.AnyAsync(
            x => x.TenantId == tenantId && x.HazComKey == hazComKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "hazcom.duplicate",
                "A HazCom reference with this key already exists.",
                409);
        }

        await ValidateLinkedSdsKeyAsync(tenantId, request.LinkedSdsKey, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = new HazComReference
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            HazComKey = hazComKey,
            Title = NormalizeRequired(request.Title, "Title"),
            Description = NormalizeOptional(request.Description),
            LinkedSdsKey = NormalizeOptionalKey(request.LinkedSdsKey),
            LocationRef = NormalizeOptional(request.LocationRef),
            DocumentUrl = NormalizeOptional(request.DocumentUrl),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.HazComReferences.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "hazcom_reference.create",
            tenantId,
            actorUserId,
            "hazcom_reference",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<HazComReferenceResponse> UpdateAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid hazComReferenceId,
        UpdateHazComReferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, hazComReferenceId, cancellationToken);
        if (request.Title is not null)
        {
            entity.Title = NormalizeRequired(request.Title, "Title");
        }

        if (request.Description is not null)
        {
            entity.Description = NormalizeOptional(request.Description);
        }

        if (request.LinkedSdsKey is not null)
        {
            await ValidateLinkedSdsKeyAsync(tenantId, request.LinkedSdsKey, cancellationToken);
            entity.LinkedSdsKey = NormalizeOptionalKey(request.LinkedSdsKey);
        }

        if (request.LocationRef is not null)
        {
            entity.LocationRef = NormalizeOptional(request.LocationRef);
        }

        if (request.DocumentUrl is not null)
        {
            entity.DocumentUrl = NormalizeOptional(request.DocumentUrl);
        }

        if (request.IsActive.HasValue)
        {
            entity.IsActive = request.IsActive.Value;
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "hazcom_reference.update",
            tenantId,
            actorUserId,
            "hazcom_reference",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private async Task<HazComReference> LoadAsync(
        Guid tenantId,
        Guid hazComReferenceId,
        CancellationToken cancellationToken)
    {
        var entity = await db.HazComReferences.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == hazComReferenceId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("hazcom.not_found", "HazCom reference was not found.", 404);
        }

        return entity;
    }

    private async Task ValidateLinkedSdsKeyAsync(
        Guid tenantId,
        string? linkedSdsKey,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeOptionalKey(linkedSdsKey);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        var exists = await db.SdsReferences.AnyAsync(
            x => x.TenantId == tenantId && x.SdsKey == normalized && x.IsActive,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("sds.not_found", "Linked SDS key was not found.", 404);
        }
    }

    private static HazComReferenceResponse MapResponse(HazComReference entity) =>
        new(
            entity.Id,
            entity.HazComKey,
            entity.Title,
            entity.Description,
            entity.LinkedSdsKey,
            entity.LocationRef,
            entity.DocumentUrl,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeKey(string key)
    {
        var normalized = key.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 64)
        {
            throw new StlApiException(
                "hazcom.validation",
                "HazCom key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRequired(string value, string field)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "hazcom.validation",
                $"{field} is required.",
                400);
        }

        return normalized;
    }

    private static string NormalizeOptional(string? value) => value?.Trim() ?? string.Empty;

    private static string? NormalizeOptionalKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant();
    }
}
