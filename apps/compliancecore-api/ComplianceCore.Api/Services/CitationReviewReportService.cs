using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class CitationReviewReportService(ComplianceCoreDbContext db)
{
    public async Task<CitationReviewReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? reviewState = null,
        string? programKey = null,
        string? rulePackKey = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedReviewState = NormalizeReviewState(reviewState);
        var normalizedProgramKey = NormalizeFilter(programKey);
        var normalizedRulePackKey = NormalizeFilter(rulePackKey);
        var hasProgramFilter = normalizedProgramKey is not null;
        var hasRulePackFilter = normalizedRulePackKey is not null;

        var rows = await (
            from citation in db.RegulatoryCitations.AsNoTracking()
            join program in db.RegulatoryPrograms.AsNoTracking() on citation.RegulatoryProgramId equals program.Id
            join rulePack in db.RulePacks.AsNoTracking() on citation.RulePackId equals rulePack.Id into rulePackJoin
            from rulePack in rulePackJoin.DefaultIfEmpty()
            where citation.TenantId == tenantId
                && (!hasProgramFilter || program.ProgramKey == normalizedProgramKey)
                && (!hasRulePackFilter || (rulePack != null && rulePack.PackKey == normalizedRulePackKey))
            select new
            {
                citation.Id,
                citation.CitationKey,
                citation.SourceReference,
                citation.Label,
                citation.VersionNumber,
                citation.IsActive,
                citation.RulePackId,
                RulePackKey = rulePack != null ? rulePack.PackKey : null,
                RulePackLabel = rulePack != null ? rulePack.Label : null,
                citation.SupersedesCitationId,
                citation.UpdatedAt,
                ProgramKey = program.ProgramKey,
                ProgramLabel = program.Label,
            })
            .ToListAsync(cancellationToken);

        var citationIds = rows.Select(x => x.Id).ToList();
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

        var supersededByCounts = await db.RegulatoryCitations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SupersedesCitationId.HasValue && citationIds.Contains(x.SupersedesCitationId.Value))
            .GroupBy(x => x.SupersedesCitationId!.Value)
            .Select(x => new
            {
                CitationId = x.Key,
                Count = x.Count(),
            })
            .ToListAsync(cancellationToken);

        var referencedCitationIds = rows
            .Where(x => x.SupersedesCitationId.HasValue)
            .Select(x => x.SupersedesCitationId!.Value)
            .Distinct()
            .ToList();
        var referencedCitationKeys = referencedCitationIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.RegulatoryCitations.AsNoTracking()
                .Where(x => x.TenantId == tenantId && referencedCitationIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.CitationKey, cancellationToken);

        var factRequirementLookup = factRequirementCounts.ToDictionary(x => x.CitationId, x => x.Count);
        var mappingLookup = mappingCounts.ToDictionary(x => x.CitationId, x => x.Count);
        var supersededByLookup = supersededByCounts.ToDictionary(x => x.CitationId, x => x.Count);

        var items = rows
            .Select(row =>
            {
                factRequirementLookup.TryGetValue(row.Id, out var factRequirementCount);
                mappingLookup.TryGetValue(row.Id, out var mappingCount);
                supersededByLookup.TryGetValue(row.Id, out var supersededByCount);
                var reviewStateValue = GetReviewState(
                    row.IsActive,
                    row.RulePackId,
                    factRequirementCount,
                    mappingCount,
                    supersededByCount);
                if (normalizedReviewState is not null
                    && !string.Equals(reviewStateValue, normalizedReviewState, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                referencedCitationKeys.TryGetValue(row.SupersedesCitationId ?? Guid.Empty, out var supersedesCitationKey);

                return new CitationReviewReportItem(
                    row.Id,
                    row.CitationKey,
                    row.SourceReference,
                    row.ProgramKey,
                    row.ProgramLabel,
                    row.Label,
                    row.VersionNumber,
                    reviewStateValue,
                    row.IsActive,
                    row.RulePackId.HasValue,
                    row.RulePackKey,
                    row.RulePackLabel,
                    factRequirementCount,
                    mappingCount,
                    supersededByCount,
                    supersedesCitationKey,
                    row.UpdatedAt,
                    BuildSummary(reviewStateValue, row.IsActive, row.RulePackKey, factRequirementCount, mappingCount, supersedesCitationKey, supersededByCount));
            })
            .Where(item => item is not null)
            .Select(item => item!)
            .OrderBy(item => ReviewRank(item.ReviewState))
            .ThenByDescending(item => item.UpdatedAt)
            .ThenBy(item => item.CitationKey, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var filteredTotal = items.Count;
        var limitedItems = limit is > 0 ? items.Take(limit.Value).ToList() : items;

        return new CitationReviewReportSummaryResponse(
            tenantId,
            filteredTotal,
            items.Count(x => x.IsActive),
            items.Count(x => string.Equals(x.ReviewState, "reviewed", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.ReviewState, "needs_review", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.ReviewState, "inactive", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(x.ReviewState, "superseded", StringComparison.OrdinalIgnoreCase)),
            items.Count(x => x.HasRulePack),
            items.Sum(x => x.FactRequirementCount),
            items.Sum(x => x.MappingCount),
            DateTimeOffset.UtcNow,
            limitedItems);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? reviewState = null,
        string? programKey = null,
        string? rulePackKey = null,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(
            tenantId,
            reviewState,
            programKey,
            rulePackKey,
            limit: null,
            cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(
            "citationId,citationKey,sourceReference,programKey,programLabel,citationLabel,versionNumber,reviewState,isActive,hasRulePack,rulePackKey,rulePackLabel,factRequirementCount,mappingCount,supersededByCount,supersedesCitationKey,updatedAt,summary");

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
            builder.Append(CsvEscape(item.ProgramLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.CitationLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.VersionNumber.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.ReviewState));
            builder.Append(',');
            builder.Append(CsvEscape(item.IsActive.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.HasRulePack.ToString().ToLowerInvariant()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackLabel ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.FactRequirementCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.MappingCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.SupersededByCount.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.SupersedesCitationKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.UpdatedAt.ToString("O")));
            builder.AppendLine(CsvEscape(item.Summary));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-citation-review-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static string GetReviewState(
        bool isActive,
        Guid? rulePackId,
        int factRequirementCount,
        int mappingCount,
        int supersededByCount)
    {
        if (!isActive && supersededByCount > 0)
        {
            return "superseded";
        }

        if (!isActive)
        {
            return "inactive";
        }

        if (rulePackId.HasValue && (factRequirementCount > 0 || mappingCount > 0))
        {
            return "reviewed";
        }

        return "needs_review";
    }

    private static int ReviewRank(string reviewState) =>
        reviewState switch
        {
            "needs_review" => 0,
            "inactive" => 1,
            "superseded" => 2,
            "reviewed" => 3,
            _ => 4,
        };

    private static string BuildSummary(
        string reviewState,
        bool isActive,
        string? rulePackKey,
        int factRequirementCount,
        int mappingCount,
        string? supersedesCitationKey,
        int supersededByCount)
    {
        return reviewState switch
        {
            "reviewed" => $"Active citation linked to {factRequirementCount} fact requirement(s) and {mappingCount} mapping(s).",
            "needs_review" when isActive && string.IsNullOrWhiteSpace(rulePackKey) =>
                "Active citation needs rule-pack assignment and downstream review.",
            "needs_review" when isActive =>
                "Active citation is missing downstream rule or evidence linkage.",
            "inactive" => "Inactive citation retained for historical reference.",
            "superseded" when !string.IsNullOrWhiteSpace(supersedesCitationKey) =>
                $"Supersedes {supersedesCitationKey}; newer revision(s) reference this citation {supersededByCount} time(s).",
            "superseded" => $"Superseded by {supersededByCount} newer revision(s).",
            _ => "Citation review item.",
        };
    }

    private static string? NormalizeReviewState(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Trim().Replace('-', '_').ToLowerInvariant();
    }

    private static string? NormalizeFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Trim();
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
