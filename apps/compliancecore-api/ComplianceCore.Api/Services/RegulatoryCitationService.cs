using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RegulatoryCitationService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<RegulatoryCitationResponse>> ListAsync(
        Guid tenantId,
        Guid? regulatoryProgramId = null,
        Guid? rulePackId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.RegulatoryCitations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive);

        if (regulatoryProgramId.HasValue)
        {
            query = query.Where(x => x.RegulatoryProgramId == regulatoryProgramId.Value);
        }

        if (rulePackId.HasValue)
        {
            query = query.Where(x => x.RulePackId == rulePackId.Value);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAt)
            .Join(
                db.RegulatoryPrograms.AsNoTracking(),
                citation => citation.RegulatoryProgramId,
                program => program.Id,
                (citation, program) => new { citation, program })
            .GroupJoin(
                db.RulePacks.AsNoTracking(),
                joined => joined.citation.RulePackId,
                pack => pack.Id,
                (joined, packs) => new RegulatoryCitationResponse(
                    joined.citation.Id,
                    joined.citation.RegulatoryProgramId,
                    joined.program.ProgramKey,
                    joined.program.Label,
                    joined.citation.RulePackId,
                    packs.Select(p => p.PackKey).FirstOrDefault(),
                    packs.Select(p => p.Label).FirstOrDefault(),
                    joined.citation.CitationKey,
                    joined.citation.Label,
                    joined.citation.SourceReference,
                    joined.citation.Description,
                    joined.citation.VersionNumber,
                    joined.citation.SupersedesCitationId,
                    joined.citation.IsActive,
                    joined.citation.CreatedAt,
                    joined.citation.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InternalCitationLookupItem>> LookupByIdsAsync(
        Guid tenantId,
        IReadOnlyList<Guid> citationIds,
        CancellationToken cancellationToken = default)
    {
        if (citationIds.Count == 0)
        {
            return Array.Empty<InternalCitationLookupItem>();
        }

        var distinctIds = citationIds.Distinct().ToList();
        return await db.RegulatoryCitations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && distinctIds.Contains(x.Id))
            .Join(
                db.RegulatoryPrograms.AsNoTracking(),
                citation => citation.RegulatoryProgramId,
                program => program.Id,
                (citation, program) => new { citation, program })
            .GroupJoin(
                db.RulePacks.AsNoTracking(),
                joined => joined.citation.RulePackId,
                pack => pack.Id,
                (joined, packs) => new InternalCitationLookupItem(
                    joined.citation.Id,
                    joined.citation.CitationKey,
                    joined.citation.VersionNumber,
                    joined.citation.Label,
                    joined.citation.SourceReference,
                    joined.citation.Description,
                    joined.program.ProgramKey,
                    packs.Select(p => p.PackKey).FirstOrDefault(),
                    joined.citation.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<RegulatoryCitationResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateRegulatoryCitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var citationKey = GoverningBodyService.NormalizeKey(
            request.CitationKey,
            "citations.validation",
            "Citation key");
        var label = GoverningBodyService.NormalizeLabel(request.Label, "citations.validation", "Label");
        var sourceReference = NormalizeSourceReference(request.SourceReference);
        var description = GoverningBodyService.NormalizeDescription(request.Description, "citations.validation");

        var program = await db.RegulatoryPrograms.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.RegulatoryProgramId && x.IsActive,
            cancellationToken);
        if (program is null)
        {
            throw new StlApiException("citations.program_not_found", "Regulatory program was not found.", 404);
        }

        RulePack? rulePack = null;
        if (request.RulePackId.HasValue)
        {
            rulePack = await db.RulePacks.FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.Id == request.RulePackId.Value
                    && x.RegulatoryProgramId == request.RegulatoryProgramId
                    && x.IsActive,
                cancellationToken);
            if (rulePack is null)
            {
                throw new StlApiException("citations.rule_pack_not_found", "Rule pack was not found for this program.", 404);
            }
        }

        RegulatoryCitation? supersedes = null;
        if (request.SupersedesCitationId.HasValue)
        {
            supersedes = await db.RegulatoryCitations.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.SupersedesCitationId.Value && x.IsActive,
                cancellationToken);
            if (supersedes is null)
            {
                throw new StlApiException("citations.supersedes_not_found", "Superseded citation was not found.", 404);
            }
        }

        var latestVersion = await db.RegulatoryCitations
            .Where(x => x.TenantId == tenantId && x.CitationKey == citationKey)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync(cancellationToken) ?? 0;

        var now = DateTimeOffset.UtcNow;
        var entity = new RegulatoryCitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RegulatoryProgramId = request.RegulatoryProgramId,
            RulePackId = request.RulePackId,
            CitationKey = citationKey,
            Label = label,
            SourceReference = sourceReference,
            Description = description,
            VersionNumber = latestVersion + 1,
            SupersedesCitationId = request.SupersedesCitationId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.RegulatoryCitations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "citation.create",
            tenantId,
            actorUserId,
            "citation",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity, program, rulePack);
    }

    private static RegulatoryCitationResponse MapResponse(
        RegulatoryCitation entity,
        RegulatoryProgram program,
        RulePack? rulePack) =>
        new(
            entity.Id,
            entity.RegulatoryProgramId,
            program.ProgramKey,
            program.Label,
            entity.RulePackId,
            rulePack?.PackKey,
            rulePack?.Label,
            entity.CitationKey,
            entity.Label,
            entity.SourceReference,
            entity.Description,
            entity.VersionNumber,
            entity.SupersedesCitationId,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeSourceReference(string sourceReference)
    {
        var trimmed = sourceReference.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 256)
        {
            throw new StlApiException(
                "citations.validation",
                "Source reference must be between 2 and 256 characters.",
                400);
        }

        return trimmed;
    }
}
