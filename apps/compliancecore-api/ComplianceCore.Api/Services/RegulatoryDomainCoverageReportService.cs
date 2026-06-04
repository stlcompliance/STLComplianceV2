using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class RegulatoryDomainCoverageReportService(ComplianceCoreDbContext db)
{
    public async Task<RegulatoryDomainCoverageReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var programs = await (
            from program in db.RegulatoryPrograms.AsNoTracking()
            join jurisdiction in db.Jurisdictions.AsNoTracking() on program.JurisdictionId equals jurisdiction.Id
            join governingBody in db.GoverningBodies.AsNoTracking() on jurisdiction.GoverningBodyId equals governingBody.Id
            where program.TenantId == tenantId
            select new
            {
                program.Id,
                program.ProgramKey,
                program.Label,
                program.UpdatedAt,
                JurisdictionKey = jurisdiction.JurisdictionKey,
                JurisdictionLabel = jurisdiction.Label,
                GoverningBodyKey = governingBody.BodyKey,
                GoverningBodyLabel = governingBody.Label,
            })
            .ToListAsync(cancellationToken);

        var rulePackCounts = await db.RulePacks.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.RegulatoryProgramId)
            .Select(x => new
            {
                ProgramId = x.Key,
                RulePackCount = x.Count(),
                OperationalRulePackCount = x.Count(pack => !string.IsNullOrWhiteSpace(pack.RuleContentJson)),
                ReferenceRulePackCount = x.Count(pack => string.IsNullOrWhiteSpace(pack.RuleContentJson)),
                LatestUpdatedAt = x.Max(pack => pack.UpdatedAt),
            })
            .ToListAsync(cancellationToken);

        var citationCounts = await db.RegulatoryCitations.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.RegulatoryProgramId)
            .Select(x => new
            {
                ProgramId = x.Key,
                CitationCount = x.Count(),
            })
            .ToListAsync(cancellationToken);

        var mappingCounts = await db.RegulatoryMappings.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.RegulatoryProgramId)
            .Select(x => new
            {
                ProgramId = x.Key,
                MappingCount = x.Count(),
            })
            .ToListAsync(cancellationToken);

        var packCountsByProgram = rulePackCounts.ToDictionary(x => x.ProgramId);
        var citationsByProgram = citationCounts.ToDictionary(x => x.ProgramId, x => x.CitationCount);
        var mappingsByProgram = mappingCounts.ToDictionary(x => x.ProgramId, x => x.MappingCount);

        var items = programs
            .OrderBy(x => x.GoverningBodyKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.JurisdictionKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ProgramKey, StringComparer.OrdinalIgnoreCase)
            .Select(x =>
            {
                packCountsByProgram.TryGetValue(x.Id, out var packCount);
                citationsByProgram.TryGetValue(x.Id, out var citationCount);
                mappingsByProgram.TryGetValue(x.Id, out var mappingCount);
                var coverageMode = GetCoverageMode(packCount?.OperationalRulePackCount ?? 0, packCount?.ReferenceRulePackCount ?? 0);

                return new RegulatoryDomainCoverageReportItem(
                    x.Id,
                    x.ProgramKey,
                    x.Label,
                    x.GoverningBodyKey,
                    x.GoverningBodyLabel,
                    x.JurisdictionKey,
                    x.JurisdictionLabel,
                    coverageMode,
                    packCount?.RulePackCount ?? 0,
                    packCount?.OperationalRulePackCount ?? 0,
                    packCount?.ReferenceRulePackCount ?? 0,
                    citationCount,
                    mappingCount,
                    packCount is null ? x.UpdatedAt : (packCount.LatestUpdatedAt > x.UpdatedAt ? packCount.LatestUpdatedAt : x.UpdatedAt));
            })
            .ToList();

        var totalRulePacks = items.Sum(x => x.RulePackCount);
        var totalOperationalPacks = items.Sum(x => x.OperationalRulePackCount);
        var totalReferencePacks = items.Sum(x => x.ReferenceRulePackCount);
        var totalCitations = items.Sum(x => x.CitationCount);
        var totalMappings = items.Sum(x => x.MappingCount);

        return new RegulatoryDomainCoverageReportSummaryResponse(
            tenantId,
            items.Count,
            items.Count(x => string.Equals(x.CoverageMode, "operational", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.CoverageMode, "reference", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.CoverageMode, "mixed", StringComparison.OrdinalIgnoreCase)),
            totalRulePacks,
            totalOperationalPacks,
            totalReferencePacks,
            totalCitations,
            totalMappings,
            DateTimeOffset.UtcNow,
            items);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "regulatoryProgramId,programKey,programLabel,governingBodyKey,governingBodyLabel,jurisdictionKey,jurisdictionLabel,coverageMode,rulePackCount,operationalRulePackCount,referenceRulePackCount,citationCount,mappingCount,updatedAt");

        foreach (var item in summary.Programs)
        {
            builder.Append(CsvEscape(item.RegulatoryProgramId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.ProgramKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.ProgramLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.GoverningBodyKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.GoverningBodyLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.JurisdictionKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.JurisdictionLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.CoverageMode));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.OperationalRulePackCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.ReferenceRulePackCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.CitationCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.MappingCount.ToString()));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-regulatory-domain-coverage-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static string GetCoverageMode(int operationalRulePackCount, int referenceRulePackCount) =>
        operationalRulePackCount > 0
            ? (referenceRulePackCount > 0 ? "mixed" : "operational")
            : (referenceRulePackCount > 0 ? "reference" : "unmapped");

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
