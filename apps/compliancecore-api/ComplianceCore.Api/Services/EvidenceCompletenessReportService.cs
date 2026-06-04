using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class EvidenceCompletenessReportService(ComplianceCoreDbContext db)
{
    public async Task<EvidenceCompletenessReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? scopeKey,
        string? rulePackKey,
        string? severity,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var query = db.MissingEvidenceWarnings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(scopeKey)
            && !string.Equals(scopeKey, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.ScopeKey == scopeKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(rulePackKey))
        {
            query = query.Where(x => x.PackKey == rulePackKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(severity)
            && !string.Equals(severity, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Severity == severity.Trim());
        }

        var warnings = await query
            .OrderByDescending(x => x.EvaluatedAt)
            .ToListAsync(cancellationToken);

        var groups = warnings
            .GroupBy(x => new { x.RulePackId, x.PackKey, x.ScopeKey })
            .Select(group =>
            {
                var criticalCount = group.Count(x => x.Severity == MissingEvidenceWarningSeverities.Critical);
                var highCount = group.Count(x => x.Severity == MissingEvidenceWarningSeverities.High);
                var mediumCount = group.Count(x => x.Severity == MissingEvidenceWarningSeverities.Medium);
                var lowCount = group.Count(x => x.Severity == MissingEvidenceWarningSeverities.Low);
                var totalWarnings = group.Count();
                var score = ComputeCompletenessScore(totalWarnings, criticalCount, highCount, mediumCount, lowCount);
                var level = DetermineLevel(score, totalWarnings);
                var latest = group.Max(x => x.EvaluatedAt);
                var summary = totalWarnings == 0
                    ? "No missing evidence warnings recorded."
                    : $"{totalWarnings} warning(s): {criticalCount} critical, {highCount} high, {mediumCount} medium, {lowCount} low.";

                return new EvidenceCompletenessReportItem(
                    group.Key.RulePackId,
                    group.Key.PackKey,
                    group.Key.ScopeKey,
                    totalWarnings,
                    criticalCount,
                    highCount,
                    mediumCount,
                    lowCount,
                    score,
                    level,
                    latest,
                    summary);
            })
            .OrderByDescending(x => x.CompletenessScore)
            .ThenByDescending(x => x.LatestWarningAt ?? DateTimeOffset.MinValue)
            .ThenBy(x => x.PackKey, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var cappedLimit = Math.Clamp(limit ?? 25, 1, 100);
        var totalWarnings = warnings.Count;
        var criticalWarnings = warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Critical);
        var highWarnings = warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.High);
        var mediumWarnings = warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Medium);
        var lowWarnings = warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Low);
        var completenessScore = ComputeCompletenessScore(totalWarnings, criticalWarnings, highWarnings, mediumWarnings, lowWarnings);

        var completeCount = groups.Count(x => x.TotalWarnings == 0 || x.CompletenessScore >= 90);
        var partialCount = groups.Count(x => x.TotalWarnings > 0 && x.CompletenessScore >= 60 && x.CompletenessScore < 90);
        var incompleteCount = groups.Count(x => x.CompletenessScore < 60);

        return new EvidenceCompletenessReportSummaryResponse(
            tenantId,
            groups.Count,
            completeCount,
            partialCount,
            incompleteCount,
            totalWarnings,
            criticalWarnings,
            highWarnings,
            mediumWarnings,
            lowWarnings,
            completenessScore,
            DateTimeOffset.UtcNow,
            groups.Take(cappedLimit).ToList());
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? scopeKey,
        string? rulePackKey,
        string? severity,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(
            tenantId,
            scopeKey,
            rulePackKey,
            severity,
            limit: 100,
            cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(
            "rulePackId,packKey,scopeKey,totalWarnings,criticalWarningCount,highWarningCount,mediumWarningCount,lowWarningCount,completenessScore,completenessLevel,latestWarningAt,summary");

        foreach (var item in summary.RulePacks)
        {
            builder.Append(CsvEscape(item.RulePackId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.ScopeKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.TotalWarnings.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.CriticalWarningCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.HighWarningCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.MediumWarningCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.LowWarningCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.CompletenessScore.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.CompletenessLevel));
            builder.Append(',');
            builder.Append(CsvEscape(item.LatestWarningAt?.ToString("O") ?? string.Empty));
            builder.AppendLine(CsvEscape(item.Summary));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-evidence-completeness-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static int ComputeCompletenessScore(int totalWarnings, int criticalCount, int highCount, int mediumCount, int lowCount)
    {
        if (totalWarnings == 0)
        {
            return 100;
        }

        var penalty = criticalCount * 30 + highCount * 15 + mediumCount * 8 + lowCount * 3;
        return Math.Clamp(100 - penalty, 0, 100);
    }

    private static string DetermineLevel(int score, int totalWarnings) =>
        totalWarnings == 0
            ? "complete"
            : score >= 60
                ? score >= 90
                    ? "complete"
                    : "partial"
                : "incomplete";

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
