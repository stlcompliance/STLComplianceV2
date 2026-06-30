using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class ExternalPartiesCsvImportService(
    SupplyArrDbContext db,
    SupplierDirectoryService parties)
{
    private const string ImportType = "external_parties_csv";

    private static readonly string[] Headers =
    [
        "supplier_key",
        "parent_supplier_key",
        "unit_kind",
        "display_name",
        "legal_name",
        "tax_identifier",
        "approval_status",
        "status",
        "notes"
    ];

    private static readonly string[] LegacyHeaders =
    [
        "party_key",
        "party_type",
        "display_name",
        "legal_name",
        "tax_identifier",
        "approval_status",
        "status",
        "notes"
    ];

    private static readonly HashSet<string> AllowedLegacySupplierTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "vendor",
        "dealer",
        "supplier",
        "carrier"
    };

    private static readonly HashSet<string> AllowedUnitKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        "identity",
        "sub_unit"
    };

    private static readonly HashSet<string> AllowedApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "approved",
        "restricted",
        "inactive"
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    public async Task<ExternalPartiesCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        ExternalPartiesCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<ExternalPartiesCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var existingParties = await db.ExternalParties
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.PartyKey })
            .ToListAsync(cancellationToken);
        var existingKeys = existingParties
            .Select(x => x.PartyKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var partyIdsByKey = existingParties.ToDictionary(x => x.PartyKey, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var rowsByKey = rows
            .GroupBy(x => x.SupplierKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accepted = 0;

        foreach (var row in rows)
        {
            ValidateRow(row, existingKeys, rowsByKey, seenKeys, issues);
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
        var pendingRows = rows.OrderBy(x => x.LineNumber).ToList();
        while (pendingRows.Count > 0)
        {
            var progressed = false;
            var remainingRows = new List<ImportRow>();

            foreach (var row in pendingRows)
            {
                Guid? parentPartyId = null;
                if (!string.IsNullOrWhiteSpace(row.ParentSupplierKey))
                {
                    if (!partyIdsByKey.TryGetValue(row.ParentSupplierKey, out var resolvedParentPartyId))
                    {
                        remainingRows.Add(row);
                        continue;
                    }

                    parentPartyId = resolvedParentPartyId;
                }

                var supplier = await parties.CreateSupplierAsync(
                    tenantId,
                    actorUserId,
                    new CreateSupplierRequest(
                        row.SupplierKey,
                        parentPartyId,
                        row.UnitKind,
                        row.DisplayName,
                        row.LegalName,
                        row.TaxIdentifier,
                        row.Notes,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null),
                    cancellationToken);

                if (!string.Equals(row.ApprovalStatus, "pending", StringComparison.OrdinalIgnoreCase))
                {
                    await parties.UpdateSupplierApprovalStatusAsync(
                        tenantId,
                        actorUserId,
                        supplier.SupplierId,
                        new UpdateSupplierApprovalStatusRequest(row.ApprovalStatus),
                        cancellationToken);
                }

                if (!string.Equals(row.Status, "active", StringComparison.OrdinalIgnoreCase))
                {
                    await parties.UpdateSupplierStatusAsync(
                        tenantId,
                        actorUserId,
                        supplier.SupplierId,
                        new UpdateSupplierStatusRequest(row.Status),
                        cancellationToken);
                }

                partyIdsByKey[row.SupplierKey] = supplier.SupplierId;
                created++;
                progressed = true;
            }

            if (!progressed)
            {
                foreach (var row in remainingRows)
                {
                    issues.Add(new ExternalPartiesCsvImportIssue(
                        row.LineNumber,
                        "supplier.parent_not_found",
                        "parent_supplier_key must reference an existing or earlier-created supplier identity."));
                }

                return BuildResponse(false, rows.Count, accepted, created, issues);
            }

            pendingRows = remainingRows;
        }

        return BuildResponse(false, rows.Count, accepted, created, issues);
    }

    private static ExternalPartiesCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int partiesAccepted,
        int partiesCreated,
        IReadOnlyList<ExternalPartiesCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, partiesAccepted, partiesCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<ExternalPartiesCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new ExternalPartiesCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        var usesCanonicalHeaders = headerFields.Count == Headers.Length
            && headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase);
        var usesLegacyHeaders = headerFields.Count == LegacyHeaders.Length
            && headerFields.SequenceEqual(LegacyHeaders, StringComparer.OrdinalIgnoreCase);
        if (!usesCanonicalHeaders && !usesLegacyHeaders)
        {
            issues.Add(new ExternalPartiesCsvImportIssue(
                1,
                "csv.header",
                $"Header must be: {string.Join(",", Headers)}. Legacy compatibility header remains accepted: {string.Join(",", LegacyHeaders)}"));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            var expectedColumns = usesCanonicalHeaders ? Headers.Length : LegacyHeaders.Length;
            if (fields.Count != expectedColumns)
            {
                issues.Add(new ExternalPartiesCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {expectedColumns} columns but found {fields.Count}."));
                continue;
            }

            if (usesCanonicalHeaders)
            {
                rows.Add(new ImportRow(
                    index + 1,
                    NormalizeKey(fields[0]),
                    "supplier",
                    NormalizeNullableKey(fields[1]),
                    NormalizeNullableKey(fields[2]),
                    NormalizeOptional(fields[3]),
                    NormalizeOptional(fields[4]),
                    NormalizeNullable(fields[5]),
                    NormalizeStatus(fields[6], "pending"),
                    NormalizeStatus(fields[7], "active"),
                    NormalizeOptional(fields[8])));
            }
            else
            {
                rows.Add(new ImportRow(
                    index + 1,
                    NormalizeKey(fields[0]),
                    NormalizeLegacyPartyType(fields[1]),
                    null,
                    null,
                    NormalizeOptional(fields[2]),
                    NormalizeOptional(fields[3]),
                    NormalizeNullable(fields[4]),
                    NormalizeStatus(fields[5], "pending"),
                    NormalizeStatus(fields[6], "active"),
                    NormalizeOptional(fields[7])));
            }
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        ISet<string> existingKeys,
        IReadOnlyDictionary<string, ImportRow> rowsByKey,
        ISet<string> seenKeys,
        List<ExternalPartiesCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "supplier_key", row.SupplierKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "display_name", row.DisplayName, 2, 256, issues);
        ValidateMaxLength(row.LineNumber, "legal_name", row.LegalName, 256, issues);
        if (!AllowedLegacySupplierTypes.Contains(row.PartyType) && !string.Equals(row.PartyType, "customer", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.legacy_type_invalid", "Legacy party_type must be supplier, vendor, dealer, or carrier."));
        }
        else if (string.Equals(row.PartyType, "customer", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.legacy_customer_unsupported", "Legacy party_type customer is no longer supported in SupplyArr. Customer master data belongs in CustomArr."));
        }

        if (!string.IsNullOrWhiteSpace(row.UnitKind) && !AllowedUnitKinds.Contains(row.UnitKind))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.unit_kind_invalid", "unit_kind must be identity or sub_unit."));
        }

        if (!string.IsNullOrWhiteSpace(row.ParentSupplierKey))
        {
            if (string.Equals(row.ParentSupplierKey, row.SupplierKey, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.parent_self", "parent_supplier_key cannot match supplier_key."));
            }

            if (!string.IsNullOrWhiteSpace(row.UnitKind)
                && !string.Equals(row.UnitKind, "sub_unit", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.unit_kind_parent_mismatch", "Rows with parent_supplier_key must use unit_kind sub_unit."));
            }

            if (!existingKeys.Contains(row.ParentSupplierKey)
                && !rowsByKey.ContainsKey(row.ParentSupplierKey))
            {
                issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.parent_not_found", "parent_supplier_key must reference an existing or imported supplier identity."));
            }

            if (rowsByKey.TryGetValue(row.ParentSupplierKey, out var parentRow)
                && !string.Equals(ResolveUnitKind(parentRow), "identity", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.parent_not_root", "parent_supplier_key must point to a root supplier identity, not another sub-unit."));
            }
        }
        else if (string.Equals(row.UnitKind, "sub_unit", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.parent_required", "unit_kind sub_unit requires parent_supplier_key."));
        }

        if (!AllowedApprovalStatuses.Contains(row.ApprovalStatus))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "party.approval_status_invalid", "approval_status must be pending, approved, restricted, or inactive."));
        }

        if (!AllowedStatuses.Contains(row.Status))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "party.status_invalid", "status must be active or inactive."));
        }

        if (existingKeys.Contains(row.SupplierKey))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.duplicate", "supplier_key already exists."));
        }

        if (!seenKeys.Add(row.SupplierKey))
        {
            issues.Add(new ExternalPartiesCsvImportIssue(row.LineNumber, "supplier.duplicate_in_file", "supplier_key appears more than once in the import file."));
        }
    }

    private static string ResolveUnitKind(ImportRow row) =>
        !string.IsNullOrWhiteSpace(row.UnitKind)
            ? row.UnitKind
            : string.IsNullOrWhiteSpace(row.ParentSupplierKey)
                ? "identity"
                : "sub_unit";

    private static string NormalizeLegacyPartyType(string value)
    {
        var normalized = NormalizeKey(value);
        return normalized switch
        {
            "vendor" or "dealer" or "carrier" or "supplier" => "supplier",
            _ => normalized
        };
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<ExternalPartiesCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new ExternalPartiesCsvImportIssue(
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
        List<ExternalPartiesCsvImportIssue> issues)
    {
        if (value.Length > maxLength)
        {
            issues.Add(new ExternalPartiesCsvImportIssue(
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

    private static string? NormalizeNullableKey(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : NormalizeKey(value);

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string? NormalizeNullable(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeStatus(string value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().ToLowerInvariant();

    private sealed record ImportRow(
        int LineNumber,
        string SupplierKey,
        string PartyType,
        string? ParentSupplierKey,
        string? UnitKind,
        string DisplayName,
        string LegalName,
        string? TaxIdentifier,
        string ApprovalStatus,
        string Status,
        string Notes);
}
