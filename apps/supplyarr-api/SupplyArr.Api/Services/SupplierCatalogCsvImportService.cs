using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class SupplierCatalogCsvImportService(
    SupplyArrDbContext db,
    PartRegistryService parts)
{
    private const string ImportType = "supplier_catalog_csv";

    private static readonly string[] Headers =
    [
        "supplier_key",
        "part_key",
        "supplier_part_number",
        "is_preferred",
        "catalog_unit_price",
        "catalog_currency_code",
        "catalog_minimum_order_quantity",
        "catalog_lead_time_days",
        "catalog_quantity_available",
        "catalog_availability_status"
    ];

    public async Task<SupplierCatalogCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        SupplierCatalogCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<SupplierCatalogCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var suppliersByKey = await db.Suppliers
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.SupplierKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var partIds = await db.Parts
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.PartKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var existingLinks = await db.PartSupplierLinks
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.PartId, x.SupplierId })
            .ToListAsync(cancellationToken);
        var existingLinkKeys = existingLinks
            .Select(x => LinkKey(x.PartId, x.SupplierId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seenLinkKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var acceptedLinks = 0;
        foreach (var row in rows)
        {
            ValidateRow(row, suppliersByKey, partIds, existingLinkKeys, seenLinkKeys, issues);
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
            var supplierId = suppliersByKey[row.SupplierKey];
            var link = await parts.AddSupplierLinkAsync(
                tenantId,
                actorUserId,
                partId,
                new CreatePartSupplierLinkRequest(null, supplierId, row.SupplierPartNumber, row.IsPreferred),
                cancellationToken);

            if (row.CatalogUnitPrice is not null)
            {
                await parts.UpsertSupplierLinkCatalogPriceAsync(
                    tenantId,
                    actorUserId,
                    partId,
                    link.LinkId,
                    new UpsertPartSupplierLinkCatalogPriceRequest(
                        row.CatalogUnitPrice.Value,
                        row.CatalogCurrencyCode,
                        row.CatalogMinimumOrderQuantity),
                    cancellationToken);
            }

            if (row.CatalogLeadTimeDays is not null)
            {
                await parts.UpsertSupplierLinkCatalogLeadTimeAsync(
                    tenantId,
                    actorUserId,
                    partId,
                    link.LinkId,
                    new UpsertPartSupplierLinkCatalogLeadTimeRequest(row.CatalogLeadTimeDays.Value),
                    cancellationToken);
            }

            if (row.CatalogQuantityAvailable is not null || !string.IsNullOrWhiteSpace(row.CatalogAvailabilityStatus))
            {
                await parts.UpsertSupplierLinkCatalogAvailabilityAsync(
                    tenantId,
                    actorUserId,
                    partId,
                    link.LinkId,
                    new UpsertPartSupplierLinkCatalogAvailabilityRequest(
                        row.CatalogQuantityAvailable,
                        row.CatalogAvailabilityStatus),
                    cancellationToken);
            }

            createdLinks++;
        }

        return BuildResponse(false, rows.Count, acceptedLinks, createdLinks, issues);
    }

    private static SupplierCatalogCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int linksAccepted,
        int linksCreated,
        IReadOnlyList<SupplierCatalogCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, linksAccepted, linksCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<SupplierCatalogCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new SupplierCatalogCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new SupplierCatalogCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = NormalizeHeaderFields(ParseRow(lines[0]));
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new SupplierCatalogCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}."));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new SupplierCatalogCsvImportIssue(
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
        IReadOnlyDictionary<string, Guid> suppliersByKey,
        IReadOnlyDictionary<string, Guid> partIds,
        ISet<string> existingLinkKeys,
        ISet<string> seenLinkKeys,
        List<SupplierCatalogCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "supplier_key", row.SupplierKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "supplier_part_number", row.SupplierPartNumber, 1, 128, issues);
        ValidateMaxLength(row.LineNumber, "catalog_availability_status", row.CatalogAvailabilityStatus, 64, issues);

        var hasSupplier = suppliersByKey.TryGetValue(row.SupplierKey, out var supplierId);
        if (!hasSupplier)
        {
            issues.Add(new SupplierCatalogCsvImportIssue(row.LineNumber, "supplier.not_found", "Supplier key was not found."));
        }

        var hasPart = partIds.TryGetValue(row.PartKey, out var partId);
        if (!hasPart)
        {
            issues.Add(new SupplierCatalogCsvImportIssue(row.LineNumber, "part.not_found", "Part was not found."));
        }

        if (!hasSupplier || !hasPart)
        {
            return;
        }

        var linkKey = LinkKey(partId, supplierId);
        if (existingLinkKeys.Contains(linkKey))
        {
            issues.Add(new SupplierCatalogCsvImportIssue(row.LineNumber, "supplier_catalog.duplicate", "This supplier unit is already linked to the part."));
        }

        if (!seenLinkKeys.Add(linkKey))
        {
            issues.Add(new SupplierCatalogCsvImportIssue(row.LineNumber, "supplier_catalog.duplicate_in_file", "Supplier and part pair appears more than once in the import file."));
        }

        if (row.CatalogUnitPrice is null
            && row.CatalogLeadTimeDays is null
            && row.CatalogQuantityAvailable is null
            && string.IsNullOrWhiteSpace(row.CatalogAvailabilityStatus))
        {
            issues.Add(new SupplierCatalogCsvImportIssue(row.LineNumber, "supplier_catalog.empty_facts", "Supplier catalog import requires price, lead time, quantity available, or availability status."));
        }
    }

    private static IReadOnlyList<string> NormalizeHeaderFields(IReadOnlyList<string> headerFields) =>
        headerFields.ToList();

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
        List<SupplierCatalogCsvImportIssue> issues)
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
        issues.Add(new SupplierCatalogCsvImportIssue(lineNumber, "csv.validation", $"{column} must be {requirement}."));
        return null;
    }

    private static int? ParseOptionalInt(
        int lineNumber,
        string column,
        string value,
        List<SupplierCatalogCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value.Trim(), out var parsed) && parsed >= 0)
        {
            return parsed;
        }

        issues.Add(new SupplierCatalogCsvImportIssue(lineNumber, "csv.validation", $"{column} must be a non-negative integer."));
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
        List<SupplierCatalogCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new SupplierCatalogCsvImportIssue(
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
        List<SupplierCatalogCsvImportIssue> issues)
    {
        if (value.Length > maxLength)
        {
            issues.Add(new SupplierCatalogCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be {maxLength} characters or fewer."));
        }
    }

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string? NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string LinkKey(Guid partId, Guid supplierId) => $"{partId:D}:{supplierId:D}";

    private sealed record ImportRow(
        int LineNumber,
        string SupplierKey,
        string PartKey,
        string SupplierPartNumber,
        bool IsPreferred,
        decimal? CatalogUnitPrice,
        string? CatalogCurrencyCode,
        decimal? CatalogMinimumOrderQuantity,
        int? CatalogLeadTimeDays,
        decimal? CatalogQuantityAvailable,
        string CatalogAvailabilityStatus);
}

