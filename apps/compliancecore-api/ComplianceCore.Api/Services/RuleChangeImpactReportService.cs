using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class RuleChangeImpactReportService(ComplianceCoreDbContext db)
{
    public async Task<RuleChangeImpactReportResponse> GetSummaryAsync(
        Guid tenantId,
        string? packKey = null,
        CancellationToken cancellationToken = default)
    {
        var changeEvents = await (
            from evt in db.RuleChangeEvents.AsNoTracking()
            join pack in db.RulePacks.AsNoTracking() on evt.RulePackId equals pack.Id
            join program in db.RegulatoryPrograms.AsNoTracking() on pack.RegulatoryProgramId equals program.Id
            where evt.TenantId == tenantId
                && (string.IsNullOrWhiteSpace(packKey) || evt.PackKey == packKey.Trim().ToLowerInvariant())
            select new
            {
                evt.RulePackId,
                evt.PackKey,
                evt.ProgramKey,
                evt.ChangeType,
                evt.Summary,
                evt.DetectedAt,
            })
            .ToListAsync(cancellationToken);

        var impactedPackIds = changeEvents.Select(x => x.RulePackId).Distinct().ToList();
        var evaluationCounts = await db.RuleEvaluationRuns.AsNoTracking()
            .Where(x => x.TenantId == tenantId && impactedPackIds.Contains(x.RulePackId))
            .GroupBy(x => x.RulePackId)
            .Select(x => new { RulePackId = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var findingCounts = await db.ComplianceFindings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && impactedPackIds.Contains(x.RulePackId))
            .GroupBy(x => x.RulePackId)
            .Select(x => new { RulePackId = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var waiverCounts = await db.ComplianceWaivers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && impactedPackIds.Contains(x.RulePackId))
            .GroupBy(x => x.RulePackId)
            .Select(x => new { RulePackId = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var evaluationLookup = evaluationCounts.ToDictionary(x => x.RulePackId, x => x.Count);
        var findingLookup = findingCounts.ToDictionary(x => x.RulePackId, x => x.Count);
        var waiverLookup = waiverCounts.ToDictionary(x => x.RulePackId, x => x.Count);

        var items = changeEvents
            .GroupBy(x => new { x.RulePackId, x.PackKey, x.ProgramKey })
            .Select(group =>
            {
                var ordered = group.OrderByDescending(x => x.DetectedAt).ToList();
                var latest = ordered[0];
                var versionCreatedCount = group.Count(x => string.Equals(x.ChangeType, RuleChangeTypes.VersionCreated, StringComparison.OrdinalIgnoreCase));
                var statusChangedCount = group.Count(x => string.Equals(x.ChangeType, RuleChangeTypes.StatusChanged, StringComparison.OrdinalIgnoreCase));
                var contentUpdatedCount = group.Count(x => string.Equals(x.ChangeType, RuleChangeTypes.ContentUpdated, StringComparison.OrdinalIgnoreCase));

                evaluationLookup.TryGetValue(group.Key.RulePackId, out var evaluationCount);
                findingLookup.TryGetValue(group.Key.RulePackId, out var findingCount);
                waiverLookup.TryGetValue(group.Key.RulePackId, out var waiverCount);

                return new RuleChangeImpactReportItem(
                    group.Key.RulePackId,
                    group.Key.PackKey,
                    group.Key.ProgramKey,
                    latest.ChangeType,
                    latest.Summary,
                    group.Count(),
                    versionCreatedCount,
                    statusChangedCount,
                    contentUpdatedCount,
                    evaluationCount,
                    findingCount,
                    waiverCount,
                    latest.DetectedAt);
            })
            .OrderByDescending(x => x.LatestChangedAt)
            .ThenBy(x => x.PackKey, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new RuleChangeImpactReportResponse(
            tenantId,
            items.Count,
            items.Sum(x => x.ChangeEventCount),
            items.Sum(x => x.EvaluationRunCount),
            items.Sum(x => x.FindingCount),
            items.Sum(x => x.WaiverCount),
            DateTimeOffset.UtcNow,
            items);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? packKey = null,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, packKey, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("rulePackId,packKey,programKey,latestChangeType,latestSummary,changeEventCount,versionCreatedCount,statusChangedCount,contentUpdatedCount,evaluationRunCount,findingCount,waiverCount,latestChangedAt");

        foreach (var item in summary.RulePacks)
        {
            builder.Append(CsvEscape(item.RulePackId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.ProgramKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.LatestChangeType));
            builder.Append(',');
            builder.Append(CsvEscape(item.LatestSummary));
            builder.Append(',');
            builder.Append(CsvEscape(item.ChangeEventCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.VersionCreatedCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.StatusChangedCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.ContentUpdatedCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.EvaluationRunCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.FindingCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.WaiverCount.ToString()));
            builder.AppendLine(CsvEscape(item.LatestChangedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-rule-change-impact-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
