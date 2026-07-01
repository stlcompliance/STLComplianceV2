using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class SuppliersCsvImportService(
    SupplyArrDbContext db,
    SupplierDirectoryService suppliers)
{
    private const string ImportType = "suppliers_csv";

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
        "notes",
        "service_types"
    ];

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

    private static readonly HashSet<string> AllowedServiceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "products",
        "parts",
        "maintenance",
        "repair",
        "warranty",
        "field_service",
        "logistics"
    };

    public async Task<SuppliersCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        SuppliersCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<SuppliersCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, issues);
        }

        var existingSuppliers = await db.Suppliers
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.Id, x.SupplierKey })
            .ToListAsync(cancellationToken);
        var existingKeys = existingSuppliers
            .Select(x => x.SupplierKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var supplierIdsByKey = existingSuppliers.ToDictionary(x => x.SupplierKey, x => x.Id, StringComparer.OrdinalIgnoreCase);
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
                Guid? parentSupplierId = null;
                if (!string.IsNullOrWhiteSpace(row.ParentSupplierKey))
                {
                    if (!supplierIdsByKey.TryGetValue(row.ParentSupplierKey, out var resolvedParentSupplierId))
                    {
                        remainingRows.Add(row);
                        continue;
                    }

                    parentSupplierId = resolvedParentSupplierId;
                }

                var supplier = await suppliers.CreateSupplierAsync(
                    tenantId,
                    actorUserId,
                    new CreateSupplierRequest(
                        row.SupplierKey,
                        parentSupplierId,
                        row.UnitKind,
                        row.DisplayName,
                        row.LegalName,
                        row.TaxIdentifier,
                        row.Notes,
                        row.ServiceTypes,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null),
                    cancellationToken);

                if (!string.Equals(row.ApprovalStatus, "pending", StringComparison.OrdinalIgnoreCase))
                {
                    await suppliers.UpdateSupplierApprovalStatusAsync(
                        tenantId,
                        actorUserId,
                        supplier.SupplierId,
                        new UpdateSupplierApprovalStatusRequest(row.ApprovalStatus),
                        cancellationToken);
                }

                if (!string.Equals(row.Status, "active", StringComparison.OrdinalIgnoreCase))
                {
                    await suppliers.UpdateSupplierStatusAsync(
                        tenantId,
                        actorUserId,
                        supplier.SupplierId,
                        new UpdateSupplierStatusRequest(row.Status),
                        cancellationToken);
                }

                supplierIdsByKey[row.SupplierKey] = supplier.SupplierId;
                created++;
                progressed = true;
            }

            if (!progressed)
            {
                foreach (var row in remainingRows)
                {
                    issues.Add(new SuppliersCsvImportIssue(
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

    private static SuppliersCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int suppliersAccepted,
        int suppliersCreated,
        IReadOnlyList<SuppliersCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, suppliersAccepted, suppliersCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<SuppliersCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new SuppliersCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new SuppliersCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        var usesCanonicalHeaders = headerFields.Count == Headers.Length
            && headerFields.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase);
        if (!usesCanonicalHeaders)
        {
            issues.Add(new SuppliersCsvImportIssue(
                1,
                "csv.header",
                $"Header must be: {string.Join(",", Headers)}."));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            var expectedColumns = Headers.Length;
            if (fields.Count != expectedColumns)
            {
                issues.Add(new SuppliersCsvImportIssue(
                    index + 1,
                    "csv.columns",
                    $"Expected {expectedColumns} columns but found {fields.Count}."));
                continue;
            }

            rows.Add(new ImportRow(
                index + 1,
                NormalizeKey(fields[0]),
                NormalizeNullableKey(fields[1]),
                NormalizeNullableKey(fields[2]),
                NormalizeOptional(fields[3]),
                NormalizeOptional(fields[4]),
                NormalizeNullable(fields[5]),
                NormalizeStatus(fields[6], "pending"),
                NormalizeStatus(fields[7], "active"),
                NormalizeOptional(fields[8]),
                ParseServiceTypes(fields[9])));
        }

        return rows;
    }

    private static void ValidateRow(
        ImportRow row,
        ISet<string> existingKeys,
        IReadOnlyDictionary<string, ImportRow> rowsByKey,
        ISet<string> seenKeys,
        List<SuppliersCsvImportIssue> issues)
    {
        ValidateLength(row.LineNumber, "supplier_key", row.SupplierKey, 2, 128, issues);
        ValidateLength(row.LineNumber, "display_name", row.DisplayName, 2, 256, issues);
        ValidateMaxLength(row.LineNumber, "legal_name", row.LegalName, 256, issues);

        if (!string.IsNullOrWhiteSpace(row.UnitKind) && !AllowedUnitKinds.Contains(row.UnitKind))
        {
            issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.unit_kind_invalid", "unit_kind must be identity or sub_unit."));
        }

        if (!string.IsNullOrWhiteSpace(row.ParentSupplierKey))
        {
            if (string.Equals(row.ParentSupplierKey, row.SupplierKey, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.parent_self", "parent_supplier_key cannot match supplier_key."));
            }

            if (!string.IsNullOrWhiteSpace(row.UnitKind)
                && !string.Equals(row.UnitKind, "sub_unit", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.unit_kind_parent_mismatch", "Rows with parent_supplier_key must use unit_kind sub_unit."));
            }

            if (!existingKeys.Contains(row.ParentSupplierKey)
                && !rowsByKey.ContainsKey(row.ParentSupplierKey))
            {
                issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.parent_not_found", "parent_supplier_key must reference an existing or imported supplier identity."));
            }

            if (rowsByKey.TryGetValue(row.ParentSupplierKey, out var parentRow)
                && !string.Equals(ResolveUnitKind(parentRow), "identity", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.parent_not_root", "parent_supplier_key must point to a root supplier identity, not another sub-unit."));
            }
        }
        else if (string.Equals(row.UnitKind, "sub_unit", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.parent_required", "unit_kind sub_unit requires parent_supplier_key."));
        }

        if (!AllowedApprovalStatuses.Contains(row.ApprovalStatus))
        {
            issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.approval_status_invalid", "approval_status must be pending, approved, restricted, or inactive."));
        }

        if (!AllowedStatuses.Contains(row.Status))
        {
            issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.status_invalid", "status must be active or inactive."));
        }

        foreach (var serviceType in row.ServiceTypes)
        {
            if (!AllowedServiceTypes.Contains(serviceType))
            {
                issues.Add(new SuppliersCsvImportIssue(
                    row.LineNumber,
                    "supplier.service_types_invalid",
                    "service_types must contain only products, parts, maintenance, repair, warranty, field_service, or logistics."));
                break;
            }
        }

        if (existingKeys.Contains(row.SupplierKey))
        {
            issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.duplicate", "supplier_key already exists."));
        }

        if (!seenKeys.Add(row.SupplierKey))
        {
            issues.Add(new SuppliersCsvImportIssue(row.LineNumber, "supplier.duplicate_in_file", "supplier_key appears more than once in the import file."));
        }
    }

    private static string ResolveUnitKind(ImportRow row) =>
        !string.IsNullOrWhiteSpace(row.UnitKind)
            ? row.UnitKind
            : string.IsNullOrWhiteSpace(row.ParentSupplierKey)
                ? "identity"
                : "sub_unit";

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<SuppliersCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new SuppliersCsvImportIssue(
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
        List<SuppliersCsvImportIssue> issues)
    {
        if (value.Length > maxLength)
        {
            issues.Add(new SuppliersCsvImportIssue(
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

    private static IReadOnlyList<string> ParseServiceTypes(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value
                .Split(['|', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(serviceType => serviceType.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

    private static string NormalizeStatus(string value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim().ToLowerInvariant();

    private sealed record ImportRow(
        int LineNumber,
        string SupplierKey,
        string? ParentSupplierKey,
        string? UnitKind,
        string DisplayName,
        string LegalName,
        string? TaxIdentifier,
        string ApprovalStatus,
        string Status,
        string Notes,
        IReadOnlyList<string> ServiceTypes);
}

