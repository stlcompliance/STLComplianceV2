using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class PriceListCsvImportService(
    SupplyArrDbContext db,
    PricingSnapshotService pricing)
{
    private const string ImportType = "price_list_csv";

    private static readonly string[] Headers =
    [
        "vendor_party_key",
        "part_key",
        "snapshot_key",
        "unit_price",
        "currency_code",
        "minimum_order_quantity",
        "effective_from",
        "source",
        "notes"
    ];

    public async Task<PriceListCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        PriceListCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<PriceListCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var links = await db.PartVendorLinks
            .Include(x => x.Part)
            .Include(x => x.ExternalParty)
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(
                x => $"{x.ExternalParty!.PartyKey}|{x.Part!.PartKey}",
                x => x.Id,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);
        var existingSnapshotKeys = await db.PartVendorPricingSnapshots
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
            await pricing.CreateAsync(
                tenantId,
                actorUserId,
                new CreatePricingSnapshotRequest(
                    row.SnapshotKey,
                    links[LinkLookupKey(row)],
                    row.UnitPrice,
                    row.CurrencyCode,
                    row.MinimumOrderQuantity,
                    row.EffectiveFrom,
                    row.Source,
                    row.Notes),
                cancellationToken);
            created++;
        }

        return BuildResponse(false, rows.Count, accepted, created, issues);
    }

    private static PriceListCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int pricesAccepted,
        int pricesCreated,
        IReadOnlyList<PriceListCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, pricesAccepted, pricesCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<PriceListCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new PriceListCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new PriceListCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new PriceListCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new PriceListCsvImportIssue(
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
                ParseRequiredDecimal(fields[3], index + 1, "unit_price", issues),
                NormalizeOptional(fields[4]),
                ParseOptionalDecimal(fields[5], index + 1, "minimum_order_quantity", issues),
                ParseOptionalDate(fields[6], index + 1, "effective_from", issues),
                NormalizeOptional(fields[7]),
                NormalizeOptional(fields[8])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> links,
        ISet<string> existingSnapshotKeys,
        ISet<string> seenKeys,
        List<PriceListCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "vendor_party_key", row.VendorPartyKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "snapshot_key", row.SnapshotKey, 1, 128, issues);
        if (!links.ContainsKey(LinkLookupKey(row)))
        {
            issues.Add(new PriceListCsvImportIssue(row.LineNumber, "vendor_link.not_found", "Vendor/part link was not found."));
        }

        if (row.UnitPrice <= 0)
        {
            issues.Add(new PriceListCsvImportIssue(row.LineNumber, "price.invalid", "unit_price must be greater than zero."));
        }

        if (row.MinimumOrderQuantity is <= 0)
        {
            issues.Add(new PriceListCsvImportIssue(row.LineNumber, "minimum_order_quantity.invalid", "minimum_order_quantity must be greater than zero."));
        }

        if (existingSnapshotKeys.Contains(row.SnapshotKey))
        {
            issues.Add(new PriceListCsvImportIssue(row.LineNumber, "snapshot.duplicate", "Snapshot key already exists."));
        }

        if (!seenKeys.Add(row.SnapshotKey))
        {
            issues.Add(new PriceListCsvImportIssue(row.LineNumber, "snapshot.duplicate_in_file", "Snapshot key appears more than once in the import file."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<PriceListCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new PriceListCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static decimal ParseRequiredDecimal(
        string value,
        int lineNumber,
        string column,
        List<PriceListCsvImportIssue> issues)
    {
        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return decimal.Round(parsed, 4, MidpointRounding.AwayFromZero);
        }

        issues.Add(new PriceListCsvImportIssue(lineNumber, "csv.decimal", $"{column} must be a decimal number."));
        return 0;
    }

    private static decimal? ParseOptionalDecimal(
        string value,
        int lineNumber,
        string column,
        List<PriceListCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return ParseRequiredDecimal(value, lineNumber, column, issues);
    }

    private static DateTimeOffset? ParseOptionalDate(
        string value,
        int lineNumber,
        string column,
        List<PriceListCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        issues.Add(new PriceListCsvImportIssue(lineNumber, "csv.date", $"{column} must be a date or timestamp."));
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

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string LinkLookupKey(ImportRow row) => $"{row.VendorPartyKey}|{row.PartKey}";

    private sealed record ImportRow(
        int LineNumber,
        string VendorPartyKey,
        string PartKey,
        string SnapshotKey,
        decimal UnitPrice,
        string CurrencyCode,
        decimal? MinimumOrderQuantity,
        DateTimeOffset? EffectiveFrom,
        string Source,
        string Notes);
}
