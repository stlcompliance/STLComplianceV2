using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class RemediationQueueReportService(ComplianceCoreDbContext db)
{
    public const string CsvHeader =
        "warningId,runId,rulePackId,packKey,factKey,warningType,severity,reasonCode,queueState,recommendedAction,hasMirrorAtScope,isRequiredInRule,isRequiredInCatalog,summary,evaluatedAt";

    public async Task<RemediationQueueReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        bool queueOnly,
        string? scopeKey,
        string? rulePackKey,
        string? severity,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var latestRun = await db.MissingEvidenceWarningRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.EvaluatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestRun is null)
        {
            return new RemediationQueueReportSummaryResponse(
                0,
                0,
                0,
                0,
                0,
                0,
                null,
                DateTimeOffset.UtcNow,
                []);
        }

        var cappedLimit = Math.Clamp(limit ?? 12, 1, MissingEvidenceWarningRules.MaxListLimit);
        var query = db.MissingEvidenceWarnings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RunId == latestRun.Id);

        if (!string.IsNullOrWhiteSpace(scopeKey))
        {
            query = query.Where(x => x.ScopeKey == ProductFactMirrorRules.NormalizeScopeKey(scopeKey));
        }

        if (!string.IsNullOrWhiteSpace(rulePackKey))
        {
            var normalizedPack = rulePackKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.PackKey == normalizedPack);
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            var normalizedSeverity = severity.Trim().ToLowerInvariant();
            query = query.Where(x => x.Severity == normalizedSeverity);
        }

        var warnings = await query
            .OrderByDescending(x => MissingEvidenceWarningSeverities.Rank(x.Severity))
            .ThenBy(x => x.PackKey)
            .ThenBy(x => x.FactKey)
            .ToListAsync(cancellationToken);

        if (queueOnly)
        {
            warnings = warnings
                .Where(ShouldQueue)
                .ToList();
        }

        var totalWarnings = warnings.Count;
        var queuedCount = warnings.Count(ShouldQueue);
        var criticalCount = warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Critical);
        var highCount = warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.High);
        var mediumCount = warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Medium);
        var lowCount = warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Low);

        return new RemediationQueueReportSummaryResponse(
            totalWarnings,
            queuedCount,
            criticalCount,
            highCount,
            mediumCount,
            lowCount,
            latestRun.EvaluatedAt,
            DateTimeOffset.UtcNow,
            warnings
                .Take(cappedLimit)
                .Select(MapResponse)
                .ToList());
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        bool queueOnly,
        string? scopeKey,
        string? rulePackKey,
        string? severity,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, queueOnly, scopeKey, rulePackKey, severity, limit: MissingEvidenceWarningRules.MaxListLimit, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(CsvHeader);

        foreach (var item in summary.QueueItems)
        {
            builder.Append(CsvEscape(item.WarningId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RunId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.FactKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.WarningType));
            builder.Append(',');
            builder.Append(CsvEscape(item.Severity));
            builder.Append(',');
            builder.Append(CsvEscape(item.ReasonCode));
            builder.Append(',');
            builder.Append(CsvEscape(item.QueueState));
            builder.Append(',');
            builder.Append(CsvEscape(item.RecommendedAction));
            builder.Append(',');
            builder.Append(CsvEscape(item.HasMirrorAtScope.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.IsRequiredInRule.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.IsRequiredInCatalog.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.Summary));
            builder.AppendLine(CsvEscape(item.EvaluatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-remediation-queue-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static bool ShouldQueue(MissingEvidenceWarning warning) =>
        warning.Severity is MissingEvidenceWarningSeverities.Critical or MissingEvidenceWarningSeverities.High
        || string.Equals(warning.ReasonCode, MissingEvidenceReasonCodes.NoFactDefinition, StringComparison.OrdinalIgnoreCase)
        || string.Equals(warning.ReasonCode, MissingEvidenceReasonCodes.UnresolvedFact, StringComparison.OrdinalIgnoreCase);

    private static RemediationQueueItemResponse MapResponse(MissingEvidenceWarning warning) =>
        new(
            warning.Id,
            warning.RunId,
            warning.RulePackId,
            warning.PackKey,
            warning.FactKey,
            warning.WarningType,
            warning.Severity,
            warning.ReasonCode,
            "open",
            GetRecommendedAction(warning),
            warning.HasMirrorAtScope,
            warning.IsRequiredInRule,
            warning.IsRequiredInCatalog,
            warning.Summary,
            warning.EvaluatedAt);

    private static string GetRecommendedAction(MissingEvidenceWarning warning)
    {
        if (string.Equals(warning.ReasonCode, MissingEvidenceReasonCodes.NoFactDefinition, StringComparison.OrdinalIgnoreCase))
        {
            return "Create the fact definition and register a product source or mirror.";
        }

        if (string.Equals(warning.ReasonCode, MissingEvidenceReasonCodes.UnresolvedFact, StringComparison.OrdinalIgnoreCase))
        {
            return warning.HasMirrorAtScope
                ? "Review the source payload or context so the fact can resolve at runtime."
                : "Create or repair the product source or mirror for this fact at the current scope.";
        }

        if (warning.IsRequiredInRule && !warning.HasMirrorAtScope)
        {
            return "Provision the required mirror or product source at this scope.";
        }

        if (warning.IsRequiredInCatalog && !warning.HasMirrorAtScope)
        {
            return "Provision a catalog-backed mirror or source for this requirement.";
        }

        return "Review the requirement mapping and remediate the missing evidence path.";
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
