using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;

namespace TrainArr.Api.Services;

public sealed class PersonalTrainingDashboardService(
    TrainArrDbContext db,
    TrainingAssignmentService assignmentService,
    FieldInboxService fieldInboxService,
    PersonTrainingHistoryService historyService)
{
    private const int RecentHistoryLimit = 10;
    private const int ExpiringSoonDays = 30;

    public async Task<PersonalTrainingDashboardResponse> GetAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiringThreshold = now.AddDays(ExpiringSoonDays);

        var assignments = await assignmentService.ListAsync(
            tenantId,
            staffarrPersonId,
            staffarrIncidentRemediationId: null,
            status: null,
            cancellationToken);
        var qualifications = await db.QualificationIssues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.StaffarrPersonId == staffarrPersonId)
            .OrderByDescending(x => x.IssuedAt)
            .Select(x => new QualificationIssueResponse(
                x.Id,
                x.TrainingAssignmentId,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.QualificationName,
                x.GrantPublicationId,
                x.Status,
                x.IssuedAt,
                x.ExpiresAt,
                x.StatusChangedAt,
                x.LifecycleReason,
                x.LifecyclePublicationId))
            .ToListAsync(cancellationToken);
        var inbox = await fieldInboxService.GetAsync(tenantId, staffarrPersonId, cancellationToken);
        var history = await historyService.GetForPersonAsync(
            tenantId,
            staffarrPersonId,
            RecentHistoryLimit,
            cancellationToken);

        return new PersonalTrainingDashboardResponse(
            staffarrPersonId,
            now,
            new PersonalTrainingDashboardSummary(
                assignments.Count(x => TrainingAssignmentService.ActiveAssignmentStatuses.Contains(x.Status)),
                assignments.Count(x => string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase)),
                assignments.Count(x => TrainingAssignmentService.ActiveAssignmentStatuses.Contains(x.Status)
                    && x.DueAt is not null
                    && x.DueAt < now),
                qualifications.Count,
                qualifications.Count(x => string.Equals(x.Status, "issued", StringComparison.OrdinalIgnoreCase)
                    && x.ExpiresAt is not null
                    && x.ExpiresAt >= now
                    && x.ExpiresAt <= expiringThreshold),
                inbox.Summary.TotalCount,
                history.Items.Count),
            assignments,
            qualifications,
            inbox,
            history.Items);
    }
}
