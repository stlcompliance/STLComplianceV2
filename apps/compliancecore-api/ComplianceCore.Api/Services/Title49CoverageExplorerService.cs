using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class Title49CoverageExplorerService(ComplianceCoreDbContext db)
{
    public async Task<Title49CoverageExplorerResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var packs = await (
            from pack in db.RulePacks.AsNoTracking()
            join program in db.RegulatoryPrograms.AsNoTracking() on pack.RegulatoryProgramId equals program.Id
            where pack.TenantId == tenantId && EF.Functions.Like(pack.PackKey, "title49%")
            select new
            {
                pack.Id,
                pack.PackKey,
                pack.Label,
                pack.RuleContentJson,
                pack.UpdatedAt,
                ProgramKey = program.ProgramKey,
            })
            .ToListAsync(cancellationToken);

        var packIds = packs.Select(x => x.Id).ToList();
        var citationCounts = await db.RegulatoryCitations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RulePackId.HasValue && packIds.Contains(x.RulePackId.Value))
            .GroupBy(x => x.RulePackId!.Value)
            .Select(x => new { RulePackId = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var factCounts = await db.FactRequirements.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RulePackId.HasValue && packIds.Contains(x.RulePackId.Value))
            .GroupBy(x => x.RulePackId!.Value)
            .Select(x => new { RulePackId = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var citationsByPack = citationCounts.ToDictionary(x => x.RulePackId, x => x.Count);
        var factsByPack = factCounts.ToDictionary(x => x.RulePackId, x => x.Count);

        var items = packs
            .OrderBy(x => x.PackKey, StringComparer.OrdinalIgnoreCase)
            .Select(x =>
            {
                citationsByPack.TryGetValue(x.Id, out var citationCount);
                factsByPack.TryGetValue(x.Id, out var factCount);
                var coverageKind = GetCoverageKind(x.PackKey, x.RuleContentJson);

                return new Title49CoverageExplorerItemResponse(
                    x.Id,
                    x.PackKey,
                    x.ProgramKey,
                    x.Label,
                    coverageKind,
                    !string.IsNullOrWhiteSpace(x.RuleContentJson),
                    citationCount,
                    factCount,
                    x.UpdatedAt);
            })
            .ToList();

        return new Title49CoverageExplorerResponse(
            tenantId,
            items.Count,
            items.Count(x => string.Equals(x.CoverageKind, "operational", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.CoverageKind, "reference", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.CoverageKind, "metadata", StringComparison.OrdinalIgnoreCase)),
            items.Sum(x => x.CitationCount),
            items.Sum(x => x.FactRequirementCount),
            DateTimeOffset.UtcNow,
            items);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("rulePackId,packKey,programKey,packLabel,coverageKind,hasContent,citationCount,factRequirementCount,updatedAt");

        foreach (var item in summary.RulePacks)
        {
            builder.Append(CsvEscape(item.RulePackId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.ProgramKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.CoverageKind));
            builder.Append(',');
            builder.Append(CsvEscape(item.HasContent.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.CitationCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.FactRequirementCount.ToString()));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-title49-coverage-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static string GetCoverageKind(string packKey, string? ruleContentJson)
    {
        if (packKey.EndsWith("_metadata", StringComparison.OrdinalIgnoreCase))
        {
            return "metadata";
        }

        if (packKey.EndsWith("_reference", StringComparison.OrdinalIgnoreCase))
        {
            return "reference";
        }

        return string.IsNullOrWhiteSpace(ruleContentJson) ? "reference" : "operational";
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
