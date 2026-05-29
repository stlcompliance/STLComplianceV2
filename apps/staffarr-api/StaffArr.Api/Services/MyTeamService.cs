using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class MyTeamService(
    StaffArrDbContext db,
    ManagerHierarchyService managerHierarchyService,
    ReadinessService readinessService)
{
    private static readonly TimeSpan ExpiringSoonWindow = TimeSpan.FromDays(30);

    private static readonly HashSet<string> OpenIncidentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "open",
        "submitted",
        "triage",
        "needs_review",
        "assigned",
        "in_progress",
        "waiting_on_training_review",
        "waiting_on_compliance_review",
        "corrective_action_pending",
    };

    private static readonly HashSet<string> PendingUpdateRequestStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PersonnelUpdateRequestStatuses.Submitted,
        PersonnelUpdateRequestStatuses.PendingReview,
    };

    public async Task<MyTeamDashboardResponse> GetDashboardAsync(
        Guid tenantId,
        Guid managerPersonId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var subordinates = await managerHierarchyService.GetSubordinatesAsync(
            tenantId,
            managerPersonId,
            includeIndirect: false,
            limit,
            cancellationToken);

        if (subordinates.Count == 0)
        {
            return new MyTeamDashboardResponse(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                [],
                []);
        }

        var personIds = subordinates.Select(x => x.PersonId).ToArray();
        var now = DateTimeOffset.UtcNow;
        var expiringThreshold = now.Add(ExpiringSoonWindow);

        var expiringCertCounts = await db.PersonCertifications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && personIds.Contains(x.PersonId)
                && x.Status == "active"
                && x.ExpiresAt != null
                && x.ExpiresAt > now
                && x.ExpiresAt <= expiringThreshold)
            .GroupBy(x => x.PersonId)
            .Select(g => new { PersonId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PersonId, x => x.Count, cancellationToken);

        var openIncidentCounts = await db.PersonnelIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && personIds.Contains(x.PersonId)
                && OpenIncidentStatuses.Contains(x.Status))
            .GroupBy(x => x.PersonId)
            .Select(g => new { PersonId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PersonId, x => x.Count, cancellationToken);

        var pendingUpdateCounts = await db.PersonnelUpdateRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && personIds.Contains(x.PersonId)
                && PendingUpdateRequestStatuses.Contains(x.Status))
            .GroupBy(x => x.PersonId)
            .Select(g => new { PersonId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PersonId, x => x.Count, cancellationToken);

        var trainingBlockerCounts = await db.PersonTrainingBlockers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && personIds.Contains(x.PersonId)
                && x.Status == "active")
            .GroupBy(x => x.PersonId)
            .Select(g => new { PersonId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PersonId, x => x.Count, cancellationToken);

        var pendingUpdateRequests = await db.PersonnelUpdateRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && personIds.Contains(x.PersonId)
                && PendingUpdateRequestStatuses.Contains(x.Status))
            .OrderByDescending(x => x.SubmittedAt)
            .Take(25)
            .ToListAsync(cancellationToken);

        var readinessTasks = subordinates
            .Select(async subordinate =>
            {
                var readiness = await readinessService.GetPersonReadinessAsync(
                    tenantId,
                    subordinate.PersonId,
                    cancellationToken);
                return (subordinate, readiness);
            })
            .ToArray();
        var readinessResults = await Task.WhenAll(readinessTasks);

        var members = new List<MyTeamMemberResponse>(readinessResults.Length);
        var notReadyCount = 0;
        var totalExpiringCerts = 0;
        var totalOpenIncidents = 0;
        var totalPendingUpdates = 0;
        var onboardingInProgressCount = 0;
        var totalTrainingBlockers = 0;

        foreach (var (subordinate, readiness) in readinessResults)
        {
            var expiringCount = expiringCertCounts.GetValueOrDefault(subordinate.PersonId);
            var incidentCount = openIncidentCounts.GetValueOrDefault(subordinate.PersonId);
            var updateCount = pendingUpdateCounts.GetValueOrDefault(subordinate.PersonId);
            var blockerCount = trainingBlockerCounts.GetValueOrDefault(subordinate.PersonId);

            if (!string.Equals(readiness.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase))
            {
                notReadyCount++;
            }

            if (!string.Equals(subordinate.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(readiness.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase))
            {
                onboardingInProgressCount++;
            }

            totalExpiringCerts += expiringCount;
            totalOpenIncidents += incidentCount;
            totalPendingUpdates += updateCount;
            totalTrainingBlockers += blockerCount;

            members.Add(new MyTeamMemberResponse(
                subordinate,
                readiness.ReadinessStatus,
                readiness.Blockers.Count,
                expiringCount,
                incidentCount,
                updateCount,
                blockerCount));
        }

        return new MyTeamDashboardResponse(
            subordinates.Count,
            notReadyCount,
            totalExpiringCerts,
            totalOpenIncidents,
            totalPendingUpdates,
            onboardingInProgressCount,
            totalTrainingBlockers,
            members,
            pendingUpdateRequests.Select(MapUpdateRequest).ToList());
    }

    private static PersonnelUpdateRequestResponse MapUpdateRequest(PersonnelUpdateRequest record) =>
        new(
            record.Id,
            record.PersonId,
            record.RequestType,
            record.Status,
            record.FieldKey,
            record.CurrentValue,
            record.RequestedValue,
            record.Details,
            record.SubmittedByUserId,
            record.SubmittedAt,
            record.ReviewedByUserId,
            record.ReviewedAt,
            record.ReviewNotes,
            record.CreatedAt,
            record.UpdatedAt);
}
