using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ContractsCsvImportService(
    SupplyArrDbContext db,
    SupplyContractService contracts)
{
    private const string ImportType = "contracts_csv";

    private static readonly string[] Headers =
    [
        "supplier_key",
        "contract_key",
        "contract_type",
        "title",
        "effective_at",
        "expires_at",
        "renewal_at",
        "payment_terms",
        "freight_terms",
        "warranty_terms",
        "minimum_spend",
        "service_level_agreement",
        "approval_status",
        "status",
        "notes"
    ];

    public async Task<ContractsCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        ContractsCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<ContractsCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var suppliersByKey = await db.Suppliers
            .Where(x => x.TenantId == tenantId
               )
            .ToDictionaryAsync(x => x.SupplierKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var existingContractKeys = await db.SupplyContracts
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.ContractKey)
            .ToListAsync(cancellationToken);
        var existingKeys = existingContractKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accepted = 0;

        foreach (var row in rows)
        {
            ValidateRow(row, suppliersByKey, existingKeys, seenKeys, issues);
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
            await contracts.CreateAsync(
                tenantId,
                actorUserId,
                new CreateSupplyContractRequest(
                    row.ContractKey,
                    row.ContractType,
                    row.Title,
                    suppliersByKey[row.SupplierKey],
                    row.EffectiveAt,
                    row.ExpiresAt,
                    row.RenewalAt,
                    row.PaymentTerms,
                    row.FreightTerms,
                    row.WarrantyTerms,
                    row.MinimumSpend,
                    row.ServiceLevelAgreement,
                    row.ApprovalStatus,
                    row.Status,
                    row.Notes),
                cancellationToken);
            created++;
        }

        return BuildResponse(false, rows.Count, accepted, created, issues);
    }

    private static ContractsCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int contractsAccepted,
        int contractsCreated,
        IReadOnlyList<ContractsCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, contractsAccepted, contractsCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<ContractsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new ContractsCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new ContractsCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        var normalizedHeaderFields = NormalizeHeaderFields(headerFields);
        if (normalizedHeaderFields.Count != Headers.Length || !normalizedHeaderFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new ContractsCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new ContractsCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {Headers.Length} columns but found {fields.Count}."));
                continue;
            }

            rows.Add(new ImportRow(
                index + 1,
                NormalizeKey(fields[0]),
                NormalizeContractKey(fields[1]),
                NormalizeOptional(fields[2]),
                NormalizeOptional(fields[3]),
                ParseOptionalDate(fields[4], index + 1, "effective_at", issues),
                ParseOptionalDate(fields[5], index + 1, "expires_at", issues),
                ParseOptionalDate(fields[6], index + 1, "renewal_at", issues),
                NormalizeOptional(fields[7]),
                NormalizeOptional(fields[8]),
                NormalizeOptional(fields[9]),
                ParseOptionalDecimal(fields[10], index + 1, "minimum_spend", issues),
                NormalizeOptional(fields[11]),
                NormalizeStatusKey(fields[12], SupplyContractApprovalStatuses.Draft),
                NormalizeStatusKey(fields[13], SupplyContractStatuses.Draft),
                NormalizeOptional(fields[14])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        IReadOnlyDictionary<string, Guid> suppliersByKey,
        ISet<string> existingContractKeys,
        ISet<string> seenKeys,
        List<ContractsCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "supplier_key", row.SupplierKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "contract_key", row.ContractKey, 3, 128, issues);
        ValidateLength(row.LineNumber, "contract_type", row.ContractType, 2, 64, issues);
        ValidateLength(row.LineNumber, "title", row.Title, 2, 256, issues);
        if (!suppliersByKey.ContainsKey(row.SupplierKey))
        {
            issues.Add(new ContractsCsvImportIssue(row.LineNumber, "supplier.not_found", "Supplier key was not found."));
        }

        if (row.ExpiresAt.HasValue && row.EffectiveAt.HasValue && row.ExpiresAt.Value <= row.EffectiveAt.Value)
        {
            issues.Add(new ContractsCsvImportIssue(row.LineNumber, "contract.invalid_expiration", "expires_at must be after effective_at."));
        }

        if (row.RenewalAt.HasValue && row.ExpiresAt.HasValue && row.RenewalAt.Value > row.ExpiresAt.Value)
        {
            issues.Add(new ContractsCsvImportIssue(row.LineNumber, "contract.invalid_renewal", "renewal_at cannot be after expires_at."));
        }

        if (row.MinimumSpend is < 0)
        {
            issues.Add(new ContractsCsvImportIssue(row.LineNumber, "contract.invalid_minimum_spend", "minimum_spend cannot be negative."));
        }

        if (!SupplyContractApprovalStatuses.All.Contains(row.ApprovalStatus))
        {
            issues.Add(new ContractsCsvImportIssue(row.LineNumber, "contract.invalid_approval_status", "approval_status is not supported."));
        }

        if (!SupplyContractStatuses.All.Contains(row.Status))
        {
            issues.Add(new ContractsCsvImportIssue(row.LineNumber, "contract.invalid_status", "status is not supported."));
        }

        if (existingContractKeys.Contains(row.ContractKey))
        {
            issues.Add(new ContractsCsvImportIssue(row.LineNumber, "contract.duplicate", "Contract key already exists."));
        }

        if (!seenKeys.Add(row.ContractKey))
        {
            issues.Add(new ContractsCsvImportIssue(row.LineNumber, "contract.duplicate_in_file", "Contract key appears more than once in the import file."));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<ContractsCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new ContractsCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static DateTimeOffset? ParseOptionalDate(
        string value,
        int lineNumber,
        string column,
        List<ContractsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        issues.Add(new ContractsCsvImportIssue(lineNumber, "csv.date", $"{column} must be a date or timestamp."));
        return null;
    }

    private static decimal? ParseOptionalDecimal(
        string value,
        int lineNumber,
        string column,
        List<ContractsCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return decimal.Round(parsed, 2, MidpointRounding.AwayFromZero);
        }

        issues.Add(new ContractsCsvImportIssue(lineNumber, "csv.decimal", $"{column} must be a decimal number."));
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
        headerFields.ToArray();

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();

    private static string NormalizeContractKey(string value) => value.Trim().ToUpperInvariant();

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeStatusKey(string value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');

    private sealed record ImportRow(
        int LineNumber,
        string SupplierKey,
        string ContractKey,
        string ContractType,
        string Title,
        DateTimeOffset? EffectiveAt,
        DateTimeOffset? ExpiresAt,
        DateTimeOffset? RenewalAt,
        string PaymentTerms,
        string FreightTerms,
        string WarrantyTerms,
        decimal? MinimumSpend,
        string ServiceLevelAgreement,
        string ApprovalStatus,
        string Status,
        string Notes);
}


