using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class PartCatalogCsvImportService(
    SupplyArrDbContext db,
    PartCatalogService catalogs,
    PartRegistryService parts)
{
    private const string ImportType = "part_catalog_csv";

    private static readonly string[] Headers =
    [
        "catalog_key",
        "catalog_name",
        "catalog_description",
        "part_key",
        "part_name",
        "part_description",
        "category_key",
        "unit_of_measure",
        "manufacturer_name",
        "manufacturer_part_number"
    ];

    public async Task<PartCatalogCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        PartCatalogCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<PartCatalogCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, 0, 0, issues);
        }

        var existingCatalogs = await db.PartCatalogs
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.CatalogKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var existingParts = await db.Parts
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.PartKey)
            .ToListAsync(cancellationToken);
        var existingPartKeys = existingParts.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var acceptedCatalogKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenParts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var acceptedParts = 0;

        foreach (var row in rows)
        {
            ValidateRow(
                row,
                existingCatalogs,
                existingPartKeys,
                seenParts,
                issues);
            if (issues.Any(x => x.LineNumber == row.LineNumber))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(row.CatalogKey) && !existingCatalogs.ContainsKey(row.CatalogKey))
            {
                acceptedCatalogKeys.Add(row.CatalogKey);
            }

            if (!string.IsNullOrWhiteSpace(row.PartKey))
            {
                acceptedParts++;
            }
        }

        if (issues.Count > 0 || request.DryRun)
        {
            return BuildResponse(request.DryRun, rows.Count, acceptedCatalogKeys.Count, acceptedParts, 0, 0, issues);
        }

        var createdCatalogs = 0;
        var createdParts = 0;
        var catalogIds = new Dictionary<string, Guid>(existingCatalogs, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.CatalogKey) || catalogIds.ContainsKey(row.CatalogKey))
            {
                continue;
            }

            var created = await catalogs.CreateAsync(
                tenantId,
                actorUserId,
                new CreatePartCatalogRequest(row.CatalogKey, row.CatalogName, row.CatalogDescription),
                cancellationToken);
            catalogIds[created.CatalogKey] = created.CatalogId;
            createdCatalogs++;
        }

        foreach (var row in rows.Where(x => !string.IsNullOrWhiteSpace(x.PartKey)))
        {
            var catalogId = string.IsNullOrWhiteSpace(row.CatalogKey) ? (Guid?)null : catalogIds[row.CatalogKey];
            await parts.CreateAsync(
                tenantId,
                actorUserId,
                new CreatePartRequest(
                    row.PartKey,
                    catalogId,
                    row.PartName,
                    row.PartDescription,
                    row.CategoryKey,
                    row.UnitOfMeasure,
                    row.ManufacturerName,
                    row.ManufacturerPartNumber),
                cancellationToken);
            createdParts++;
        }

        return BuildResponse(false, rows.Count, acceptedCatalogKeys.Count, acceptedParts, createdCatalogs, createdParts, issues);
    }

    private static PartCatalogCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int catalogsAccepted,
        int partsAccepted,
        int catalogsCreated,
        int partsCreated,
        IReadOnlyList<PartCatalogCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, catalogsAccepted, partsAccepted, catalogsCreated, partsCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<PartCatalogCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new PartCatalogCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new PartCatalogCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        if (headerFields.Count != Headers.Length || !headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new PartCatalogCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new PartCatalogCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {Headers.Length} columns but found {fields.Count}."));
                continue;
            }

            rows.Add(new ImportRow(
                index + 1,
                NormalizeKey(fields[0]),
                NormalizeName(fields[1]),
                NormalizeOptional(fields[2]),
                NormalizeKey(fields[3]),
                NormalizeName(fields[4]),
                NormalizeOptional(fields[5]),
                NormalizeOptionalKey(fields[6], "general"),
                NormalizeOptionalKey(fields[7], "each"),
                NormalizeOptional(fields[8]),
                NormalizeOptional(fields[9])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> existingCatalogs,
        ISet<string> existingParts,
        ISet<string> seenParts,
        List<PartCatalogCsvImportIssue> issues)
    {
        var hasCatalog = !string.IsNullOrWhiteSpace(row.CatalogKey);
        var hasPart = !string.IsNullOrWhiteSpace(row.PartKey);
        if (!hasCatalog && !hasPart)
        {
            issues.Add(new PartCatalogCsvImportIssue(row.LineNumber, "import.empty_row", "Row must include a catalog key and/or part key."));
            return;
        }

        if (hasCatalog)
        {
            ValidateLength(row.LineNumber, "catalog_key", row.CatalogKey, 2, 128, issues);
            if (!existingCatalogs.ContainsKey(row.CatalogKey))
            {
                ValidateLength(row.LineNumber, "catalog_name", row.CatalogName, 2, 256, issues);
            }
        }

        if (!hasPart)
        {
            return;
        }

        ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "part_name", row.PartName, 2, 256, issues);
        ValidateMaxLength(row.LineNumber, "category_key", row.CategoryKey, 128, issues);
        ValidateMaxLength(row.LineNumber, "unit_of_measure", row.UnitOfMeasure, 32, issues);
        ValidateMaxLength(row.LineNumber, "manufacturer_name", row.ManufacturerName, 256, issues);
        ValidateMaxLength(row.LineNumber, "manufacturer_part_number", row.ManufacturerPartNumber, 128, issues);
        if (existingParts.Contains(row.PartKey))
        {
            issues.Add(new PartCatalogCsvImportIssue(row.LineNumber, "part.duplicate", "Part key already exists."));
        }

        if (!seenParts.Add(row.PartKey))
        {
            issues.Add(new PartCatalogCsvImportIssue(row.LineNumber, "part.duplicate_in_file", "Part key appears more than once in the import file."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<PartCatalogCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new PartCatalogCsvImportIssue(
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
        List<PartCatalogCsvImportIssue> issues)
    {
        if (value.Length > maxLength)
        {
            issues.Add(new PartCatalogCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be {maxLength} characters or fewer."));
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

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    private static string NormalizeName(string value) => value.Trim();

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeOptionalKey(string value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().ToLowerInvariant();

    private sealed record ImportRow(
        int LineNumber,
        string CatalogKey,
        string CatalogName,
        string CatalogDescription,
        string PartKey,
        string PartName,
        string PartDescription,
        string CategoryKey,
        string UnitOfMeasure,
        string ManufacturerName,
        string ManufacturerPartNumber);
}
