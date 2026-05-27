using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class JurisdictionService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<JurisdictionResponse>> ListAsync(
        Guid tenantId,
        Guid? governingBodyId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Jurisdictions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive);

        if (governingBodyId.HasValue)
        {
            query = query.Where(x => x.GoverningBodyId == governingBodyId.Value);
        }

        return await query
            .OrderBy(x => x.Label)
            .Join(
                db.GoverningBodies.AsNoTracking(),
                jurisdiction => jurisdiction.GoverningBodyId,
                body => body.Id,
                (jurisdiction, body) => new JurisdictionResponse(
                    jurisdiction.Id,
                    jurisdiction.GoverningBodyId,
                    body.BodyKey,
                    body.Label,
                    jurisdiction.JurisdictionKey,
                    jurisdiction.Label,
                    jurisdiction.Description,
                    jurisdiction.IsActive,
                    jurisdiction.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<JurisdictionResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateJurisdictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var jurisdictionKey = GoverningBodyService.NormalizeKey(
            request.JurisdictionKey,
            "jurisdictions.validation",
            "Jurisdiction key");
        var label = GoverningBodyService.NormalizeLabel(request.Label, "jurisdictions.validation", "Label");
        var description = GoverningBodyService.NormalizeDescription(request.Description, "jurisdictions.validation");

        var governingBody = await db.GoverningBodies.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.GoverningBodyId && x.IsActive,
            cancellationToken);
        if (governingBody is null)
        {
            throw new StlApiException("jurisdictions.governing_body_not_found", "Governing body was not found.", 404);
        }

        var exists = await db.Jurisdictions.AnyAsync(
            x => x.TenantId == tenantId && x.JurisdictionKey == jurisdictionKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "jurisdictions.duplicate",
                "A jurisdiction with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new Jurisdiction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            GoverningBodyId = request.GoverningBodyId,
            JurisdictionKey = jurisdictionKey,
            Label = label,
            Description = description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Jurisdictions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "jurisdiction.create",
            tenantId,
            actorUserId,
            "jurisdiction",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return new JurisdictionResponse(
            entity.Id,
            entity.GoverningBodyId,
            governingBody.BodyKey,
            governingBody.Label,
            entity.JurisdictionKey,
            entity.Label,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt);
    }
}
