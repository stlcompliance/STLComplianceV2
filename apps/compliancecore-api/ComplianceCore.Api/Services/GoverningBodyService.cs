using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class GoverningBodyService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<GoverningBodyResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.GoverningBodies
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderBy(x => x.Label)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<GoverningBodyResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateGoverningBodyRequest request,
        CancellationToken cancellationToken = default)
    {
        var bodyKey = NormalizeKey(request.BodyKey, "governing_bodies.validation", "Body key");
        var label = NormalizeLabel(request.Label, "governing_bodies.validation", "Label");
        var description = NormalizeDescription(request.Description, "governing_bodies.validation");

        var exists = await db.GoverningBodies.AnyAsync(
            x => x.TenantId == tenantId && x.BodyKey == bodyKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "governing_bodies.duplicate",
                "A governing body with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new GoverningBody
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BodyKey = bodyKey,
            Label = label,
            Description = description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.GoverningBodies.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "governing_body.create",
            tenantId,
            actorUserId,
            "governing_body",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static GoverningBodyResponse MapResponse(GoverningBody entity) =>
        new(
            entity.Id,
            entity.BodyKey,
            entity.Label,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt);

    internal static string NormalizeKey(string key, string errorCode, string fieldName)
    {
        var normalized = key.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                errorCode,
                $"{fieldName} must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    internal static string NormalizeLabel(string label, string errorCode, string fieldName)
    {
        var trimmed = label.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                errorCode,
                $"{fieldName} must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    internal static string NormalizeDescription(string description, string errorCode)
    {
        var trimmed = description.Trim();
        if (trimmed.Length < 4 || trimmed.Length > 1024)
        {
            throw new StlApiException(
                errorCode,
                "Description must be between 4 and 1024 characters.",
                400);
        }

        return trimmed;
    }
}
