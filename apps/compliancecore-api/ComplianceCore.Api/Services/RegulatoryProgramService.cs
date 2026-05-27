using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RegulatoryProgramService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<RegulatoryProgramResponse>> ListAsync(
        Guid tenantId,
        Guid? jurisdictionId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.RegulatoryPrograms
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive);

        if (jurisdictionId.HasValue)
        {
            query = query.Where(x => x.JurisdictionId == jurisdictionId.Value);
        }

        return await query
            .OrderBy(x => x.Label)
            .Join(
                db.Jurisdictions.AsNoTracking(),
                program => program.JurisdictionId,
                jurisdiction => jurisdiction.Id,
                (program, jurisdiction) => new RegulatoryProgramResponse(
                    program.Id,
                    program.JurisdictionId,
                    jurisdiction.JurisdictionKey,
                    jurisdiction.Label,
                    program.ProgramKey,
                    program.Label,
                    program.Description,
                    program.IsActive,
                    program.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<RegulatoryProgramResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateRegulatoryProgramRequest request,
        CancellationToken cancellationToken = default)
    {
        var programKey = GoverningBodyService.NormalizeKey(
            request.ProgramKey,
            "regulatory_programs.validation",
            "Program key");
        var label = GoverningBodyService.NormalizeLabel(request.Label, "regulatory_programs.validation", "Label");
        var description = GoverningBodyService.NormalizeDescription(
            request.Description,
            "regulatory_programs.validation");

        var jurisdiction = await db.Jurisdictions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.JurisdictionId && x.IsActive,
            cancellationToken);
        if (jurisdiction is null)
        {
            throw new StlApiException("regulatory_programs.jurisdiction_not_found", "Jurisdiction was not found.", 404);
        }

        var exists = await db.RegulatoryPrograms.AnyAsync(
            x => x.TenantId == tenantId && x.ProgramKey == programKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "regulatory_programs.duplicate",
                "A regulatory program with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new RegulatoryProgram
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            JurisdictionId = request.JurisdictionId,
            ProgramKey = programKey,
            Label = label,
            Description = description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.RegulatoryPrograms.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "regulatory_program.create",
            tenantId,
            actorUserId,
            "regulatory_program",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return new RegulatoryProgramResponse(
            entity.Id,
            entity.JurisdictionId,
            jurisdiction.JurisdictionKey,
            jurisdiction.Label,
            entity.ProgramKey,
            entity.Label,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt);
    }
}
