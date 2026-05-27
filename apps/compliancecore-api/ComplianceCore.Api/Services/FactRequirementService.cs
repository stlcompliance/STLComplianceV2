using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class FactRequirementService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<FactRequirementResponse>> ListAsync(
        Guid tenantId,
        Guid? rulePackId = null,
        Guid? citationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.FactRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive);

        if (rulePackId.HasValue)
        {
            query = query.Where(x => x.RulePackId == rulePackId.Value);
        }

        if (citationId.HasValue)
        {
            query = query.Where(x => x.CitationId == citationId.Value);
        }

        return await (
            from requirement in query.OrderByDescending(x => x.UpdatedAt)
            join definition in db.FactDefinitions.AsNoTracking() on requirement.FactDefinitionId equals definition.Id
            join pack in db.RulePacks.AsNoTracking() on requirement.RulePackId equals pack.Id into packJoin
            from pack in packJoin.DefaultIfEmpty()
            join citation in db.RegulatoryCitations.AsNoTracking() on requirement.CitationId equals citation.Id into citationJoin
            from citation in citationJoin.DefaultIfEmpty()
            select new FactRequirementResponse(
                requirement.Id,
                requirement.FactDefinitionId,
                definition.FactKey,
                definition.Label,
                requirement.RulePackId,
                pack != null ? pack.PackKey : null,
                requirement.CitationId,
                citation != null ? citation.CitationKey : null,
                requirement.RequirementKey,
                requirement.Label,
                requirement.Description,
                requirement.IsRequired,
                requirement.IsActive,
                requirement.CreatedAt,
                requirement.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<FactRequirementResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateFactRequirementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.RulePackId.HasValue && !request.CitationId.HasValue)
        {
            throw new StlApiException(
                "fact_requirements.validation",
                "A fact requirement must be linked to a rule pack or a citation.",
                400);
        }

        var requirementKey = GoverningBodyService.NormalizeKey(
            request.RequirementKey,
            "fact_requirements.validation",
            "Requirement key");
        var label = GoverningBodyService.NormalizeLabel(request.Label, "fact_requirements.validation", "Label");
        var description = GoverningBodyService.NormalizeDescription(request.Description, "fact_requirements.validation");

        var definition = await db.FactDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.FactDefinitionId && x.IsActive,
            cancellationToken);
        if (definition is null)
        {
            throw new StlApiException("fact_requirements.definition_not_found", "Fact definition was not found.", 404);
        }

        RulePack? rulePack = null;
        if (request.RulePackId.HasValue)
        {
            rulePack = await db.RulePacks.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.RulePackId.Value && x.IsActive,
                cancellationToken);
            if (rulePack is null)
            {
                throw new StlApiException("fact_requirements.rule_pack_not_found", "Rule pack was not found.", 404);
            }
        }

        RegulatoryCitation? citation = null;
        if (request.CitationId.HasValue)
        {
            citation = await db.RegulatoryCitations.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.CitationId.Value && x.IsActive,
                cancellationToken);
            if (citation is null)
            {
                throw new StlApiException("fact_requirements.citation_not_found", "Citation was not found.", 404);
            }
        }

        var exists = await db.FactRequirements.AnyAsync(
            x => x.TenantId == tenantId && x.RequirementKey == requirementKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "fact_requirements.duplicate",
                "A fact requirement with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new FactRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FactDefinitionId = request.FactDefinitionId,
            RulePackId = request.RulePackId,
            CitationId = request.CitationId,
            RequirementKey = requirementKey,
            Label = label,
            Description = description,
            IsRequired = request.IsRequired,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.FactRequirements.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "fact_requirement.create",
            tenantId,
            actorUserId,
            "fact_requirement",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return new FactRequirementResponse(
            entity.Id,
            entity.FactDefinitionId,
            definition.FactKey,
            definition.Label,
            entity.RulePackId,
            rulePack?.PackKey,
            entity.CitationId,
            citation?.CitationKey,
            entity.RequirementKey,
            entity.Label,
            entity.Description,
            entity.IsRequired,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
