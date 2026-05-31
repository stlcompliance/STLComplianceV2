using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class VendorCatalogCsvImportService(
    SupplyArrDbContext db,
    PartRegistryService parts)
{
    private const string ImportType = "vendor_catalog_csv";

    private static readonly string[] Headers =
    [
        "vendor_party_key",
        "part_key",
        "vendor_part_number",
        "is_preferred",
        "catalog_unit_price",
        "catalog_currency_code",
        "catalog_minimum_order_quantity",
        "catalog_lead_time_days",
        "catalog_quantity_available",
        "catalog_availability_status"
    ];

    public async Task<VendorCatalogCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        VendorCatalogCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<VendorCatalogCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var vendors = await db.ExternalParties
            .Where(x => x.TenantId == tenantId)
            .Where(x => x.PartyType == "vendor" || x.PartyType == "supplier")
            .ToDictionaryAsync(x => x.PartyKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var partIds = await db.Parts
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.PartKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var existingLinks = await db.PartVendorLinks
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.PartId, x.ExternalPartyId })
            .ToListAsync(cancellationToken);
        var existingLinkKeys = existingLinks
            .Select(x => LinkKey(x.PartId, x.ExternalPartyId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seenLinkKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var acceptedLinks = 0;
        foreach (var row in rows)
        {
            ValidateRow(row, vendors, partIds, existingLinkKeys, seenLinkKeys, issues);
            if (issues.All(x => x.LineNumber != row.LineNumber))
            {
                acceptedLinks++;
            }
        }

        if (issues.Count > 0 || request.DryRun)
        {
            return BuildResponse(request.DryRun, rows.Count, acceptedLinks, 0, issues);
        }

        var createdLinks = 0;
        foreach (var row in rows)
        {
            var partId = partIds[row.PartKey];
            var vendorId = vendors[row.VendorPartyKey];
            var link = await parts.AddVendorLinkAsync(
                tenantId,
                actorUserId,
                partId,
                new CreatePartVendorLinkRequest(vendorId, row.VendorPartNumber, row.IsPreferred),
                cancellationToken);

            if (row.CatalogUnitPrice is not null)
            {
                await parts.UpsertVendorLinkCatalogPriceAsync(
                    tenantId,
                    actorUserId,
                    partId,
                    link.LinkId,
                    new UpsertPartVendorLinkCatalogPriceRequest(
                        row.CatalogUnitPrice.Value,
                        row.CatalogCurrencyCode,
                        row.CatalogMinimumOrderQuantity),
                    cancellationToken);
            }

            if (row.CatalogLeadTimeDays is not null)
            {
                await parts.UpsertVendorLinkCatalogLeadTimeAsync(
                    tenantId,
                    actorUserId,
                    partId,
                    link.LinkId,
                    new UpsertPartVendorLinkCatalogLeadTimeRequest(row.CatalogLeadTimeDays.Value),
                    cancellationToken);
            }

            if (row.CatalogQuantityAvailable is not null || !string.IsNullOrWhiteSpace(row.CatalogAvailabilityStatus))
            {
                await parts.UpsertVendorLinkCatalogAvailabilityAsync(
                    tenantId,
                    actorUserId,
                    partId,
                    link.LinkId,
                    new UpsertPartVendorLinkCatalogAvailabilityRequest(
                        row.CatalogQuantityAvailable,
                        row.CatalogAvailabilityStatus),
                    cancellationToken);
            }

            createdLinks++;
        }

        return BuildResponse(false, rows.Count, acceptedLinks, createdLinks, issues);
    }

    private static VendorCatalogCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int linksAccepted,
        int linksCreated,
        IReadOnlyList<VendorCatalogCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, linksAccepted, linksCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<VendorCatalogCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new VendorCatalogCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new VendorCatalogCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new VendorCatalogCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new VendorCatalogCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {Headers.Length} columns but found {fields.Count}."));
                continue;
            }

            var lineNumber = index + 1;
            var unitPrice = ParseOptionalDecimal(lineNumber, "catalog_unit_price", fields[4], allowZero: false, issues);
            var minimumOrderQuantity = ParseOptionalDecimal(lineNumber, "catalog_minimum_order_quantity", fields[6], allowZero: false, issues);
            var leadTimeDays = ParseOptionalInt(lineNumber, "catalog_lead_time_days", fields[7], issues);
            var quantityAvailable = ParseOptionalDecimal(lineNumber, "catalog_quantity_available", fields[8], allowZero: true, issues);
            rows.Add(new ImportRow(
                lineNumber,
                NormalizeKey(fields[0]),
                NormalizeKey(fields[1]),
                NormalizeOptional(fields[2]),
                ParseBool(fields[3]),
                unitPrice,
                NormalizeCurrency(fields[5]),
                minimumOrderQuantity,
                leadTimeDays,
                quantityAvailable,
                NormalizeOptional(fields[9])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> vendors,
        IReadOnlyDictionary<string, Guid> partIds,
        ISet<string> existingLinkKeys,
        ISet<string> seenLinkKeys,
        List<VendorCatalogCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "vendor_party_key", row.VendorPartyKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "vendor_part_number", row.VendorPartNumber, 1, 128, issues);
        ValidateMaxLength(row.LineNumber, "catalog_availability_status", row.CatalogAvailabilityStatus, 64, issues);

        var hasVendor = vendors.TryGetValue(row.VendorPartyKey, out var vendorId);
        if (!hasVendor)
        {
            issues.Add(new VendorCatalogCsvImportIssue(row.LineNumber, "vendor.not_found", "Vendor or supplier party was not found."));
        }

        var hasPart = partIds.TryGetValue(row.PartKey, out var partId);
        if (!hasPart)
        {
            issues.Add(new VendorCatalogCsvImportIssue(row.LineNumber, "part.not_found", "Part was not found."));
        }

        if (!hasVendor || !hasPart)
        {
            return;
        }

        var linkKey = LinkKey(partId, vendorId);
        if (existingLinkKeys.Contains(linkKey))
        {
            issues.Add(new VendorCatalogCsvImportIssue(row.LineNumber, "vendor_catalog.duplicate", "This vendor is already linked to the part."));
        }

        if (!seenLinkKeys.Add(linkKey))
        {
            issues.Add(new VendorCatalogCsvImportIssue(row.LineNumber, "vendor_catalog.duplicate_in_file", "Vendor and part pair appears more than once in the import file."));
        }

        if (row.CatalogUnitPrice is null
            && row.CatalogLeadTimeDays is null
            && row.CatalogQuantityAvailable is null
            && string.IsNullOrWhiteSpace(row.CatalogAvailabilityStatus))
        {
            issues.Add(new VendorCatalogCsvImportIssue(row.LineNumber, "vendor_catalog.empty_facts", "Vendor catalog import requires price, lead time, quantity available, or availability status."));
        }
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

    private static decimal? ParseOptionalDecimal(
        int lineNumber,
        string column,
        string value,
        bool allowZero,
        List<VendorCatalogCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (decimal.TryParse(value.Trim(), out var parsed) && (allowZero ? parsed >= 0 : parsed > 0))
        {
            return parsed;
        }

        var requirement = allowZero ? "a non-negative decimal" : "a decimal greater than zero";
        issues.Add(new VendorCatalogCsvImportIssue(lineNumber, "csv.validation", $"{column} must be {requirement}."));
        return null;
    }

    private static int? ParseOptionalInt(
        int lineNumber,
        string column,
        string value,
        List<VendorCatalogCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value.Trim(), out var parsed) && parsed >= 0)
        {
            return parsed;
        }

        issues.Add(new VendorCatalogCsvImportIssue(lineNumber, "csv.validation", $"{column} must be a non-negative integer."));
        return null;
    }

    private static bool ParseBool(string value) =>
        value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("1", StringComparison.OrdinalIgnoreCase);

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<VendorCatalogCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new VendorCatalogCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static void ValidateMaxLength(
        int lineNumber,
        string column,
        string value,
        int maxLength,
        List<VendorCatalogCsvImportIssue> issues)
    {
        if (value.Length > maxLength)
        {
            issues.Add(new VendorCatalogCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be {maxLength} characters or fewer."));
        }
    }

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string? NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string LinkKey(Guid partId, Guid vendorId) => $"{partId:D}:{vendorId:D}";

    private sealed record ImportRow(
        int LineNumber,
        string VendorPartyKey,
        string PartKey,
        string VendorPartNumber,
        bool IsPreferred,
        decimal? CatalogUnitPrice,
        string? CatalogCurrencyCode,
        decimal? CatalogMinimumOrderQuantity,
        int? CatalogLeadTimeDays,
        decimal? CatalogQuantityAvailable,
        string CatalogAvailabilityStatus);
}
