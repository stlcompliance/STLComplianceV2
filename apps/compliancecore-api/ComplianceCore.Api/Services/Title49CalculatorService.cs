using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class Title49CalculatorService(ComplianceCoreDbContext db)
{
    private static readonly Regex RetentionDurationRegex = new(
        @"^(?<value>\d+)\s*(?<unit>day|days|week|weeks|month|months|year|years)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<Title49CalculatorSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? sourceProduct = null,
        string? sourceEntity = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.FactRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive);

        if (!string.IsNullOrWhiteSpace(sourceProduct))
        {
            var normalized = sourceProduct.Trim();
            query = query.Where(x => x.SourceProduct.Contains(normalized));
        }

        if (!string.IsNullOrWhiteSpace(sourceEntity))
        {
            var normalized = sourceEntity.Trim();
            query = query.Where(x => x.SourceEntity.Contains(normalized));
        }

        var rows = await (
            from requirement in query.OrderByDescending(x => x.UpdatedAt)
            join definition in db.FactDefinitions.AsNoTracking() on requirement.FactDefinitionId equals definition.Id
            join pack in db.RulePacks.AsNoTracking() on requirement.RulePackId equals pack.Id into packJoin
            from pack in packJoin.DefaultIfEmpty()
            join citation in db.RegulatoryCitations.AsNoTracking() on requirement.CitationId equals citation.Id into citationJoin
            from citation in citationJoin.DefaultIfEmpty()
            select new { requirement, definition, pack, citation })
            .ToListAsync(cancellationToken);

        var items = rows.Select(row => BuildItem(row.requirement, row.definition, row.pack, row.citation)).ToList();

        return new Title49CalculatorSummaryResponse(
            tenantId,
            items.Count,
            items.Count(item => item.ParsedNumericThreshold.HasValue),
            items.Count(item => item.ParsedRetentionDays.HasValue),
            items.Count(item => item.ParsedNumericThreshold.HasValue && item.ParsedRetentionDays.HasValue),
            items.Count(item => item.IsReady),
            items.Count(item => !item.IsReady),
            DateTimeOffset.UtcNow,
            items);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? sourceProduct = null,
        string? sourceEntity = null,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, sourceProduct, sourceEntity, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("factRequirementId,requirementKey,factKey,packKey,citationKey,sourceProduct,sourceEntity,valueType,operator,expectedValue,retentionPeriod,calculatorKind,parsedNumericThreshold,parsedRetentionDays,isReady,updatedAt");

        foreach (var item in summary.Requirements)
        {
            builder.Append(CsvEscape(item.FactRequirementId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RequirementKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.FactKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.CitationKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.SourceProduct));
            builder.Append(',');
            builder.Append(CsvEscape(item.SourceEntity));
            builder.Append(',');
            builder.Append(CsvEscape(item.ValueType));
            builder.Append(',');
            builder.Append(CsvEscape(item.Operator));
            builder.Append(',');
            builder.Append(CsvEscape(item.ExpectedValue));
            builder.Append(',');
            builder.Append(CsvEscape(item.RetentionPeriod));
            builder.Append(',');
            builder.Append(CsvEscape(item.CalculatorKind));
            builder.Append(',');
            builder.Append(CsvEscape(item.ParsedNumericThreshold?.ToString(CultureInfo.InvariantCulture) ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.ParsedRetentionDays?.ToString(CultureInfo.InvariantCulture) ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.IsReady.ToString().ToLowerInvariant()));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-title49-calculators-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static Title49CalculatorItemResponse BuildItem(
        FactRequirement requirement,
        FactDefinition definition,
        RulePack? pack,
        RegulatoryCitation? citation)
    {
        var parsedThreshold = TryParseDecimal(requirement.ExpectedValue, definition.ValueType);
        var parsedRetentionDays = TryParseRetentionDays(requirement.RetentionPeriod);
        var calculatorKind = parsedThreshold.HasValue && parsedRetentionDays.HasValue
            ? "mixed"
            : parsedThreshold.HasValue
                ? "numeric_threshold"
                : parsedRetentionDays.HasValue
                    ? "retention_duration"
                    : "review";
        var isReady = parsedThreshold.HasValue || parsedRetentionDays.HasValue;

        return new Title49CalculatorItemResponse(
            requirement.Id,
            requirement.RequirementKey,
            definition.FactKey,
            pack?.PackKey,
            citation?.CitationKey,
            requirement.SourceProduct,
            requirement.SourceEntity,
            requirement.ValueType,
            requirement.Operator,
            requirement.ExpectedValue,
            requirement.RetentionPeriod,
            calculatorKind,
            parsedThreshold,
            parsedRetentionDays,
            isReady,
            requirement.UpdatedAt);
    }

    private static decimal? TryParseDecimal(string value, string valueType)
    {
        if (!string.Equals(valueType, FactValueTypes.Number, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(valueType, FactValueTypes.Integer, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static int? TryParseRetentionDays(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days))
        {
            return days;
        }

        var match = RetentionDurationRegex.Match(trimmed);
        if (!match.Success)
        {
            return null;
        }

        var amount = int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
        return match.Groups["unit"].Value.ToLowerInvariant() switch
        {
            "day" or "days" => amount,
            "week" or "weeks" => amount * 7,
            "month" or "months" => amount * 30,
            "year" or "years" => amount * 365,
            _ => null
        };
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
