using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class HazmatTableCoverageReportService(ComplianceCoreDbContext db)
{
    public async Task<HazmatTableCoverageReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var materialKeys = await db.MaterialKeys.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.Key,
                x.Label,
                x.Category,
                x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        var mappings = await db.RegulatoryMappings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.MaterialKeyId.HasValue)
            .GroupBy(x => x.MaterialKeyId!.Value)
            .Select(x => new
            {
                MaterialKeyId = x.Key,
                MappingCount = x.Count(),
                RulePackCount = x.Where(y => y.RulePackId.HasValue).Select(y => y.RulePackId!.Value).Distinct().Count(),
                CitationCount = x.Where(y => y.CitationId.HasValue).Select(y => y.CitationId!.Value).Distinct().Count(),
                HasLookupControl = x.Any(y => string.Equals(y.TargetKind, "material_key", StringComparison.OrdinalIgnoreCase)),
            })
            .ToListAsync(cancellationToken);

        var mappingLookup = mappings.ToDictionary(x => x.MaterialKeyId);
        var citedMaterialIds = await db.RegulatoryMappings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.MaterialKeyId.HasValue && x.CitationId.HasValue)
            .Select(x => x.MaterialKeyId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
        var citationLinkedSet = citedMaterialIds.ToHashSet();

        var items = materialKeys
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .Select(x =>
            {
                mappingLookup.TryGetValue(x.Id, out var mapping);
                var hasMapping = mapping is not null;
                var hasCitationLink = citationLinkedSet.Contains(x.Id);
                var coverageMode = hasMapping
                    ? (hasCitationLink ? "lookup_and_citation" : "lookup_controlled")
                    : (hasCitationLink ? "citation_linked" : "unmapped");

                return new HazmatTableCoverageReportItem(
                    x.Id,
                    x.Key,
                    x.Label,
                    x.Category,
                    coverageMode,
                    mapping?.MappingCount ?? 0,
                    mapping?.RulePackCount ?? 0,
                    mapping?.CitationCount ?? 0,
                    mapping?.HasLookupControl ?? false,
                    hasCitationLink,
                    x.UpdatedAt);
            })
            .ToList();

        return new HazmatTableCoverageReportSummaryResponse(
            tenantId,
            items.Count,
            items.Count(x => x.HasLookupControl),
            items.Count(x => x.HasCitationLink),
            items.Count(x => string.Equals(x.CoverageMode, "unmapped", StringComparison.OrdinalIgnoreCase)),
            items.Sum(x => x.MappingCount),
            items.Sum(x => x.RulePackCount),
            items.Sum(x => x.CitationCount),
            DateTimeOffset.UtcNow,
            items);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("materialKeyId,key,label,category,coverageMode,mappingCount,rulePackCount,citationCount,hasLookupControl,hasCitationLink,updatedAt");

        foreach (var item in summary.MaterialKeys)
        {
            builder.Append(CsvEscape(item.MaterialKeyId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.Key));
            builder.Append(',');
            builder.Append(CsvEscape(item.Label));
            builder.Append(',');
            builder.Append(CsvEscape(item.Category));
            builder.Append(',');
            builder.Append(CsvEscape(item.CoverageMode));
            builder.Append(',');
            builder.Append(CsvEscape(item.MappingCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.CitationCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.HasLookupControl.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.HasCitationLink.ToString().ToLowerInvariant()));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-hazmat-table-coverage-{DateTime.UtcNow:yyyy-MM-dd}.csv",
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
