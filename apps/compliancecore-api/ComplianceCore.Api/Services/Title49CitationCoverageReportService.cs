using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class Title49CitationCoverageReportService(ComplianceCoreDbContext db)
{
    public async Task<Title49CitationCoverageReportResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var citations = await (
            from citation in db.RegulatoryCitations.AsNoTracking()
            join program in db.RegulatoryPrograms.AsNoTracking() on citation.RegulatoryProgramId equals program.Id
            join rulePack in db.RulePacks.AsNoTracking() on citation.RulePackId equals rulePack.Id into rulePackJoin
            from rulePack in rulePackJoin.DefaultIfEmpty()
            where citation.TenantId == tenantId
                && EF.Functions.Like(citation.SourceReference, "49 CFR%")
            select new
            {
                citation.Id,
                citation.CitationKey,
                citation.SourceReference,
                citation.Label,
                citation.IsActive,
                citation.RulePackId,
                RulePackHasContent = rulePack != null && !string.IsNullOrWhiteSpace(rulePack.RuleContentJson),
                citation.UpdatedAt,
                ProgramKey = program.ProgramKey,
            })
            .ToListAsync(cancellationToken);

        var citationIds = citations.Select(x => x.Id).ToList();
        var factRequirementCounts = await db.FactRequirements.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CitationId.HasValue && citationIds.Contains(x.CitationId.Value))
            .GroupBy(x => x.CitationId!.Value)
            .Select(x => new
            {
                CitationId = x.Key,
                Count = x.Count(),
            })
            .ToListAsync(cancellationToken);

        var mappingCounts = await db.RegulatoryMappings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CitationId.HasValue && citationIds.Contains(x.CitationId.Value))
            .GroupBy(x => x.CitationId!.Value)
            .Select(x => new
            {
                CitationId = x.Key,
                Count = x.Count(),
            })
            .ToListAsync(cancellationToken);

        var factRequirementLookup = factRequirementCounts.ToDictionary(x => x.CitationId, x => x.Count);
        var mappingLookup = mappingCounts.ToDictionary(x => x.CitationId, x => x.Count);

        var items = citations
            .OrderBy(x => x.SourceReference, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.CitationKey, StringComparer.OrdinalIgnoreCase)
            .Select(x =>
            {
                factRequirementLookup.TryGetValue(x.Id, out var factRequirementCount);
                mappingLookup.TryGetValue(x.Id, out var mappingCount);
                var coverageMode = GetCoverageMode(x.IsActive, x.RulePackId, x.RulePackHasContent);

                return new Title49CitationCoverageReportItemResponse(
                    x.Id,
                    x.CitationKey,
                    x.SourceReference,
                    x.ProgramKey,
                    x.Label,
                    coverageMode,
                    x.IsActive,
                    x.RulePackId.HasValue,
                    x.RulePackId.HasValue ? 1 : 0,
                    factRequirementCount,
                    mappingCount,
                    x.UpdatedAt);
            })
            .ToList();

        return new Title49CitationCoverageReportResponse(
            tenantId,
            items.Count,
            items.Count(x => x.IsActive),
            items.Count(x => string.Equals(x.CoverageMode, "operational", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.CoverageMode, "reference", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.CoverageMode, "unmapped", StringComparison.OrdinalIgnoreCase)),
            items.Sum(x => x.RulePackCount),
            items.Sum(x => x.FactRequirementCount),
            items.Sum(x => x.MappingCount),
            DateTimeOffset.UtcNow,
            items);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("citationId,citationKey,sourceReference,programKey,citationLabel,coverageMode,isActive,hasRulePack,rulePackCount,factRequirementCount,mappingCount,updatedAt");

        foreach (var item in summary.Citations)
        {
            builder.Append(CsvEscape(item.CitationId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.CitationKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.SourceReference));
            builder.Append(',');
            builder.Append(CsvEscape(item.ProgramKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.CitationLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.CoverageMode));
            builder.Append(',');
            builder.Append(CsvEscape(item.IsActive.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.HasRulePack.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.FactRequirementCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.MappingCount.ToString()));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-title49-citation-coverage-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static string GetCoverageMode(bool isActive, Guid? rulePackId, bool rulePackHasContent)
    {
        if (!isActive)
        {
            return "inactive";
        }

        if (rulePackId is null)
        {
            return "unmapped";
        }

        return rulePackHasContent ? "operational" : "reference";
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
