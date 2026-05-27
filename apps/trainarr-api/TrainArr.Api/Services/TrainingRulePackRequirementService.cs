using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingRulePackRequirementService(
    TrainArrDbContext db,
    ComplianceCoreRulePackClient rulePackClient,
    ITrainArrAuditService audit)
{
    public async Task<IReadOnlyList<TrainingRulePackRequirementResponse>> ListAsync(
        Guid tenantId,
        string entityType,
        Guid entityId,
        bool includeMetadata,
        CancellationToken cancellationToken = default)
    {
        ValidateEntityType(entityType);
        await EnsureEntityExistsAsync(tenantId, entityType, entityId, cancellationToken);

        var requirements = await db.TrainingRulePackRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == entityType && x.EntityId == entityId)
            .OrderBy(x => x.RulePackKey)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return await MapResponsesAsync(tenantId, requirements, includeMetadata, cancellationToken);
    }

    public async Task<TrainingRulePackRequirementResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        string entityType,
        Guid entityId,
        UpsertTrainingRulePackRequirementRequest request,
        bool validateWithComplianceCore,
        CancellationToken cancellationToken = default)
    {
        ValidateEntityType(entityType);
        await EnsureEntityExistsAsync(tenantId, entityType, entityId, cancellationToken);

        var rulePackKey = NormalizeRulePackKey(request.RulePackKey);

        ComplianceCoreRulePackLookupItem? validatedPack = null;
        if (validateWithComplianceCore)
        {
            var lookup = await rulePackClient.LookupAsync(
                new ComplianceCoreRulePackLookupPayload(tenantId, [rulePackKey]),
                cancellationToken);
            validatedPack = lookup.FirstOrDefault(x =>
                string.Equals(x.RulePackKey, rulePackKey, StringComparison.OrdinalIgnoreCase));
            if (validatedPack is null)
            {
                throw new StlApiException(
                    "rule_pack_requirements.not_found",
                    "Rule pack was not found in Compliance Core for this tenant.",
                    404);
            }

            if (!validatedPack.IsActive)
            {
                throw new StlApiException(
                    "rule_pack_requirements.inactive",
                    "Rule pack is not active in Compliance Core.",
                    400);
            }
        }

        var existing = await db.TrainingRulePackRequirements.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.EntityType == entityType
                && x.EntityId == entityId
                && x.RulePackKey == rulePackKey,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        TrainingRulePackRequirement entity;
        var isCreate = existing is null;

        if (existing is null)
        {
            entity = new TrainingRulePackRequirement
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EntityType = entityType,
                EntityId = entityId,
                RulePackKey = rulePackKey,
                AttachedByUserId = actorUserId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.TrainingRulePackRequirements.Add(entity);
        }
        else
        {
            entity = existing;
            entity.AttachedByUserId = actorUserId;
            entity.UpdatedAt = now;
        }

        if (validatedPack is not null)
        {
            entity.KnownVersionNumber = validatedPack.VersionNumber;
            entity.KnownStatus = validatedPack.Status.Trim().ToLowerInvariant();
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            isCreate ? "rule_pack_requirement.create" : "rule_pack_requirement.update",
            tenantId,
            actorUserId,
            entityType,
            entityId.ToString(),
            rulePackKey,
            cancellationToken: cancellationToken);

        var responses = await MapResponsesAsync(tenantId, [entity], includeMetadata: true, cancellationToken);
        return responses[0];
    }

    public async Task RemoveAsync(
        Guid tenantId,
        Guid? actorUserId,
        string entityType,
        Guid entityId,
        Guid requirementId,
        CancellationToken cancellationToken = default)
    {
        ValidateEntityType(entityType);
        await EnsureEntityExistsAsync(tenantId, entityType, entityId, cancellationToken);

        var requirement = await db.TrainingRulePackRequirements.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.Id == requirementId
                && x.EntityType == entityType
                && x.EntityId == entityId,
            cancellationToken);
        if (requirement is null)
        {
            throw new StlApiException(
                "rule_pack_requirements.not_found",
                "Rule pack requirement was not found.",
                404);
        }

        db.TrainingRulePackRequirements.Remove(requirement);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "rule_pack_requirement.remove",
            tenantId,
            actorUserId,
            entityType,
            entityId.ToString(),
            requirement.RulePackKey,
            cancellationToken: cancellationToken);
    }

    public async Task<string?> ResolveRulePackKeyAsync(
        Guid tenantId,
        Guid? trainingDefinitionId,
        Guid? trainingProgramId,
        string? qualificationKey,
        CancellationToken cancellationToken = default)
    {
        if (trainingDefinitionId is { } definitionId && definitionId != Guid.Empty)
        {
            var fromDefinition = await ResolveFirstRequirementKeyAsync(
                tenantId,
                TrainingRulePackRequirementEntityTypes.TrainingDefinition,
                definitionId,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(fromDefinition))
            {
                return fromDefinition;
            }
        }

        if (trainingProgramId is { } programId && programId != Guid.Empty)
        {
            var fromProgram = await ResolveFirstRequirementKeyAsync(
                tenantId,
                TrainingRulePackRequirementEntityTypes.TrainingProgram,
                programId,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(fromProgram))
            {
                return fromProgram;
            }
        }

        if (!string.IsNullOrWhiteSpace(qualificationKey))
        {
            var normalizedQualificationKey = qualificationKey.Trim().ToLowerInvariant();
            var definition = await db.TrainingDefinitions
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.QualificationKey == normalizedQualificationKey)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (definition != Guid.Empty)
            {
                return await ResolveFirstRequirementKeyAsync(
                    tenantId,
                    TrainingRulePackRequirementEntityTypes.TrainingDefinition,
                    definition,
                    cancellationToken);
            }
        }

        return null;
    }

    private async Task<string?> ResolveFirstRequirementKeyAsync(
        Guid tenantId,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        return await db.TrainingRulePackRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == entityType && x.EntityId == entityId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => x.RulePackKey)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<TrainingRulePackRequirementResponse>> MapResponsesAsync(
        Guid tenantId,
        IReadOnlyList<TrainingRulePackRequirement> requirements,
        bool includeMetadata,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, ComplianceCoreRulePackLookupItem> metadataByKey =
            new Dictionary<string, ComplianceCoreRulePackLookupItem>(StringComparer.OrdinalIgnoreCase);
        if (includeMetadata && requirements.Count > 0)
        {
            try
            {
                var lookup = await rulePackClient.LookupAsync(
                    new ComplianceCoreRulePackLookupPayload(
                        tenantId,
                        requirements.Select(x => x.RulePackKey).Distinct(StringComparer.OrdinalIgnoreCase).ToList()),
                    cancellationToken);
                metadataByKey = lookup.ToDictionary(x => x.RulePackKey, StringComparer.OrdinalIgnoreCase);
            }
            catch (StlApiException)
            {
                metadataByKey = new Dictionary<string, ComplianceCoreRulePackLookupItem>(StringComparer.OrdinalIgnoreCase);
            }
        }

        return requirements
            .Select(requirement =>
            {
                TrainingRulePackMetadataResponse? metadata = null;
                if (metadataByKey.TryGetValue(requirement.RulePackKey, out var item))
                {
                    metadata = new TrainingRulePackMetadataResponse(
                        item.Label,
                        item.Description,
                        item.RegulatoryProgramKey,
                        item.RegulatoryProgramLabel,
                        item.VersionNumber,
                        item.Status,
                        item.IsActive);
                }

                return new TrainingRulePackRequirementResponse(
                    requirement.Id,
                    requirement.EntityType,
                    requirement.EntityId,
                    requirement.RulePackKey,
                    requirement.CreatedAt,
                    requirement.UpdatedAt,
                    metadata);
            })
            .ToList();
    }

    private async Task EnsureEntityExistsAsync(
        Guid tenantId,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var exists = entityType switch
        {
            TrainingRulePackRequirementEntityTypes.TrainingDefinition => await db.TrainingDefinitions.AnyAsync(
                x => x.TenantId == tenantId && x.Id == entityId,
                cancellationToken),
            TrainingRulePackRequirementEntityTypes.TrainingProgram => await db.TrainingPrograms.AnyAsync(
                x => x.TenantId == tenantId && x.Id == entityId,
                cancellationToken),
            _ => false,
        };

        if (!exists)
        {
            throw new StlApiException(
                "rule_pack_requirements.entity_not_found",
                "Training entity was not found for rule pack requirement.",
                404);
        }
    }

    private static void ValidateEntityType(string entityType)
    {
        if (!TrainingRulePackRequirementEntityTypeExtensions.IsSupported(entityType))
        {
            throw new StlApiException(
                "rule_pack_requirements.entity_type_invalid",
                "Entity type must be training_definition or training_program.",
                400);
        }
    }

    private static string NormalizeRulePackKey(string rulePackKey)
    {
        var trimmed = rulePackKey.Trim().ToLowerInvariant();
        if (trimmed.Length < 2 || trimmed.Length > 64)
        {
            throw new StlApiException(
                "rule_pack_requirements.validation",
                "Rule pack key must be between 2 and 64 characters.",
                400);
        }

        return trimmed;
    }
}

public static class TrainingRulePackRequirementEntityTypeExtensions
{
    public static bool IsSupported(string entityType) =>
        string.Equals(entityType, TrainingRulePackRequirementEntityTypes.TrainingDefinition, StringComparison.Ordinal)
        || string.Equals(entityType, TrainingRulePackRequirementEntityTypes.TrainingProgram, StringComparison.Ordinal);
}
