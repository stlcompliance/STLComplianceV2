using System.Text;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace TrainArr.Api.Services;

public sealed class AssignmentReportService(TrainArrDbContext db)
{
    private const int RecentLimit = 25;

    public async Task<AssignmentReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? status,
        bool overdueOnly,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var assignments = await db.TrainingAssignments
            .AsNoTracking()
            .Include(x => x.TrainingDefinition)
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var filtered = assignments
            .Where(x => MatchesStatusFilter(x, status))
            .Where(x => !overdueOnly || IsOverdue(x, now))
            .ToList();

        var openCount = assignments.Count(x => AssignmentDueReminderRules.OpenAssignmentStatuses.Contains(x.Status));
        var completedCount = assignments.Count(x => string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase));
        var overdueCount = assignments.Count(x => IsOverdue(x, now));
        var total = assignments.Count;
        var completionRate = total == 0 ? 0m : Math.Round(completedCount * 100m / total, 1);

        var completedAssignments = assignments
            .Where(x => string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase) && x.CompletedAt is not null)
            .ToList();
        var assignmentIds = assignments.Select(x => x.Id).ToList();
        var evaluationRows = await db.TrainingEvaluations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assignmentIds.Contains(x.TrainingAssignmentId))
            .Select(x => new { x.TrainingAssignmentId, x.Result, x.Score })
            .ToListAsync(cancellationToken);
        var evidenceByAssignmentId = await db.TrainingEvidence
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assignmentIds.Contains(x.TrainingAssignmentId))
            .GroupBy(x => x.TrainingAssignmentId)
            .Select(g => new { AssignmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AssignmentId, x => x.Count, cancellationToken);
        var signoffsByAssignmentId = await db.TrainingSignoffs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assignmentIds.Contains(x.TrainingAssignmentId))
            .GroupBy(x => x.TrainingAssignmentId)
            .Select(g => new { AssignmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AssignmentId, x => x.Count, cancellationToken);
        var laborRows = await db.TrainingAssignmentLaborEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assignmentIds.Contains(x.TrainingAssignmentId))
            .Select(x => new { x.TrainingAssignmentId, x.HoursWorked, x.CostPerHour })
            .ToListAsync(cancellationToken);
        var localizedContentReferences = await db.TrainingProgramContentReferences
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.LocaleTag != null)
            .Select(x => x.LocaleTag)
            .ToListAsync(cancellationToken);

        var averageCompletionDays = completedAssignments.Count == 0
            ? null
            : (decimal?)Math.Round(
                completedAssignments
                    .Average(x => (x.CompletedAt!.Value - x.CreatedAt).TotalDays),
                1);
        var evaluationPassCount = evaluationRows.Count(x =>
            string.Equals(x.Result, "pass", StringComparison.OrdinalIgnoreCase)
            || string.Equals(x.Result, "passed", StringComparison.OrdinalIgnoreCase));
        decimal? evaluationPassRate = evaluationRows.Count == 0
            ? null
            : Math.Round(evaluationPassCount * 100m / evaluationRows.Count, 1);
        var averageEvaluationScore = evaluationRows.Count(x => x.Score is not null) == 0
            ? null
            : (decimal?)Math.Round(
                evaluationRows.Where(x => x.Score is not null).Average(x => x.Score!.Value),
                1);
        var completedWithEvidenceCount = completedAssignments.Count(x =>
            evidenceByAssignmentId.TryGetValue(x.Id, out var count) && count > 0);
        var completedWithSignoffCount = completedAssignments.Count(x =>
            signoffsByAssignmentId.TryGetValue(x.Id, out var count) && count > 0);
        var evidenceCoverage = completedAssignments.Count == 0
            ? 0m
            : Math.Round(completedWithEvidenceCount * 100m / completedAssignments.Count, 1);
        var signoffCoverage = completedAssignments.Count == 0
            ? 0m
            : Math.Round(completedWithSignoffCount * 100m / completedAssignments.Count, 1);
        var totalLaborHours = laborRows.Sum(x => x.HoursWorked);
        var totalLaborCost = laborRows.Sum(x => Math.Round(x.HoursWorked * x.CostPerHour, 2));
        var averageLaborHours = completedAssignments.Count == 0
            ? null
            : (decimal?)Math.Round(totalLaborHours / completedAssignments.Count, 2);
        var averageLaborCost = completedAssignments.Count == 0
            ? null
            : (decimal?)Math.Round(totalLaborCost / completedAssignments.Count, 2);

        var analytics = new AssignmentEffectivenessAnalyticsResponse(
            averageCompletionDays,
            evaluationPassRate,
            averageEvaluationScore,
            evidenceCoverage,
            signoffCoverage,
            Math.Round(totalLaborHours, 2),
            Math.Round(totalLaborCost, 2),
            averageLaborHours,
            averageLaborCost,
            localizedContentReferences.Count,
            localizedContentReferences.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var recent = filtered
            .OrderByDescending(x => x.UpdatedAt)
            .Take(RecentLimit)
            .Select(x => new AssignmentReportSummaryItem(
                x.Id,
                x.StaffarrPersonId,
                x.TrainingDefinition.DefinitionKey,
                x.TrainingDefinition.Name,
                x.Status,
                x.DueAt,
                IsOverdue(x, now),
                x.CreatedAt,
                x.CompletedAt))
            .ToList();

        return new AssignmentReportSummaryResponse(
            filtered.Count,
            openCount,
            completedCount,
            overdueCount,
            completionRate,
            analytics,
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? status,
        bool overdueOnly,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, status, overdueOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "assignmentId,staffarrPersonId,definitionKey,definitionName,status,dueAt,isOverdue,createdAt,completedAt");

        foreach (var item in summary.RecentAssignments)
        {
            builder.Append(CsvEscape(item.AssignmentId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.StaffarrPersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.DefinitionKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.DefinitionName));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.DueAt?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.Append(item.IsOverdue ? "true" : "false");
            builder.Append(',');
            builder.Append(CsvEscape(item.CreatedAt.ToString("O")));
            builder.Append(',');
            builder.AppendLine(CsvEscape(item.CompletedAt?.ToString("O") ?? string.Empty));
        }

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        return new CsvExportResult(
            "text/csv",
            $"trainarr-assignment-report-{timestamp}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<AssignmentOverdueReportResponse> GetOverdueReportAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, null, overdueOnly: true, cancellationToken);
        return new AssignmentOverdueReportResponse(
            DateTimeOffset.UtcNow,
            summary.TotalAssignments,
            summary.RecentAssignments);
    }

    private static bool MatchesStatusFilter(TrainingAssignment assignment, string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(assignment.Status, status.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOverdue(TrainingAssignment assignment, DateTimeOffset now) =>
        AssignmentDueReminderRules.OpenAssignmentStatuses.Contains(assignment.Status)
        && assignment.DueAt is not null
        && assignment.DueAt < now;

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
