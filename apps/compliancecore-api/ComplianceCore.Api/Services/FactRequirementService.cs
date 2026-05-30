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
        string? sourceProduct = null,
        string? sourceEntity = null,
        string? factKey = null,
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

        if (!string.IsNullOrWhiteSpace(sourceProduct))
        {
            query = query.Where(x => x.SourceProduct.ToLower().Contains(sourceProduct.Trim().ToLowerInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(sourceEntity))
        {
            query = query.Where(x => x.SourceEntity.ToLower().Contains(sourceEntity.Trim().ToLowerInvariant()));
        }

        var rows = await (
            from requirement in query.OrderByDescending(x => x.UpdatedAt)
            join definition in db.FactDefinitions.AsNoTracking() on requirement.FactDefinitionId equals definition.Id
            join pack in db.RulePacks.AsNoTracking() on requirement.RulePackId equals pack.Id into packJoin
            from pack in packJoin.DefaultIfEmpty()
            join citation in db.RegulatoryCitations.AsNoTracking() on requirement.CitationId equals citation.Id into citationJoin
            from citation in citationJoin.DefaultIfEmpty()
            where string.IsNullOrWhiteSpace(factKey) || definition.FactKey == factKey.Trim().ToLowerInvariant()
            select new { requirement, definition, pack, citation })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => MapResponse(x.requirement, x.definition, x.pack, x.citation))
            .ToList();
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
        var contract = BuildCreateContract(request, definition, label);
        var validationIssues = FactRequirementContractRules.Validate(contract, strictAuditMetadata: false);
        if (validationIssues.Count > 0)
        {
            throw new StlApiException(
                "fact_requirements.validation",
                string.Join(" ", validationIssues),
                400);
        }

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
            ApplicabilityKey = contract.ApplicabilityKey,
            SourceProduct = FactRequirementContractRules.NormalizeProducts(contract.SourceProduct),
            SourceEntity = contract.SourceEntity,
            SourceFieldOrRecordType = contract.SourceFieldOrRecordType,
            ValueType = contract.ValueType,
            Operator = contract.Operator,
            ExpectedValue = contract.ExpectedValue,
            EvidenceKind = contract.EvidenceKind,
            RequiredDocumentType = contract.RequiredDocumentType,
            RetentionPeriod = contract.RetentionPeriod,
            AuditQuestion = contract.AuditQuestion,
            FailureSeverity = contract.FailureSeverity,
            AutomaticFailureFlag = contract.AutomaticFailureFlag,
            OverrideAllowed = contract.OverrideAllowed,
            OverridePermission = contract.OverridePermission,
            RemediationRequired = contract.RemediationRequired,
            ExternallyAssertable = contract.ExternallyAssertable,
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

        return MapResponse(entity, definition, rulePack, citation);
    }

    internal static FactRequirementResponse MapResponse(
        FactRequirement requirement,
        FactDefinition definition,
        RulePack? pack,
        RegulatoryCitation? citation) =>
        new(
            requirement.Id,
            requirement.FactDefinitionId,
            definition.FactKey,
            definition.Label,
            requirement.RulePackId,
            pack?.PackKey,
            requirement.CitationId,
            citation?.CitationKey,
            requirement.RequirementKey,
            requirement.ApplicabilityKey,
            requirement.SourceProduct,
            requirement.SourceEntity,
            requirement.SourceFieldOrRecordType,
            requirement.ValueType,
            requirement.Operator,
            requirement.ExpectedValue,
            requirement.EvidenceKind,
            requirement.RequiredDocumentType,
            requirement.RetentionPeriod,
            requirement.AuditQuestion,
            requirement.FailureSeverity,
            requirement.AutomaticFailureFlag,
            requirement.OverrideAllowed,
            requirement.OverridePermission,
            requirement.RemediationRequired,
            requirement.ExternallyAssertable,
            requirement.Label,
            requirement.Description,
            requirement.IsRequired,
            requirement.IsActive,
            requirement.CreatedAt,
            requirement.UpdatedAt);

    private static FactRequirementContractInput BuildCreateContract(
        CreateFactRequirementRequest request,
        FactDefinition definition,
        string label)
    {
        var valueType = (request.ValueType ?? definition.ValueType).Trim().ToLowerInvariant();
        var evidenceKind = (request.EvidenceKind ?? FactRequirementEvidenceKinds.SystemFact).Trim().ToLowerInvariant();
        var operatorValue = (request.Operator ?? FactRequirementOperators.Equal).Trim().ToLowerInvariant();
        var expectedValue = request.ExpectedValue
            ?? (string.Equals(valueType, FactValueTypes.Boolean, StringComparison.OrdinalIgnoreCase) ? "true" : string.Empty);

        return new FactRequirementContractInput(
            request.RequirementKey.Trim().ToLowerInvariant(),
            definition.FactKey,
            (request.ApplicabilityKey ?? "default").Trim(),
            (request.SourceProduct ?? ComplianceCoreProductKeys.ComplianceCore).Trim(),
            (request.SourceEntity ?? "fact_requirement").Trim(),
            (request.SourceFieldOrRecordType ?? "manual_catalog_requirement").Trim(),
            valueType,
            operatorValue,
            expectedValue.Trim(),
            evidenceKind,
            (request.RequiredDocumentType ?? string.Empty).Trim(),
            (request.RetentionPeriod ?? "per_citation_or_company_policy").Trim(),
            (request.AuditQuestion ?? $"Is {label.ToLowerInvariant()}?").Trim(),
            (request.FailureSeverity ?? FactRequirementFailureSeverities.Major).Trim().ToLowerInvariant(),
            request.AutomaticFailureFlag ?? false,
            request.OverrideAllowed ?? true,
            (request.OverridePermission ?? "compliance.override.fact_requirement").Trim(),
            request.RemediationRequired ?? true,
            request.IsRequired,
            request.ExternallyAssertable);
    }
}
