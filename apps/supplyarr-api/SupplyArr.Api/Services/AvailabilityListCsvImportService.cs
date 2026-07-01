using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class AvailabilityListCsvImportService(
    SupplyArrDbContext db,
    AvailabilitySnapshotService availability)
{
    private const string ImportType = "availability_list_csv";

    private static readonly string[] Headers =
    [
        "supplier_key",
        "part_key",
        "snapshot_key",
        "quantity_available",
        "availability_status",
        "effective_from",
        "source",
        "notes"
    ];

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        AvailabilityStatuses.InStock,
        AvailabilityStatuses.Limited,
        AvailabilityStatuses.Backorder,
        AvailabilityStatuses.OutOfStock,
        AvailabilityStatuses.Discontinued
    };

    public async Task<AvailabilityListCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        AvailabilityListCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<AvailabilityListCsvImportIssue>();
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
        var existingSnapshotKeys = await db.PartSupplierAvailabilitySnapshots
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
            await availability.CreateAsync(
                tenantId,
                actorUserId,
                new CreateAvailabilitySnapshotRequest(
                    row.SnapshotKey,
                    links[LinkLookupKey(row)],
                    row.QuantityAvailable,
                    row.AvailabilityStatus,
                    row.EffectiveFrom,
                    row.Source,
                    row.Notes),
                cancellationToken);
            created++;
        }

        return BuildResponse(false, rows.Count, accepted, created, issues);
    }

    private static AvailabilityListCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int availabilityAccepted,
        int availabilityCreated,
        IReadOnlyList<AvailabilityListCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, availabilityAccepted, availabilityCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<AvailabilityListCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new AvailabilityListCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new AvailabilityListCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = NormalizeHeaderFields(ParseRow(lines[0]));
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new AvailabilityListCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new AvailabilityListCsvImportIssue(
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
                ParseOptionalDecimal(fields[3], index + 1, "quantity_available", issues),
                NormalizeKey(fields[4]),
                ParseOptionalDate(fields[5], index + 1, "effective_from", issues),
                NormalizeOptional(fields[6]),
                NormalizeOptional(fields[7])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> links,
        ISet<string> existingSnapshotKeys,
        ISet<string> seenKeys,
        List<AvailabilityListCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "supplier_key", row.SupplierKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "snapshot_key", row.SnapshotKey, 1, 128, issues);
        ValidateLength(row.LineNumber, "availability_status", row.AvailabilityStatus, 1, 32, issues);
        if (!links.ContainsKey(LinkLookupKey(row)))
        {
            issues.Add(new AvailabilityListCsvImportIssue(row.LineNumber, "supplier_link.not_found", "Supplier/part link was not found."));
        }

        if (row.QuantityAvailable is < 0)
        {
            issues.Add(new AvailabilityListCsvImportIssue(row.LineNumber, "availability.quantity_invalid", "quantity_available cannot be negative."));
        }

        if (!AllowedStatuses.Contains(row.AvailabilityStatus))
        {
            issues.Add(new AvailabilityListCsvImportIssue(row.LineNumber, "availability.status_invalid", "availability_status must be in_stock, limited, backorder, out_of_stock, or discontinued."));
        }

        if (existingSnapshotKeys.Contains(row.SnapshotKey))
        {
            issues.Add(new AvailabilityListCsvImportIssue(row.LineNumber, "snapshot.duplicate", "Snapshot key already exists."));
        }

        if (!seenKeys.Add(row.SnapshotKey))
        {
            issues.Add(new AvailabilityListCsvImportIssue(row.LineNumber, "snapshot.duplicate_in_file", "Snapshot key appears more than once in the import file."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<AvailabilityListCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new AvailabilityListCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static decimal? ParseOptionalDecimal(
        string value,
        int lineNumber,
        string column,
        List<AvailabilityListCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return decimal.Round(parsed, 4, MidpointRounding.AwayFromZero);
        }

        issues.Add(new AvailabilityListCsvImportIssue(lineNumber, "csv.decimal", $"{column} must be a decimal number."));
        return null;
    }

    private static DateTimeOffset? ParseOptionalDate(
        string value,
        int lineNumber,
        string column,
        List<AvailabilityListCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        issues.Add(new AvailabilityListCsvImportIssue(lineNumber, "csv.date", $"{column} must be a date or timestamp."));
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
        decimal? QuantityAvailable,
        string AvailabilityStatus,
        DateTimeOffset? EffectiveFrom,
        string Source,
        string Notes);
}

