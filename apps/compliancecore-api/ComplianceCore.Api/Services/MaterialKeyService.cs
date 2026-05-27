using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class MaterialKeyService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<MaterialKeyResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.MaterialKeys
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderBy(x => x.Label)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<MaterialKeyResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateMaterialKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        var key = NormalizeKey(request.Key);
        var label = NormalizeLabel(request.Label);
        var category = NormalizeCategory(request.Category);
        var description = NormalizeDescription(request.Description);

        var exists = await db.MaterialKeys.AnyAsync(
            x => x.TenantId == tenantId && x.Key == key,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "material_keys.duplicate",
                "A material key with this identifier already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new MaterialKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Key = key,
            Label = label,
            Category = category,
            Description = description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.MaterialKeys.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "material_key.create",
            tenantId,
            actorUserId,
            "material_key",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static MaterialKeyResponse MapResponse(MaterialKey entity) =>
        new(
            entity.Id,
            entity.Key,
            entity.Label,
            entity.Category,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt);

    private static string NormalizeKey(string key)
    {
        var normalized = key.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "material_keys.validation",
                "Key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeLabel(string label)
    {
        var trimmed = label.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "material_keys.validation",
                "Label must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeCategory(string category)
    {
        var normalized = category.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "material_keys.validation",
                "Category must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDescription(string description)
    {
        var trimmed = description.Trim();
        if (trimmed.Length < 4 || trimmed.Length > 1024)
        {
            throw new StlApiException(
                "material_keys.validation",
                "Description must be between 4 and 1024 characters.",
                400);
        }

        return trimmed;
    }
}
