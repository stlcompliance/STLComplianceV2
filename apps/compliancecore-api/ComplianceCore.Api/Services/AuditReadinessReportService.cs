using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class AuditReadinessReportService(ComplianceCoreDbContext db)
{
    public const string CsvHeader =
        "forecastId,runId,scopeKey,rulePackId,packKey,readinessScore,readinessLevel,riskScore,riskLevel,effectivenessScore,effectivenessLevel,missingEvidenceWarningCount,highestMissingEvidenceSeverity,summary,forecastedAt";

    public async Task<AuditReadinessReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? scopeKey,
        string? rulePackKey,
        string? readinessLevel,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var latestRun = await db.ReadinessForecastRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ForecastedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestRun is null)
        {
            return new AuditReadinessReportSummaryResponse(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                ReadinessForecastLevels.Unknown,
                0,
                0,
                null,
                DateTimeOffset.UtcNow,
                []);
        }

        var cappedLimit = Math.Clamp(limit ?? 25, 1, ReadinessForecastRules.MaxListLimit);
        var query = db.ReadinessForecasts
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

        if (!string.IsNullOrWhiteSpace(readinessLevel))
        {
            var normalizedLevel = readinessLevel.Trim().ToLowerInvariant();
            query = query.Where(x => x.ReadinessLevel == normalizedLevel);
        }

        var forecasts = await query
            .OrderBy(x => x.ReadinessScore)
            .ThenBy(x => x.PackKey)
            .ToListAsync(cancellationToken);

        var total = forecasts.Count;
        var scopesTracked = forecasts.Select(x => x.ScopeKey).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var readyCount = forecasts.Count(x => x.ReadinessLevel == ReadinessForecastLevels.Ready);
        var cautionCount = forecasts.Count(x => x.ReadinessLevel == ReadinessForecastLevels.Caution);
        var notReadyCount = forecasts.Count(x => x.ReadinessLevel == ReadinessForecastLevels.NotReady);
        var unknownCount = forecasts.Count(x => x.ReadinessLevel == ReadinessForecastLevels.Unknown);
        var readinessScore = total == 0 ? 0 : (int)Math.Round(forecasts.Average(x => x.ReadinessScore));
        var overallLevel = total == 0
            ? ReadinessForecastLevels.Unknown
            : forecasts.Select(x => x.ReadinessLevel).Aggregate(ReadinessForecastLevels.Ready, ReadinessForecastLevels.Min);
        var lowestScore = total == 0 ? 0 : forecasts.Min(x => x.ReadinessScore);
        var averageScore = total == 0 ? 0 : (int)Math.Round(forecasts.Average(x => x.ReadinessScore));

        return new AuditReadinessReportSummaryResponse(
            total,
            scopesTracked,
            readyCount,
            cautionCount,
            notReadyCount,
            unknownCount,
            readinessScore,
            overallLevel,
            lowestScore,
            averageScore,
            latestRun.ForecastedAt,
            DateTimeOffset.UtcNow,
            forecasts
                .Take(cappedLimit)
                .Select(MapResponse)
                .ToList());
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? scopeKey,
        string? rulePackKey,
        string? readinessLevel,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(
            tenantId,
            scopeKey,
            rulePackKey,
            readinessLevel,
            limit: ReadinessForecastRules.MaxListLimit,
            cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(CsvHeader);
        foreach (var forecast in summary.Forecasts)
        {
            builder.Append(CsvEscape(forecast.ForecastId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.RunId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.ScopeKey));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.RulePackId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.ReadinessScore.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.ReadinessLevel));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.RiskScore.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.RiskLevel));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.EffectivenessScore.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.EffectivenessLevel));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.MissingEvidenceWarningCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.HighestMissingEvidenceSeverity));
            builder.Append(',');
            builder.Append(CsvEscape(forecast.Summary));
            builder.AppendLine(CsvEscape(forecast.ForecastedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-audit-readiness-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static ReadinessForecastResponse MapResponse(ReadinessForecast entity) =>
        new(
            entity.Id,
            entity.RunId,
            entity.ScopeKey,
            entity.RulePackId,
            entity.PackKey,
            entity.ReadinessScore,
            entity.ReadinessLevel,
            entity.RiskScore,
            entity.RiskLevel,
            entity.EffectivenessScore,
            entity.EffectivenessLevel,
            entity.MissingEvidenceWarningCount,
            entity.HighestMissingEvidenceSeverity,
            entity.Summary,
            entity.ForecastedAt);

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
