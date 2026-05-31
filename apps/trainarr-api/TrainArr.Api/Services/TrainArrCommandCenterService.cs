using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;

namespace TrainArr.Api.Services;

public sealed class TrainArrCommandCenterService(
    TrainArrDbContext db,
    AssignmentReportService assignmentReports,
    QualificationReportService qualificationReports,
    ComplianceReportService complianceReports)
{
    public async Task<TrainArrCommandCenterResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await assignmentReports.GetSummaryAsync(
            tenantId,
            status: null,
            overdueOnly: false,
            cancellationToken);
        var qualifications = await qualificationReports.GetSummaryAsync(tenantId, status: null, cancellationToken);
        var compliance = await complianceReports.GetSummaryAsync(
            tenantId,
            attentionOnly: false,
            cancellationToken);
        var gaps = await complianceReports.GetGapReportAsync(tenantId, cancellationToken);

        var failedEvaluations = await db.TrainingEvaluations
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId
                && (x.Result == "fail"
                    || x.Result == "failed"
                    || x.Result == "remediation_required"),
                cancellationToken);

        var programsNeedingReview = await db.TrainingPrograms
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId
                && (x.Status == "draft"
                    || x.Status == "in_review"
                    || x.Status == "review_required"),
                cancellationToken);

        var risks = BuildRisks(
            assignments,
            qualifications,
            compliance,
            gaps.TotalGaps,
            failedEvaluations,
            programsNeedingReview);

        return new TrainArrCommandCenterResponse(
            DateTimeOffset.UtcNow,
            assignments,
            qualifications,
            compliance,
            failedEvaluations,
            compliance.OpenRemediationCount,
            qualifications.ExpiringWithin30Days,
            programsNeedingReview,
            gaps.TotalGaps,
            CalculateAuditReadinessScore(
                assignments.OverdueAssignments,
                qualifications.ExpiredCount + qualifications.SuspendedCount + qualifications.RevokedCount,
                compliance.AttentionItemCount,
                gaps.TotalGaps,
                failedEvaluations,
                programsNeedingReview),
            risks);
    }

    private static IReadOnlyList<TrainArrCommandCenterRiskItem> BuildRisks(
        AssignmentReportSummaryResponse assignments,
        QualificationReportSummaryResponse qualifications,
        ComplianceReportSummaryResponse compliance,
        int unqualifiedAssignmentRisks,
        int failedEvaluations,
        int programsNeedingReview)
    {
        List<TrainArrCommandCenterRiskItem> risks = [];

        AddRisk(
            risks,
            "overdue_assignments",
            assignments.OverdueAssignments,
            "high",
            "Training assignments are overdue.",
            "/api/v1/reports/assignments/overdue");
        AddRisk(
            risks,
            "expiring_qualifications",
            qualifications.ExpiringWithin30Days,
            "medium",
            "Qualifications expire within 30 days.",
            "/api/v1/reports/qualifications/expiring");
        AddRisk(
            risks,
            "failed_evaluations",
            failedEvaluations,
            "high",
            "Training evaluations need remediation follow-up.",
            null);
        AddRisk(
            risks,
            "remediation_backlog",
            compliance.OpenRemediationCount,
            "high",
            "Incident-triggered training remediations remain open.",
            "/api/v1/reports/compliance/summary");
        AddRisk(
            risks,
            "unqualified_assignment_risks",
            unqualifiedAssignmentRisks,
            "high",
            "Open assignments are missing current issued qualifications.",
            "/api/v1/reports/compliance/gaps");
        AddRisk(
            risks,
            "programs_needing_review",
            programsNeedingReview,
            "medium",
            "Training programs are still draft or waiting for review.",
            null);

        return risks;
    }

    private static void AddRisk(
        ICollection<TrainArrCommandCenterRiskItem> risks,
        string key,
        int count,
        string severity,
        string message,
        string? reportPath)
    {
        if (count <= 0)
        {
            return;
        }

        risks.Add(new TrainArrCommandCenterRiskItem(key, severity, count, message, reportPath));
    }

    private static int CalculateAuditReadinessScore(
        int overdueAssignments,
        int qualificationLifecycleIssues,
        int complianceAttentionItems,
        int unqualifiedAssignmentRisks,
        int failedEvaluations,
        int programsNeedingReview)
    {
        var penalty =
            overdueAssignments * 8
            + qualificationLifecycleIssues * 8
            + complianceAttentionItems * 6
            + unqualifiedAssignmentRisks * 8
            + failedEvaluations * 8
            + programsNeedingReview * 4;

        return Math.Clamp(100 - penalty, 0, 100);
    }
}
