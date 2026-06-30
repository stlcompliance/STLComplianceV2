using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class OpenPurchaseOrdersCsvImportService(
    SupplyArrDbContext db,
    PurchaseRequestService purchaseRequests,
    PurchaseOrderService purchaseOrders)
{
    private const string ImportType = "open_purchase_orders_csv";

    private static readonly string[] Headers =
    [
        "order_key",
        "request_key",
        "supplier_key",
        "part_key",
        "quantity_ordered",
        "title",
        "line_notes",
        "order_notes"
    ];

    public async Task<OpenPurchaseOrdersCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        OpenPurchaseOrdersCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<OpenPurchaseOrdersCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, 0, issues);
        }

        var suppliersByKey = await db.ExternalParties
            .Where(x => x.TenantId == tenantId
                && (x.PartyType == "vendor" || x.PartyType == "supplier"))
            .ToDictionaryAsync(x => x.PartyKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var partsByKey = await db.Parts
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.PartKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var existingOrderKeys = (await db.PurchaseOrders
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.OrderKey)
            .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingRequestKeys = (await db.PurchaseRequests
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.RequestKey)
            .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var acceptedLineCount = 0;
        var grouped = rows.GroupBy(x => x.OrderKey, StringComparer.OrdinalIgnoreCase).ToList();
        var seenOrderKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenRequestKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in grouped)
        {
            ValidateGroup(group.ToList(), suppliersByKey, partsByKey, existingOrderKeys, existingRequestKeys, seenOrderKeys, seenRequestKeys, issues);
            acceptedLineCount += group.Count(row => issues.All(issue => issue.LineNumber != row.LineNumber));
        }

        if (issues.Count > 0 || request.DryRun)
        {
            var acceptedOrders = grouped.Count(group => group.All(row => issues.All(issue => issue.LineNumber != row.LineNumber)));
            return BuildResponse(request.DryRun, rows.Count, acceptedOrders, acceptedLineCount, 0, issues);
        }

        var created = 0;
        foreach (var group in grouped)
        {
            var orderRows = group.OrderBy(x => x.LineNumber).ToList();
            var first = orderRows[0];
            var purchaseRequest = await purchaseRequests.CreateAsync(
                tenantId,
                actorUserId,
                new CreatePurchaseRequestRequest(
                    RequestKey: first.RequestKey,
                    Title: first.Title,
                    Notes: first.OrderNotes,
                    SupplierId: suppliersByKey[first.SupplierKey],
                    Lines: orderRows
                        .Select(row => new CreatePurchaseRequestLineRequest(
                            partsByKey[row.PartKey],
                            row.QuantityOrdered,
                            row.LineNotes))
                        .ToList()),
                cancellationToken);

            purchaseRequest = await purchaseRequests.SubmitAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                purchaseRequest.PurchaseRequestId,
                cancellationToken);
            purchaseRequest = await purchaseRequests.ApproveAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                purchaseRequest.PurchaseRequestId,
                cancellationToken);

            var purchaseOrder = await purchaseOrders.CreateFromPurchaseRequestAsync(
                tenantId,
                actorUserId,
                purchaseRequest.PurchaseRequestId,
                new CreatePurchaseOrderFromPurchaseRequestRequest(
                    first.OrderKey,
                    first.Title,
                    first.OrderNotes),
                cancellationToken);
            await purchaseOrders.ApproveAsync(
                tenantId,
                actorUserId,
                purchaseOrder.PurchaseOrderId,
                cancellationToken);
            created++;
        }

        return BuildResponse(false, rows.Count, grouped.Count, rows.Count, created, issues);
    }

    private static OpenPurchaseOrdersCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int ordersAccepted,
        int linesAccepted,
        int ordersCreated,
        IReadOnlyList<OpenPurchaseOrdersCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, ordersAccepted, linesAccepted, ordersCreated, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<OpenPurchaseOrdersCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        var normalizedHeaders = headerFields
            .Select(header => string.Equals(header, "vendor_party_key", StringComparison.OrdinalIgnoreCase) ? "supplier_key" : header)
            .ToArray();
        if (normalizedHeaders.Length != Headers.Length || !normalizedHeaders.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}. Legacy vendor_party_key remains accepted."));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new OpenPurchaseOrdersCsvImportIssue(
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
                NormalizeKey(fields[3]),
                ParseQuantity(fields[4], index + 1, issues),
                NormalizeOptional(fields[5]),
                NormalizeOptional(fields[6]),
                NormalizeOptional(fields[7])));
        }

        return rows;
    }

    private static void ValidateGroup(
        IReadOnlyList<ImportRow> rows,
        IReadOnlyDictionary<string, Guid> suppliersByKey,
        IReadOnlyDictionary<string, Guid> partsByKey,
        ISet<string> existingOrderKeys,
        ISet<string> existingRequestKeys,
        ISet<string> seenOrderKeys,
        ISet<string> seenRequestKeys,
        List<OpenPurchaseOrdersCsvImportIssue> issues)
    {
        var first = rows[0];
        ValidateLength(first.LineNumber, "order_key", first.OrderKey, 1, 128, issues);
        ValidateLength(first.LineNumber, "request_key", first.RequestKey, 1, 128, issues);
        ValidateLength(first.LineNumber, "supplier_key", first.SupplierKey, 2, 128, issues);
        ValidateLength(first.LineNumber, "title", first.Title, 1, 256, issues);

        if (existingOrderKeys.Contains(first.OrderKey))
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(first.LineNumber, "purchase_order.duplicate", "Order key already exists."));
        }

        if (existingRequestKeys.Contains(first.RequestKey))
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(first.LineNumber, "purchase_request.duplicate", "Request key already exists."));
        }

        if (!seenOrderKeys.Add(first.OrderKey))
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(first.LineNumber, "purchase_order.duplicate_in_file", "Order key appears in multiple import groups."));
        }

        if (!seenRequestKeys.Add(first.RequestKey))
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(first.LineNumber, "purchase_request.duplicate_in_file", "Request key appears in multiple import groups."));
        }

        if (!suppliersByKey.ContainsKey(first.SupplierKey))
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(first.LineNumber, "supplier.not_found", "supplier_key was not found."));
        }

        foreach (var row in rows)
        {
            if (!string.Equals(row.RequestKey, first.RequestKey, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new OpenPurchaseOrdersCsvImportIssue(row.LineNumber, "purchase_request.mismatch", "Rows for the same order key must use the same request key."));
            }

            if (!string.Equals(row.SupplierKey, first.SupplierKey, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new OpenPurchaseOrdersCsvImportIssue(row.LineNumber, "supplier.mismatch", "Rows for the same order key must use the same supplier key."));
            }

            ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
            if (!partsByKey.ContainsKey(row.PartKey))
            {
                issues.Add(new OpenPurchaseOrdersCsvImportIssue(row.LineNumber, "part.not_found", "Part key was not found."));
            }

            if (row.QuantityOrdered <= 0)
            {
                issues.Add(new OpenPurchaseOrdersCsvImportIssue(row.LineNumber, "quantity.invalid", "quantity_ordered must be greater than zero."));
            }
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<OpenPurchaseOrdersCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new OpenPurchaseOrdersCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static decimal ParseQuantity(string value, int lineNumber, List<OpenPurchaseOrdersCsvImportIssue> issues)
    {
        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return decimal.Round(parsed, 4, MidpointRounding.AwayFromZero);
        }

        issues.Add(new OpenPurchaseOrdersCsvImportIssue(lineNumber, "csv.decimal", "quantity_ordered must be a decimal number."));
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

    private static string NormalizeOptional(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private sealed record ImportRow(
        int LineNumber,
        string OrderKey,
        string RequestKey,
        string SupplierKey,
        string PartKey,
        decimal QuantityOrdered,
        string Title,
        string LineNotes,
        string OrderNotes);
}
