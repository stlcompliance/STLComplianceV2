using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class ReadinessService(
    StaffArrDbContext db,
    ReadinessOverrideService overrideService,
    TrainingBlockerIngestionService trainingBlockerService,
    ComplianceCorePersonReadinessGateClient complianceCoreReadinessGate,
    IStaffArrAuditService audit)
{
    public const string ReadAction = "staffarr.readiness.read";

    public const string PersonReadinessSnapshotKind = "person_readiness";

    public const string RoutarrDispatchSnapshotKind = "routarr_dispatch_readiness";

    public async Task<PersonReadinessResponse> GetPersonReadinessAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null,
        string? auditSnapshotKind = null)
    {
        var person = await db.People
            .AsNoTracking()
            .FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        await StaffArrReadinessCertificationSeed.EnsureBaselineDefinitionsAsync(db, tenantId, cancellationToken);

        var requirements = await db.CertificationDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Category == "readiness"
                && x.Status == "active")
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var personRecords = await db.PersonCertifications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.GrantedAt)
            .ToListAsync(cancellationToken);

        var recordsByDefinition = personRecords
            .GroupBy(x => x.CertificationDefinitionId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var requirementStatuses = new List<ReadinessRequirementStatusResponse>();
        var blockers = new List<ReadinessBlockerResponse>();

        foreach (var requirement in requirements)
        {
            recordsByDefinition.TryGetValue(requirement.Id, out var records);
            records ??= [];

            var evaluation = EvaluateRequirement(requirement, records);
            requirementStatuses.Add(evaluation.RequirementStatus);
            if (evaluation.Blocker is not null)
            {
                blockers.Add(evaluation.Blocker);
            }
        }

        var certificationReady = blockers.Count == 0;

        var trainingBlockers = await trainingBlockerService.GetActiveBlockersAsync(
            tenantId,
            personId,
            cancellationToken);
        foreach (var trainingBlocker in trainingBlockers)
        {
            blockers.Add(MapTrainingBlocker(trainingBlocker));
        }
        var activeOverride = await overrideService.GetEffectiveActiveOverrideAsync(
            tenantId,
            personId,
            cancellationToken);

        ReadinessOverrideSummaryResponse? overrideSummary = null;
        if (activeOverride is not null)
        {
            overrideSummary = new ReadinessOverrideSummaryResponse(
                activeOverride.Id,
                activeOverride.Reason,
                activeOverride.GrantedAt,
                activeOverride.ExpiresAt,
                activeOverride.GrantedByUserId);
        }

        var hasTrainingBlockers = trainingBlockers.Count > 0;
        var localReadinessStatus = (!hasTrainingBlockers && certificationReady) || activeOverride is not null
            ? "ready"
            : "not_ready";
        var localReadinessBasis = activeOverride is not null && (!certificationReady || hasTrainingBlockers)
            ? "manual_override"
            : hasTrainingBlockers
                ? "training_blockers"
                : "certifications";

        var complianceCoreResult = await complianceCoreReadinessGate.CheckPersonReadinessAsync(
            tenantId,
            personId,
            person.DisplayName,
            new Dictionary<string, string>
            {
                ["product"] = "staffarr",
                ["action"] = "person_readiness",
                ["person_id"] = personId.ToString("D"),
                ["personId"] = personId.ToString("D"),
                ["person_status"] = person.EmploymentStatus,
                ["personStatus"] = person.EmploymentStatus,
                ["local_readiness_status"] = localReadinessStatus,
                ["localReadinessStatus"] = localReadinessStatus,
                ["local_readiness_basis"] = localReadinessBasis,
                ["localReadinessBasis"] = localReadinessBasis,
                ["certification_ready"] = certificationReady ? "true" : "false",
                ["certificationReady"] = certificationReady ? "true" : "false",
                ["has_training_blockers"] = hasTrainingBlockers ? "true" : "false",
                ["hasTrainingBlockers"] = hasTrainingBlockers ? "true" : "false",
                ["has_manual_override"] = activeOverride is null ? "false" : "true",
                ["hasManualOverride"] = activeOverride is null ? "false" : "true"
            },
            cancellationToken);

        if (complianceCoreResult is not null && IsBlockingComplianceCoreOutcome(complianceCoreResult))
        {
            blockers.Add(new ReadinessBlockerResponse(
                "compliancecore",
                string.IsNullOrWhiteSpace(complianceCoreResult.ReasonCode)
                    ? complianceCoreResult.Outcome
                    : complianceCoreResult.ReasonCode,
                string.IsNullOrWhiteSpace(complianceCoreResult.Message)
                    ? "Compliance Core rules prevent this person from being used for the requested activity."
                    : complianceCoreResult.Message,
                null,
                null,
                null,
                null));
        }

        var hasComplianceCoreBlockers = blockers.Any(x =>
            string.Equals(x.BlockerSource, "compliancecore", StringComparison.OrdinalIgnoreCase));
        var readinessStatus = hasComplianceCoreBlockers
            ? "not_ready"
            : localReadinessStatus;
        var readinessBasis = hasComplianceCoreBlockers
            ? "compliancecore"
            : localReadinessBasis;

        ReadinessAuditSnapshotResponse? auditSnapshot = null;
        if (!string.IsNullOrWhiteSpace(auditSnapshotKind))
        {
            var auditResult = await audit.WriteAsync(
                ReadAction,
                tenantId,
                actorUserId,
                "person_readiness",
                personId.ToString(),
                readinessStatus,
                readinessBasis,
                cancellationToken);
            auditSnapshot = new ReadinessAuditSnapshotResponse(
                auditResult.AuditEventId,
                auditSnapshotKind,
                auditResult.OccurredAt);
        }

        return new PersonReadinessResponse(
            personId,
            readinessStatus,
            readinessBasis,
            DateTimeOffset.UtcNow,
            requirementStatuses,
            blockers,
            overrideSummary,
            auditSnapshot);
    }

    private static (ReadinessRequirementStatusResponse RequirementStatus, ReadinessBlockerResponse? Blocker)
        EvaluateRequirement(CertificationDefinition requirement, IReadOnlyList<PersonCertification> records)
    {
        if (records.Count == 0)
        {
            return (
                new ReadinessRequirementStatusResponse(
                    requirement.Id,
                    requirement.CertificationKey,
                    requirement.Name,
                    "missing",
                    null,
                    null),
                CreateBlocker(requirement, "missing", $"{requirement.Name} is required but has not been granted."));
        }

        var activeRecord = records.FirstOrDefault(x =>
            string.Equals(PersonCertificationEffectiveStatus.Resolve(x), "active", StringComparison.OrdinalIgnoreCase));
        if (activeRecord is not null)
        {
            return (
                new ReadinessRequirementStatusResponse(
                    requirement.Id,
                    requirement.CertificationKey,
                    requirement.Name,
                    "satisfied",
                    "active",
                    activeRecord.ExpiresAt),
                null);
        }

        var latestRecord = records[0];
        var effectiveStatus = PersonCertificationEffectiveStatus.Resolve(latestRecord);

        if (string.Equals(effectiveStatus, "expired", StringComparison.OrdinalIgnoreCase))
        {
            var expiryText = latestRecord.ExpiresAt?.ToString("yyyy-MM-dd") ?? "the recorded expiration date";
            return (
                new ReadinessRequirementStatusResponse(
                    requirement.Id,
                    requirement.CertificationKey,
                    requirement.Name,
                    "expired",
                    effectiveStatus,
                    latestRecord.ExpiresAt),
                CreateBlocker(
                    requirement,
                    "expired",
                    $"{requirement.Name} expired on {expiryText} and must be renewed before assignment."));
        }

        if (string.Equals(effectiveStatus, "revoked", StringComparison.OrdinalIgnoreCase))
        {
            return (
                new ReadinessRequirementStatusResponse(
                    requirement.Id,
                    requirement.CertificationKey,
                    requirement.Name,
                    "revoked",
                    effectiveStatus,
                    latestRecord.ExpiresAt),
                CreateBlocker(
                    requirement,
                    "revoked",
                    $"{requirement.Name} was revoked and must be re-granted before assignment."));
        }

        return (
            new ReadinessRequirementStatusResponse(
                requirement.Id,
                requirement.CertificationKey,
                requirement.Name,
                "missing",
                effectiveStatus,
                latestRecord.ExpiresAt),
            CreateBlocker(requirement, "missing", $"{requirement.Name} is required but has not been granted."));
    }

    private static ReadinessBlockerResponse CreateBlocker(
        CertificationDefinition requirement,
        string blockerType,
        string message) =>
        new(
            "certification",
            blockerType,
            message,
            requirement.CertificationKey,
            requirement.Name,
            null,
            null);

    private static ReadinessBlockerResponse MapTrainingBlocker(PersonTrainingBlocker blocker) =>
        new(
            "training",
            blocker.BlockerType,
            blocker.Message,
            null,
            null,
            blocker.QualificationKey,
            blocker.QualificationName);

    private static bool IsBlockingComplianceCoreOutcome(ComplianceCoreProductGateResponse? result)
    {
        if (result is null)
        {
            return false;
        }

        return !string.Equals(result.Outcome, "allow", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(result.Outcome, "warn", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(result.Outcome, "waived", StringComparison.OrdinalIgnoreCase);
    }
}
