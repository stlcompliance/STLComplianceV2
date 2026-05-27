using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RegulatoryMappingService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    private static readonly HashSet<string> AllowedTargetKinds =
        new(StringComparer.OrdinalIgnoreCase) { "compliance_key", "material_key" };

    public async Task<IReadOnlyList<RegulatoryMappingResponse>> ListAsync(
        Guid tenantId,
        Guid? regulatoryProgramId = null,
        Guid? rulePackId = null,
        Guid? citationId = null,
        Guid? complianceKeyId = null,
        Guid? materialKeyId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.RegulatoryMappings
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

        if (citationId.HasValue)
        {
            query = query.Where(x => x.CitationId == citationId.Value);
        }

        if (complianceKeyId.HasValue)
        {
            query = query.Where(x => x.ComplianceKeyId == complianceKeyId.Value);
        }

        if (materialKeyId.HasValue)
        {
            query = query.Where(x => x.MaterialKeyId == materialKeyId.Value);
        }

        return await (
            from mapping in query.OrderByDescending(x => x.UpdatedAt)
            join program in db.RegulatoryPrograms.AsNoTracking() on mapping.RegulatoryProgramId equals program.Id
            join pack in db.RulePacks.AsNoTracking() on mapping.RulePackId equals pack.Id into packJoin
            from pack in packJoin.DefaultIfEmpty()
            join citation in db.RegulatoryCitations.AsNoTracking() on mapping.CitationId equals citation.Id into citationJoin
            from citation in citationJoin.DefaultIfEmpty()
            join fact in db.FactDefinitions.AsNoTracking() on mapping.FactDefinitionId equals fact.Id into factJoin
            from fact in factJoin.DefaultIfEmpty()
            join complianceKey in db.ComplianceKeys.AsNoTracking() on mapping.ComplianceKeyId equals complianceKey.Id into complianceJoin
            from complianceKey in complianceJoin.DefaultIfEmpty()
            join materialKey in db.MaterialKeys.AsNoTracking() on mapping.MaterialKeyId equals materialKey.Id into materialJoin
            from materialKey in materialJoin.DefaultIfEmpty()
            select new RegulatoryMappingResponse(
                mapping.Id,
                mapping.MappingKey,
                mapping.Label,
                mapping.Description,
                mapping.TargetKind,
                mapping.RegulatoryProgramId,
                program.ProgramKey,
                program.Label,
                mapping.RulePackId,
                pack != null ? pack.PackKey : null,
                pack != null ? pack.Label : null,
                mapping.CitationId,
                citation != null ? citation.CitationKey : null,
                mapping.FactDefinitionId,
                fact != null ? fact.FactKey : null,
                mapping.ComplianceKeyId,
                complianceKey != null ? complianceKey.Key : null,
                mapping.MaterialKeyId,
                materialKey != null ? materialKey.Key : null,
                mapping.IsActive,
                mapping.CreatedAt,
                mapping.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<RegulatoryMappingResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateRegulatoryMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        var mappingKey = GoverningBodyService.NormalizeKey(
            request.MappingKey,
            "regulatory_mappings.validation",
            "Mapping key");
        var label = GoverningBodyService.NormalizeLabel(request.Label, "regulatory_mappings.validation", "Label");
        var description = GoverningBodyService.NormalizeDescription(request.Description, "regulatory_mappings.validation");
        var targetKind = NormalizeTargetKind(request.TargetKind);

        ValidateTargetKindMatchesKeys(targetKind, request.ComplianceKeyId, request.MaterialKeyId);

        var program = await db.RegulatoryPrograms.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.RegulatoryProgramId && x.IsActive,
            cancellationToken);
        if (program is null)
        {
            throw new StlApiException(
                "regulatory_mappings.program_not_found",
                "Regulatory program was not found.",
                404);
        }

        RulePack? rulePack = null;
        if (request.RulePackId.HasValue)
        {
            rulePack = await db.RulePacks.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.RulePackId.Value && x.IsActive,
                cancellationToken);
            if (rulePack is null)
            {
                throw new StlApiException("regulatory_mappings.rule_pack_not_found", "Rule pack was not found.", 404);
            }

            if (rulePack.RegulatoryProgramId != program.Id)
            {
                throw new StlApiException(
                    "regulatory_mappings.rule_pack_program_mismatch",
                    "Rule pack must belong to the selected regulatory program.",
                    400);
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
                throw new StlApiException("regulatory_mappings.citation_not_found", "Citation was not found.", 404);
            }

            if (citation.RegulatoryProgramId != program.Id)
            {
                throw new StlApiException(
                    "regulatory_mappings.citation_program_mismatch",
                    "Citation must belong to the selected regulatory program.",
                    400);
            }
        }

        FactDefinition? factDefinition = null;
        if (request.FactDefinitionId.HasValue)
        {
            factDefinition = await db.FactDefinitions.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.FactDefinitionId.Value && x.IsActive,
                cancellationToken);
            if (factDefinition is null)
            {
                throw new StlApiException(
                    "regulatory_mappings.fact_definition_not_found",
                    "Fact definition was not found.",
                    404);
            }
        }

        ComplianceKey? complianceKey = null;
        if (request.ComplianceKeyId.HasValue)
        {
            complianceKey = await db.ComplianceKeys.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.ComplianceKeyId.Value && x.IsActive,
                cancellationToken);
            if (complianceKey is null)
            {
                throw new StlApiException(
                    "regulatory_mappings.compliance_key_not_found",
                    "Compliance key was not found.",
                    404);
            }
        }

        MaterialKey? materialKey = null;
        if (request.MaterialKeyId.HasValue)
        {
            materialKey = await db.MaterialKeys.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.MaterialKeyId.Value && x.IsActive,
                cancellationToken);
            if (materialKey is null)
            {
                throw new StlApiException(
                    "regulatory_mappings.material_key_not_found",
                    "Material key was not found.",
                    404);
            }
        }

        var exists = await db.RegulatoryMappings.AnyAsync(
            x => x.TenantId == tenantId && x.MappingKey == mappingKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "regulatory_mappings.duplicate",
                "A regulatory mapping with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new RegulatoryMapping
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MappingKey = mappingKey,
            Label = label,
            Description = description,
            TargetKind = targetKind,
            RegulatoryProgramId = request.RegulatoryProgramId,
            RulePackId = request.RulePackId,
            CitationId = request.CitationId,
            FactDefinitionId = request.FactDefinitionId,
            ComplianceKeyId = request.ComplianceKeyId,
            MaterialKeyId = request.MaterialKeyId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.RegulatoryMappings.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "regulatory_mapping.create",
            tenantId,
            actorUserId,
            "regulatory_mapping",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return new RegulatoryMappingResponse(
            entity.Id,
            entity.MappingKey,
            entity.Label,
            entity.Description,
            entity.TargetKind,
            entity.RegulatoryProgramId,
            program.ProgramKey,
            program.Label,
            entity.RulePackId,
            rulePack?.PackKey,
            rulePack?.Label,
            entity.CitationId,
            citation?.CitationKey,
            entity.FactDefinitionId,
            factDefinition?.FactKey,
            entity.ComplianceKeyId,
            complianceKey?.Key,
            entity.MaterialKeyId,
            materialKey?.Key,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static string NormalizeTargetKind(string targetKind)
    {
        var normalized = targetKind.Trim().ToLowerInvariant();
        if (!AllowedTargetKinds.Contains(normalized))
        {
            throw new StlApiException(
                "regulatory_mappings.validation",
                "Target kind must be compliance_key or material_key.",
                400);
        }

        return normalized;
    }

    private static void ValidateTargetKindMatchesKeys(
        string targetKind,
        Guid? complianceKeyId,
        Guid? materialKeyId)
    {
        var hasCompliance = complianceKeyId.HasValue;
        var hasMaterial = materialKeyId.HasValue;

        if (hasCompliance && hasMaterial)
        {
            throw new StlApiException(
                "regulatory_mappings.validation",
                "A mapping must target either a compliance key or a material key, not both.",
                400);
        }

        if (!hasCompliance && !hasMaterial)
        {
            throw new StlApiException(
                "regulatory_mappings.validation",
                "A mapping must target a compliance key or a material key.",
                400);
        }

        if (targetKind == "compliance_key" && !hasCompliance)
        {
            throw new StlApiException(
                "regulatory_mappings.validation",
                "Target kind compliance_key requires a compliance key reference.",
                400);
        }

        if (targetKind == "material_key" && !hasMaterial)
        {
            throw new StlApiException(
                "regulatory_mappings.validation",
                "Target kind material_key requires a material key reference.",
                400);
        }
    }
}
