using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class VendorDocumentsCsvImportService(
    SupplyArrDbContext db,
    PartyComplianceDocumentService documents)
{
    private const string ImportType = "vendor_documents_csv";

    private static readonly string[] Headers =
    [
        "supplier_key",
        "document_key",
        "document_type_key",
        "title",
        "effective_at",
        "expires_at",
        "file_name",
        "content_type",
        "size_bytes",
        "storage_uri"
    ];

    public async Task<VendorDocumentsCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        VendorDocumentsCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<VendorDocumentsCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var supplierIdsByKey = await db.ExternalParties
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.PartyKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var seenDocumentKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accepted = 0;

        foreach (var row in rows)
        {
            ValidateRow(row, supplierIdsByKey, seenDocumentKeys, issues);
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
            await documents.RegisterAsync(
                tenantId,
                actorUserId,
                supplierIdsByKey[row.SupplierKey],
                new RegisterPartyComplianceDocumentRequest(
                    row.DocumentKey,
                    row.DocumentTypeKey,
                    row.Title,
                    row.ExpiresAt,
                    row.EffectiveAt,
                    row.FileName,
                    row.ContentType,
                    row.SizeBytes,
                    row.StorageUri),
                cancellationToken);
            created++;
        }

        return BuildResponse(false, rows.Count, accepted, created, issues);
    }

    private static VendorDocumentsCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int documentsAccepted,
        int documentsCreated,
        IReadOnlyList<VendorDocumentsCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, documentsAccepted, documentsCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<VendorDocumentsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new VendorDocumentsCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new VendorDocumentsCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        var normalizedHeaders = headerFields
            .Select(header => string.Equals(header, "party_key", StringComparison.OrdinalIgnoreCase) ? "supplier_key" : header)
            .ToArray();
        if (normalizedHeaders.Length != Headers.Length || !normalizedHeaders.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new VendorDocumentsCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}. Legacy party_key remains accepted."));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new VendorDocumentsCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {Headers.Length} columns but found {fields.Count}."));
                continue;
            }

            var effectiveAt = ParseDate(fields[4], index + 1, "effective_at", issues);
            var expiresAt = ParseDate(fields[5], index + 1, "expires_at", issues);
            var sizeBytes = ParseLong(fields[8], index + 1, "size_bytes", issues);
            rows.Add(new ImportRow(
                index + 1,
                NormalizeKey(fields[0]),
                NormalizeDocumentKey(fields[1]),
                NormalizeKey(fields[2]),
                NormalizeOptional(fields[3]),
                effectiveAt,
                expiresAt,
                NormalizeOptional(fields[6]),
                NormalizeOptional(fields[7]),
                sizeBytes,
                NormalizeOptional(fields[9])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> supplierIdsByKey,
        ISet<string> seenDocumentKeys,
        List<VendorDocumentsCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "supplier_key", row.SupplierKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "document_key", row.DocumentKey, 1, 128, issues);
        ValidateLength(row.LineNumber, "document_type_key", row.DocumentTypeKey, 1, 64, issues);
        ValidateMaxLength(row.LineNumber, "title", row.Title, 256, issues);
        ValidateMaxLength(row.LineNumber, "file_name", row.FileName, 256, issues);
        ValidateMaxLength(row.LineNumber, "content_type", row.ContentType, 128, issues);
        if (!supplierIdsByKey.ContainsKey(row.SupplierKey))
        {
            issues.Add(new VendorDocumentsCsvImportIssue(row.LineNumber, "supplier.not_found", "Supplier key was not found."));
        }

        if (!seenDocumentKeys.Add($"{row.SupplierKey}|{row.DocumentKey}"))
        {
            issues.Add(new VendorDocumentsCsvImportIssue(row.LineNumber, "document.duplicate_in_file", "Document key appears more than once for the same supplier in the import file."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<VendorDocumentsCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new VendorDocumentsCsvImportIssue(
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
        List<VendorDocumentsCsvImportIssue> issues)
    {
        if (value.Length > maxLength)
        {
            issues.Add(new VendorDocumentsCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be {maxLength} characters or fewer."));
        }
    }

    private static DateTimeOffset? ParseDate(
        string value,
        int lineNumber,
        string column,
        List<VendorDocumentsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        issues.Add(new VendorDocumentsCsvImportIssue(lineNumber, "csv.date", $"{column} must be a date or timestamp."));
        return null;
    }

    private static long ParseLong(
        string value,
        int lineNumber,
        string column,
        List<VendorDocumentsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (long.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0)
        {
            return parsed;
        }

        issues.Add(new VendorDocumentsCsvImportIssue(lineNumber, "csv.integer", $"{column} must be a non-negative integer."));
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

    private static string NormalizeDocumentKey(string value) => value.Trim().ToUpperInvariant();

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private sealed record ImportRow(
        int LineNumber,
        string SupplierKey,
        string DocumentKey,
        string DocumentTypeKey,
        string Title,
        DateTimeOffset? EffectiveAt,
        DateTimeOffset? ExpiresAt,
        string FileName,
        string ContentType,
        long SizeBytes,
        string StorageUri);
}
