using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;

namespace TrainArr.Api.Services;

public sealed class ReadinessAlertReportService(TrainArrDbContext db)
{
    private const int AlertLimit = 100;

    public async Task<IReadOnlyList<TrainArrReadinessAlertResponse>> GetAlertsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiringThreshold = now.AddDays(30);

        var overdueAssignments = await db.TrainingAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && AssignmentDueReminderRules.OpenAssignmentStatuses.Contains(x.Status)
                && x.DueAt != null
                && x.DueAt < now)
            .OrderBy(x => x.DueAt)
            .Take(AlertLimit)
            .ToListAsync(cancellationToken);

        var expiringQualifications = await db.QualificationIssues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Status == "issued"
                && x.ExpiresAt != null
                && x.ExpiresAt >= now
                && x.ExpiresAt <= expiringThreshold)
            .OrderBy(x => x.ExpiresAt)
            .Take(AlertLimit)
            .ToListAsync(cancellationToken);

        var failedEvaluations = await db.TrainingEvaluations
            .AsNoTracking()
            .Include(x => x.TrainingAssignment)
            .Where(x => x.TenantId == tenantId
                && (x.Result == "fail" || x.Result == "remediation_required"))
            .OrderByDescending(x => x.EvaluatedAt)
            .Take(AlertLimit)
            .ToListAsync(cancellationToken);

        var openRemediations = await db.StaffarrIncidentRemediations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && !string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(x.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.UpdatedAt)
            .Take(AlertLimit)
            .ToListAsync(cancellationToken);

        var alerts = new List<TrainArrReadinessAlertResponse>(
            overdueAssignments.Count + expiringQualifications.Count + failedEvaluations.Count + openRemediations.Count);

        alerts.AddRange(overdueAssignments.Select(x => new TrainArrReadinessAlertResponse(
            AlertType: "overdue_training_assignment",
            Severity: "high",
            StaffarrPersonId: x.StaffarrPersonId,
            Message: "Training assignment is overdue and blocks readiness until completed.",
            DetectedAt: x.DueAt ?? x.UpdatedAt,
            AssignmentId: x.Id,
            QualificationIssueId: null,
            EvaluationId: null,
            RemediationId: null)));

        alerts.AddRange(expiringQualifications.Select(x => new TrainArrReadinessAlertResponse(
            AlertType: "expiring_qualification",
            Severity: "medium",
            StaffarrPersonId: x.StaffarrPersonId,
            Message: $"Qualification {x.QualificationKey} expires within 30 days.",
            DetectedAt: x.ExpiresAt ?? x.UpdatedAt,
            AssignmentId: x.TrainingAssignmentId,
            QualificationIssueId: x.Id,
            EvaluationId: null,
            RemediationId: null)));

        alerts.AddRange(failedEvaluations.Select(x => new TrainArrReadinessAlertResponse(
            AlertType: "failed_training_evaluation",
            Severity: "high",
            StaffarrPersonId: x.TrainingAssignment.StaffarrPersonId,
            Message: "Training evaluation failed and requires remediation or reassessment.",
            DetectedAt: x.EvaluatedAt,
            AssignmentId: x.TrainingAssignmentId,
            QualificationIssueId: null,
            EvaluationId: x.Id,
            RemediationId: null)));

        alerts.AddRange(openRemediations.Select(x => new TrainArrReadinessAlertResponse(
            AlertType: "open_training_remediation",
            Severity: "medium",
            StaffarrPersonId: x.StaffarrPersonId,
            Message: "Incident-driven training remediation remains open.",
            DetectedAt: x.UpdatedAt,
            AssignmentId: null,
            QualificationIssueId: null,
            EvaluationId: null,
            RemediationId: x.Id)));

        return alerts
            .OrderByDescending(x => x.DetectedAt)
            .Take(AlertLimit)
            .ToList();
    }
}
