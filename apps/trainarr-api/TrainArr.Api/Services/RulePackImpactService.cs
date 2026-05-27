using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class RulePackImpactService(
    TrainArrDbContext db,
    ComplianceCoreRulePackClient rulePackClient,
    ITrainArrAuditService audit)
{
    private static readonly HashSet<string> ActiveQualificationStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "issued",
        "suspended",
    };

    public async Task<RulePackImpactAssessmentResponse> AssessAsync(
        Guid tenantId,
        Guid? actorUserId,
        string rulePackKey,
        int? expectedVersionNumber,
        string? expectedStatus,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeRulePackKey(rulePackKey);
        var assessmentId = Guid.NewGuid();
        var assessedAt = DateTimeOffset.UtcNow;

        var requirements = await db.TrainingRulePackRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RulePackKey == normalizedKey)
            .OrderBy(x => x.EntityType)
            .ThenBy(x => x.EntityId)
            .ToListAsync(cancellationToken);

        ComplianceCoreRulePackLookupItem? currentPack = null;
        var packNotFound = false;
        try
        {
            var lookup = await rulePackClient.LookupAsync(
                new ComplianceCoreRulePackLookupPayload(tenantId, [normalizedKey]),
                cancellationToken);
            currentPack = lookup
                .Where(x => string.Equals(x.RulePackKey, normalizedKey, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefault();
            packNotFound = currentPack is null;
        }
        catch (StlApiException)
        {
            packNotFound = true;
        }

        var baselineVersion = ResolveBaselineVersion(requirements, expectedVersionNumber);
        var baselineStatus = ResolveBaselineStatus(requirements, expectedStatus);

        var triggers = BuildTriggers(
            packNotFound,
            currentPack,
            baselineVersion,
            baselineStatus);

        var drift = BuildDrift(
            packNotFound,
            currentPack,
            baselineVersion,
            baselineStatus);

        var affectedDefinitions = await BuildAffectedDefinitionsAsync(
            tenantId,
            requirements,
            cancellationToken);

        var affectedPrograms = await BuildAffectedProgramsAsync(
            tenantId,
            requirements,
            cancellationToken);

        var definitionIds = affectedDefinitions
            .Select(x => x.TrainingDefinitionId)
            .Concat(affectedPrograms.SelectMany(x => x.MemberDefinitionIds))
            .Distinct()
            .ToList();

        var affectedAssignments = await BuildAffectedAssignmentsAsync(
            tenantId,
            definitionIds,
            cancellationToken);

        var affectedQualifications = await BuildAffectedQualificationsAsync(
            tenantId,
            definitionIds,
            affectedDefinitions.Select(x => x.QualificationKey).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            cancellationToken);

        var recommendedActions = BuildRecommendedActions(
            triggers,
            affectedDefinitions,
            affectedPrograms,
            affectedAssignments,
            affectedQualifications,
            drift);

        var summary = new RulePackImpactSummary(
            requirements.Count,
            affectedDefinitions.Count,
            affectedPrograms.Count,
            affectedAssignments.Count,
            affectedQualifications.Count,
            drift?.HasVersionDrift == true || drift?.HasStatusDrift == true || drift?.PackInactive == true || drift?.PackNotFound == true,
            triggers.Count > 0 || affectedAssignments.Count > 0 || affectedQualifications.Count > 0);

        var currentState = currentPack is null
            ? null
            : new RulePackImpactCurrentStateResponse(
                currentPack.Label,
                currentPack.Description,
                currentPack.RegulatoryProgramKey,
                currentPack.RegulatoryProgramLabel,
                currentPack.VersionNumber,
                currentPack.Status,
                currentPack.IsActive);

        await audit.WriteAsync(
            "rule_pack_impact.assess",
            tenantId,
            actorUserId,
            "rule_pack",
            normalizedKey,
            summary.RequiresAttention ? "attention_required" : "reviewed",
            triggers.Count > 0 ? string.Join(",", triggers) : null,
            cancellationToken);

        return new RulePackImpactAssessmentResponse(
            assessmentId,
            normalizedKey,
            assessedAt,
            triggers,
            currentState,
            drift,
            affectedDefinitions,
            affectedPrograms,
            affectedAssignments,
            affectedQualifications,
            recommendedActions,
            summary);
    }

    private static string NormalizeRulePackKey(string rulePackKey)
    {
        var trimmed = rulePackKey.Trim().ToLowerInvariant();
        if (trimmed.Length < 2 || trimmed.Length > 64)
        {
            throw new StlApiException(
                "rule_pack_impact.validation",
                "Rule pack key must be between 2 and 64 characters.",
                400);
        }

        return trimmed;
    }

    private static int? ResolveBaselineVersion(
        IReadOnlyList<TrainingRulePackRequirement> requirements,
        int? expectedVersionNumber)
    {
        if (expectedVersionNumber is > 0)
        {
            return expectedVersionNumber;
        }

        return requirements
            .Where(x => x.KnownVersionNumber is > 0)
            .Select(x => x.KnownVersionNumber)
            .OrderByDescending(x => x)
            .FirstOrDefault();
    }

    private static string? ResolveBaselineStatus(
        IReadOnlyList<TrainingRulePackRequirement> requirements,
        string? expectedStatus)
    {
        if (!string.IsNullOrWhiteSpace(expectedStatus))
        {
            return expectedStatus.Trim().ToLowerInvariant();
        }

        return requirements
            .Select(x => x.KnownStatus)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim().ToLowerInvariant())
            .FirstOrDefault();
    }

    private static IReadOnlyList<string> BuildTriggers(
        bool packNotFound,
        ComplianceCoreRulePackLookupItem? currentPack,
        int? baselineVersion,
        string? baselineStatus)
    {
        var triggers = new List<string>();

        if (packNotFound)
        {
            triggers.Add(RulePackImpactTriggers.PackNotFound);
            return triggers;
        }

        if (currentPack is { IsActive: false })
        {
            triggers.Add(RulePackImpactTriggers.PackInactive);
        }

        if (baselineVersion is > 0 && currentPack!.VersionNumber > baselineVersion)
        {
            triggers.Add(RulePackImpactTriggers.VersionDrift);
        }

        if (!string.IsNullOrWhiteSpace(baselineStatus)
            && !string.Equals(currentPack!.Status, baselineStatus, StringComparison.OrdinalIgnoreCase))
        {
            triggers.Add(RulePackImpactTriggers.StatusChange);
        }

        if (triggers.Count == 0)
        {
            triggers.Add(RulePackImpactTriggers.ManualAssessment);
        }

        return triggers;
    }

    private static RulePackImpactDriftResponse? BuildDrift(
        bool packNotFound,
        ComplianceCoreRulePackLookupItem? currentPack,
        int? baselineVersion,
        string? baselineStatus)
    {
        if (packNotFound)
        {
            return new RulePackImpactDriftResponse(
                HasVersionDrift: false,
                BaselineVersionNumber: baselineVersion,
                CurrentVersionNumber: null,
                HasStatusDrift: false,
                BaselineStatus: baselineStatus,
                CurrentStatus: null,
                PackInactive: false,
                PackNotFound: true);
        }

        var hasVersionDrift = baselineVersion is > 0 && currentPack!.VersionNumber > baselineVersion;
        var hasStatusDrift = !string.IsNullOrWhiteSpace(baselineStatus)
            && !string.Equals(currentPack!.Status, baselineStatus, StringComparison.OrdinalIgnoreCase);

        return new RulePackImpactDriftResponse(
            hasVersionDrift,
            baselineVersion,
            currentPack!.VersionNumber,
            hasStatusDrift,
            baselineStatus,
            currentPack.Status,
            !currentPack.IsActive,
            false);
    }

    private async Task<IReadOnlyList<RulePackImpactAffectedDefinition>> BuildAffectedDefinitionsAsync(
        Guid tenantId,
        IReadOnlyList<TrainingRulePackRequirement> requirements,
        CancellationToken cancellationToken)
    {
        var definitionRequirements = requirements
            .Where(x => string.Equals(
                x.EntityType,
                TrainingRulePackRequirementEntityTypes.TrainingDefinition,
                StringComparison.Ordinal))
            .ToList();

        if (definitionRequirements.Count == 0)
        {
            return Array.Empty<RulePackImpactAffectedDefinition>();
        }

        var definitionIds = definitionRequirements.Select(x => x.EntityId).Distinct().ToList();
        var definitions = await db.TrainingDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && definitionIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return definitionRequirements
            .Where(x => definitions.ContainsKey(x.EntityId))
            .Select(requirement =>
            {
                var definition = definitions[requirement.EntityId];
                return new RulePackImpactAffectedDefinition(
                    definition.Id,
                    definition.DefinitionKey,
                    definition.Name,
                    definition.QualificationKey,
                    requirement.Id,
                    requirement.KnownVersionNumber,
                    requirement.KnownStatus);
            })
            .DistinctBy(x => x.TrainingDefinitionId)
            .OrderBy(x => x.DefinitionKey)
            .ToList();
    }

    private async Task<IReadOnlyList<RulePackImpactAffectedProgram>> BuildAffectedProgramsAsync(
        Guid tenantId,
        IReadOnlyList<TrainingRulePackRequirement> requirements,
        CancellationToken cancellationToken)
    {
        var programRequirements = requirements
            .Where(x => string.Equals(
                x.EntityType,
                TrainingRulePackRequirementEntityTypes.TrainingProgram,
                StringComparison.Ordinal))
            .ToList();

        if (programRequirements.Count == 0)
        {
            return Array.Empty<RulePackImpactAffectedProgram>();
        }

        var programIds = programRequirements.Select(x => x.EntityId).Distinct().ToList();
        var programs = await db.TrainingPrograms
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && programIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var memberDefinitions = await db.TrainingProgramDefinitions
            .AsNoTracking()
            .Where(x => programIds.Contains(x.TrainingProgramId))
            .GroupBy(x => x.TrainingProgramId)
            .ToDictionaryAsync(
                x => x.Key,
                x => (IReadOnlyList<Guid>)x.Select(item => item.TrainingDefinitionId).Distinct().ToList(),
                cancellationToken);

        return programRequirements
            .Where(x => programs.ContainsKey(x.EntityId))
            .Select(requirement =>
            {
                var program = programs[requirement.EntityId];
                memberDefinitions.TryGetValue(program.Id, out var members);
                return new RulePackImpactAffectedProgram(
                    program.Id,
                    program.ProgramKey,
                    program.Name,
                    requirement.Id,
                    requirement.KnownVersionNumber,
                    requirement.KnownStatus,
                    members ?? Array.Empty<Guid>());
            })
            .DistinctBy(x => x.TrainingProgramId)
            .OrderBy(x => x.ProgramKey)
            .ToList();
    }

    private async Task<IReadOnlyList<RulePackImpactAffectedAssignment>> BuildAffectedAssignmentsAsync(
        Guid tenantId,
        IReadOnlyList<Guid> definitionIds,
        CancellationToken cancellationToken)
    {
        if (definitionIds.Count == 0)
        {
            return Array.Empty<RulePackImpactAffectedAssignment>();
        }

        return await db.TrainingAssignments
            .AsNoTracking()
            .Include(x => x.TrainingDefinition)
            .Where(x =>
                x.TenantId == tenantId
                && definitionIds.Contains(x.TrainingDefinitionId)
                && TrainingAssignmentService.ActiveAssignmentStatuses.Contains(x.Status))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new RulePackImpactAffectedAssignment(
                x.Id,
                x.StaffarrPersonId,
                x.TrainingDefinitionId,
                x.TrainingDefinition.Name,
                x.Status,
                x.AssignmentReason,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<RulePackImpactAffectedQualification>> BuildAffectedQualificationsAsync(
        Guid tenantId,
        IReadOnlyList<Guid> definitionIds,
        IReadOnlyList<string> qualificationKeys,
        CancellationToken cancellationToken)
    {
        if (definitionIds.Count == 0 && qualificationKeys.Count == 0)
        {
            return Array.Empty<RulePackImpactAffectedQualification>();
        }

        var query = db.QualificationIssues
            .AsNoTracking()
            .Include(x => x.TrainingAssignment)
            .Where(x => x.TenantId == tenantId && ActiveQualificationStatuses.Contains(x.Status));

        query = query.Where(x =>
            definitionIds.Contains(x.TrainingAssignment.TrainingDefinitionId)
            || qualificationKeys.Contains(x.QualificationKey));

        return await query
            .OrderByDescending(x => x.IssuedAt)
            .Select(x => new RulePackImpactAffectedQualification(
                x.Id,
                x.StaffarrPersonId,
                x.TrainingAssignmentId,
                x.QualificationKey,
                x.QualificationName,
                x.Status,
                x.IssuedAt))
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyList<RulePackImpactRecommendedAction> BuildRecommendedActions(
        IReadOnlyList<string> triggers,
        IReadOnlyList<RulePackImpactAffectedDefinition> definitions,
        IReadOnlyList<RulePackImpactAffectedProgram> programs,
        IReadOnlyList<RulePackImpactAffectedAssignment> assignments,
        IReadOnlyList<RulePackImpactAffectedQualification> qualifications,
        RulePackImpactDriftResponse? drift)
    {
        var actions = new List<RulePackImpactRecommendedAction>();

        if (drift?.PackNotFound == true)
        {
            actions.Add(new RulePackImpactRecommendedAction(
                RulePackImpactRecommendedActionTypes.ReviewInactiveRulePack,
                "high",
                "Rule pack was not found in Compliance Core. Review linked training requirements and update or remove stale references."));
        }
        else if (drift?.PackInactive == true)
        {
            actions.Add(new RulePackImpactRecommendedAction(
                RulePackImpactRecommendedActionTypes.ReviewInactiveRulePack,
                "high",
                "Rule pack is inactive in Compliance Core. Review whether linked training should be suspended or remapped."));
        }

        if (triggers.Contains(RulePackImpactTriggers.VersionDrift, StringComparer.Ordinal)
            || triggers.Contains(RulePackImpactTriggers.StatusChange, StringComparer.Ordinal))
        {
            actions.Add(new RulePackImpactRecommendedAction(
                RulePackImpactRecommendedActionTypes.ReviewRequirements,
                "high",
                "Rule pack version or status changed in Compliance Core. Review linked requirements and re-validate with Compliance Core."));
        }

        foreach (var definition in definitions)
        {
            actions.Add(new RulePackImpactRecommendedAction(
                RulePackImpactRecommendedActionTypes.AcknowledgeRulePackChange,
                "medium",
                $"Review training definition \"{definition.Name}\" linked to the changed rule pack.",
                TrainingRulePackRequirementEntityTypes.TrainingDefinition,
                definition.TrainingDefinitionId));
        }

        foreach (var program in programs)
        {
            actions.Add(new RulePackImpactRecommendedAction(
                RulePackImpactRecommendedActionTypes.AcknowledgeRulePackChange,
                "medium",
                $"Review training program \"{program.Name}\" linked to the changed rule pack.",
                TrainingRulePackRequirementEntityTypes.TrainingProgram,
                program.TrainingProgramId));
        }

        if (assignments.Count > 0)
        {
            actions.Add(new RulePackImpactRecommendedAction(
                RulePackImpactRecommendedActionTypes.ReviewActiveAssignments,
                "high",
                $"{assignments.Count} active training assignment(s) may need review or reassignment after the rule pack change."));
        }

        foreach (var qualification in qualifications)
        {
            actions.Add(new RulePackImpactRecommendedAction(
                RulePackImpactRecommendedActionTypes.ReRunQualificationCheck,
                "high",
                $"Re-run qualification check for {qualification.QualificationName} ({qualification.Status}) after rule pack change.",
                "qualification_issue",
                qualification.QualificationIssueId));
        }

        if (actions.Count == 0 && (definitions.Count > 0 || programs.Count > 0))
        {
            actions.Add(new RulePackImpactRecommendedAction(
                RulePackImpactRecommendedActionTypes.AcknowledgeRulePackChange,
                "low",
                "No active assignments or qualifications require immediate action. Acknowledge the assessment for audit trail."));
        }

        return actions;
    }
}
