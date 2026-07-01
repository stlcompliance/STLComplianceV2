using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class LeadTimeListCsvImportService(
    SupplyArrDbContext db,
    LeadTimeSnapshotService leadTimes)
{
    private const string ImportType = "lead_time_list_csv";

    private static readonly string[] Headers =
    [
        "supplier_key",
        "part_key",
        "snapshot_key",
        "lead_time_days",
        "effective_from",
        "source",
        "notes"
    ];

    public async Task<LeadTimeListCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        LeadTimeListCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<LeadTimeListCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var links = await db.PartSupplierLinks
            .Include(x => x.Part)
            .Include(x => x.Supplier)
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(
                x => $"{x.Supplier!.SupplierKey}|{x.Part!.PartKey}",
                x => x.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);
        var existingSnapshotKeys = await db.PartSupplierLeadTimeSnapshots
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.SnapshotKey)
            .ToListAsync(cancellationToken);
        var existingKeys = existingSnapshotKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accepted = 0;

        foreach (var row in rows)
        {
            ValidateRow(row, links, existingKeys, seenKeys, issues);
            if (issues.Any(x => x.LineNumber == row.LineNumber))
            {
                continue;
            }

            accepted++;
        }

        if (issues.Count > 0 || request.DryRun)
        {
            return BuildResponse(request.DryRun, rows.Count, accepted, 0, issues);
        }

        var created = 0;
        foreach (var row in rows)
        {
            await leadTimes.CreateAsync(
                tenantId,
                actorUserId,
                new CreateLeadTimeSnapshotRequest(
                    row.SnapshotKey,
                    links[LinkLookupKey(row)],
                    row.LeadTimeDays,
                    row.EffectiveFrom,
                    row.Source,
                    row.Notes),
                cancellationToken);
            created++;
        }

        return BuildResponse(false, rows.Count, accepted, created, issues);
    }

    private static LeadTimeListCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int leadTimesAccepted,
        int leadTimesCreated,
        IReadOnlyList<LeadTimeListCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, leadTimesAccepted, leadTimesCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<LeadTimeListCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new LeadTimeListCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new LeadTimeListCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = NormalizeHeaderFields(ParseRow(lines[0]));
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new LeadTimeListCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new LeadTimeListCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {Headers.Length} columns but found {fields.Count}."));
                continue;
            }

            rows.Add(new ImportRow(
                index + 1,
                NormalizeKey(fields[0]),
                NormalizeKey(fields[1]),
                NormalizeOptional(fields[2]),
                ParseLeadTimeDays(fields[3], index + 1, issues),
                ParseOptionalDate(fields[4], index + 1, "effective_from", issues),
                NormalizeOptional(fields[5]),
                NormalizeOptional(fields[6])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> links,
        ISet<string> existingSnapshotKeys,
        ISet<string> seenKeys,
        List<LeadTimeListCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "supplier_key", row.SupplierKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "snapshot_key", row.SnapshotKey, 1, 128, issues);
        if (!links.ContainsKey(LinkLookupKey(row)))
        {
            issues.Add(new LeadTimeListCsvImportIssue(row.LineNumber, "supplier_link.not_found", "Supplier/part link was not found."));
        }

        if (row.LeadTimeDays < 0)
        {
            issues.Add(new LeadTimeListCsvImportIssue(row.LineNumber, "lead_time.invalid", "lead_time_days cannot be negative."));
        }

        if (existingSnapshotKeys.Contains(row.SnapshotKey))
        {
            issues.Add(new LeadTimeListCsvImportIssue(row.LineNumber, "snapshot.duplicate", "Snapshot key already exists."));
        }

        if (!seenKeys.Add(row.SnapshotKey))
        {
            issues.Add(new LeadTimeListCsvImportIssue(row.LineNumber, "snapshot.duplicate_in_file", "Snapshot key appears more than once in the import file."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<LeadTimeListCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new LeadTimeListCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static int ParseLeadTimeDays(
        string value,
        int lineNumber,
        List<LeadTimeListCsvImportIssue> issues)
    {
        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        issues.Add(new LeadTimeListCsvImportIssue(lineNumber, "csv.integer", "lead_time_days must be an integer."));
        return 0;
    }

    private static DateTimeOffset? ParseOptionalDate(
        string value,
        int lineNumber,
        string column,
        List<LeadTimeListCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        issues.Add(new LeadTimeListCsvImportIssue(lineNumber, "csv.date", $"{column} must be a date or timestamp."));
        return null;
    }

    private static IReadOnlyList<string> ParseRow(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(character);
                }

                continue;
            }

            if (character == '"')
            {
                inQuotes = true;
                continue;
            }

            if (character == ',')
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        fields.Add(current.ToString().Trim());
        return fields;
    }

    private static IReadOnlyList<string> NormalizeHeaderFields(IReadOnlyList<string> headerFields) =>
        headerFields.ToList();

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string LinkLookupKey(ImportRow row) => $"{row.SupplierKey}|{row.PartKey}";

    private sealed record ImportRow(
        int LineNumber,
        string SupplierKey,
        string PartKey,
        string SnapshotKey,
        int LeadTimeDays,
        DateTimeOffset? EffectiveFrom,
        string Source,
        string Notes);
}

