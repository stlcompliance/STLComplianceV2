using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class ReadinessService(StaffArrDbContext db)
{
    public async Task<PersonReadinessResponse> GetPersonReadinessAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var exists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!exists)
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

        var readinessStatus = blockers.Count == 0 ? "ready" : "not_ready";

        return new PersonReadinessResponse(
            personId,
            readinessStatus,
            DateTimeOffset.UtcNow,
            requirementStatuses,
            blockers);
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
            requirement.CertificationKey,
            requirement.Name,
            blockerType,
            message);
}
