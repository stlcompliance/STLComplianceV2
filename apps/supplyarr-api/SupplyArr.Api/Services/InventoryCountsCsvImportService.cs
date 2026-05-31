using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class InventoryCountsCsvImportService(
    SupplyArrDbContext db,
    PartStockService stock)
{
    private const string ImportType = "inventory_counts_csv";

    private static readonly string[] Headers =
    [
        "location_key",
        "bin_key",
        "part_key",
        "quantity_on_hand"
    ];

    public async Task<InventoryCountsCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        InventoryCountsCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<InventoryCountsCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var partsByKey = await db.Parts
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.PartKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var binsByKey = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(
                x => $"{x.InventoryLocation!.LocationKey}|{x.BinKey}",
                x => x.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        var seenCounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accepted = 0;
        foreach (var row in rows)
        {
            ValidateRow(row, partsByKey, binsByKey, seenCounts, issues);
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

        var applied = 0;
        foreach (var row in rows)
        {
            await stock.UpsertAsync(
                tenantId,
                actorUserId,
                new UpsertPartStockLevelRequest(
                    partsByKey[row.PartKey],
                    binsByKey[BinLookupKey(row)],
                    row.QuantityOnHand),
                cancellationToken);
            applied++;
        }

        return BuildResponse(false, rows.Count, accepted, applied, issues);
    }

    private static InventoryCountsCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int countsAccepted,
        int countsApplied,
        IReadOnlyList<InventoryCountsCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, countsAccepted, countsApplied, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<InventoryCountsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new InventoryCountsCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new InventoryCountsCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new InventoryCountsCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new InventoryCountsCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {Headers.Length} columns but found {fields.Count}."));
                continue;
            }

            rows.Add(new ImportRow(
                index + 1,
                NormalizeKey(fields[0]),
                NormalizeKey(fields[1]),
                NormalizeKey(fields[2]),
                ParseQuantity(fields[3], index + 1, issues)));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> partsByKey,
        IReadOnlyDictionary<string, Guid> binsByKey,
        ISet<string> seenCounts,
        List<InventoryCountsCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "location_key", row.LocationKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "bin_key", row.BinKey, 1, 128, issues);
        ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
        if (row.QuantityOnHand < 0)
        {
            issues.Add(new InventoryCountsCsvImportIssue(row.LineNumber, "inventory.quantity", "quantity_on_hand cannot be negative."));
        }

        if (!partsByKey.ContainsKey(row.PartKey))
        {
            issues.Add(new InventoryCountsCsvImportIssue(row.LineNumber, "part.not_found", "Part key was not found."));
        }

        if (!binsByKey.ContainsKey(BinLookupKey(row)))
        {
            issues.Add(new InventoryCountsCsvImportIssue(row.LineNumber, "bin.not_found", "Location/bin key pair was not found."));
        }

        if (!seenCounts.Add($"{row.LocationKey}|{row.BinKey}|{row.PartKey}"))
        {
            issues.Add(new InventoryCountsCsvImportIssue(row.LineNumber, "count.duplicate_in_file", "The same part and bin appear more than once in the import file."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<InventoryCountsCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new InventoryCountsCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static decimal ParseQuantity(
        string value,
        int lineNumber,
        List<InventoryCountsCsvImportIssue> issues)
    {
        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return decimal.Round(parsed, 4, MidpointRounding.AwayFromZero);
        }

        issues.Add(new InventoryCountsCsvImportIssue(lineNumber, "csv.decimal", "quantity_on_hand must be a decimal number."));
        return 0;
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

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    private static string BinLookupKey(ImportRow row) => $"{row.LocationKey}|{row.BinKey}";

    private sealed record ImportRow(
        int LineNumber,
        string LocationKey,
        string BinKey,
        string PartKey,
        decimal QuantityOnHand);
}
