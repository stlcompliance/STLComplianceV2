using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Services;

public sealed class PurchaseHistoryCsvImportService(
    SupplyArrDbContext db,
    PurchaseRequestService purchaseRequests,
    PurchaseOrderService purchaseOrders,
    ReceivingService receiving)
{
    private const string ImportType = "purchase_history_csv";

    private static readonly string[] Headers =
    [
        "order_key",
        "request_key",
        "receipt_key",
        "supplier_key",
        "part_key",
        "quantity_ordered",
        "quantity_received",
        "inventory_bin_key",
        "title",
        "line_notes",
        "order_notes",
        "receipt_notes"
    ];

    public async Task<PurchaseHistoryCsvImportResponse> ImportAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        PurchaseHistoryCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<PurchaseHistoryCsvImportIssue>();
        var rows = Parse(request.Csv, issues);
        if (issues.Count > 0)
        {
            return BuildResponse(request.DryRun, rows.Count, 0, 0, 0, 0, issues);
        }

        var suppliersByKey = await db.ExternalParties
            .Where(x => x.TenantId == tenantId
                && (x.PartyType == "vendor" || x.PartyType == "supplier"))
            .ToDictionaryAsync(x => x.PartyKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var partsByKey = await db.Parts
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.PartKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var binsByKey = await db.InventoryBins
            .Where(x => x.TenantId == tenantId && x.Status == "active")
            .ToDictionaryAsync(x => x.BinKey, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
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
        var existingReceiptKeys = (await db.ReceivingReceipts
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.ReceiptKey)
            .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var grouped = rows.GroupBy(x => x.OrderKey, StringComparer.OrdinalIgnoreCase).ToList();
        var seenOrderKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenRequestKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenReceiptKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in grouped)
        {
            ValidateGroup(
                group.OrderBy(x => x.LineNumber).ToList(),
                suppliersByKey,
                partsByKey,
                binsByKey,
                existingOrderKeys,
                existingRequestKeys,
                existingReceiptKeys,
                seenOrderKeys,
                seenRequestKeys,
                seenReceiptKeys,
                issues);
        }

        var acceptedOrders = grouped.Count(group => group.All(row => issues.All(issue => issue.LineNumber != row.LineNumber)));
        var acceptedLines = rows.Count(row => issues.All(issue => issue.LineNumber != row.LineNumber));
        if (issues.Count > 0 || request.DryRun)
        {
            return BuildResponse(request.DryRun, rows.Count, acceptedOrders, acceptedLines, 0, 0, issues);
        }

        var createdOrders = 0;
        var postedReceipts = 0;
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
            purchaseOrder = await purchaseOrders.ApproveAsync(
                tenantId,
                actorUserId,
                purchaseOrder.PurchaseOrderId,
                cancellationToken);
            purchaseOrder = await purchaseOrders.IssueAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                purchaseOrder.PurchaseOrderId,
                cancellationToken);
            createdOrders++;

            var receipt = await receiving.CreateFromPurchaseOrderAsync(
                tenantId,
                actorUserId,
                purchaseOrder.PurchaseOrderId,
                new CreateReceivingReceiptFromPurchaseOrderRequest(
                    first.ReceiptKey,
                    binsByKey[first.InventoryBinKey],
                    first.ReceiptNotes,
                    $"ps-{first.ReceiptKey}",
                    $"{first.ReceiptKey}-packing-slip.pdf"),
                cancellationToken);

            foreach (var row in orderRows)
            {
                var receiptLine = receipt.Lines.Single(x => string.Equals(x.PartKey, row.PartKey, StringComparison.OrdinalIgnoreCase));
                if (receiptLine.QuantityReceived != row.QuantityReceived)
                {
                    receipt = await receiving.UpdateLineAsync(
                        tenantId,
                        actorUserId,
                        receipt.ReceivingReceiptId,
                        receiptLine.LineId,
                        new UpdateReceivingReceiptLineRequest(row.QuantityReceived),
                        cancellationToken);
                }
            }

            await receiving.PostAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                receipt.ReceivingReceiptId,
                cancellationToken);
            postedReceipts++;
        }

        return BuildResponse(false, rows.Count, grouped.Count, rows.Count, createdOrders, postedReceipts, issues);
    }

    private static PurchaseHistoryCsvImportResponse BuildResponse(
        bool dryRun,
        int rowsRead,
        int ordersAccepted,
        int linesAccepted,
        int ordersCreated,
        int receiptsPosted,
        IReadOnlyList<PurchaseHistoryCsvImportIssue> issues) =>
        new(ImportType, dryRun, issues.Count == 0, rowsRead, ordersAccepted, linesAccepted, ordersCreated, receiptsPosted, issues);

    private static IReadOnlyList<ImportRow> Parse(string csv, List<PurchaseHistoryCsvImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(1, "csv.empty", "CSV content is required."));
            return [];
        }

        var headerFields = ParseRow(lines[0]);
        var normalizedHeaders = headerFields
            .Select(header => string.Equals(header, "vendor_party_key", StringComparison.OrdinalIgnoreCase) ? "supplier_key" : header)
            .ToArray();
        if (normalizedHeaders.Length != Headers.Length || !normalizedHeaders.SequenceEqual(Headers, StringComparer.OrdinalIgnoreCase))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(1, "csv.header", $"Header must be: {string.Join(",", Headers)}. Legacy vendor_party_key remains accepted."));
            return [];
        }

        var rows = new List<ImportRow>();
        for (var index = 1; index < lines.Length; index++)
        {
            var fields = ParseRow(lines[index]);
            if (fields.Count != Headers.Length)
            {
                issues.Add(new PurchaseHistoryCsvImportIssue(
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
                NormalizeKey(fields[4]),
                ParseQuantity(fields[5], "quantity_ordered", index + 1, issues),
                ParseQuantity(fields[6], "quantity_received", index + 1, issues),
                NormalizeKey(fields[7]),
                NormalizeOptional(fields[8]),
                NormalizeOptional(fields[9]),
                NormalizeOptional(fields[10]),
                NormalizeOptional(fields[11])));
        }

        return rows;
    }

    private static void ValidateGroup(
        IReadOnlyList<ImportRow> rows,
        IReadOnlyDictionary<string, Guid> suppliersByKey,
        IReadOnlyDictionary<string, Guid> partsByKey,
        IReadOnlyDictionary<string, Guid> binsByKey,
        ISet<string> existingOrderKeys,
        ISet<string> existingRequestKeys,
        ISet<string> existingReceiptKeys,
        ISet<string> seenOrderKeys,
        ISet<string> seenRequestKeys,
        ISet<string> seenReceiptKeys,
        List<PurchaseHistoryCsvImportIssue> issues)
    {
        var first = rows[0];
        ValidateLength(first.LineNumber, "order_key", first.OrderKey, 1, 128, issues);
        ValidateLength(first.LineNumber, "request_key", first.RequestKey, 1, 128, issues);
        ValidateLength(first.LineNumber, "receipt_key", first.ReceiptKey, 1, 128, issues);
        ValidateLength(first.LineNumber, "supplier_key", first.SupplierKey, 2, 128, issues);
        ValidateLength(first.LineNumber, "inventory_bin_key", first.InventoryBinKey, 1, 128, issues);
        ValidateLength(first.LineNumber, "title", first.Title, 1, 256, issues);

        if (existingOrderKeys.Contains(first.OrderKey))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(first.LineNumber, "purchase_order.duplicate", "Order key already exists."));
        }

        if (existingRequestKeys.Contains(first.RequestKey))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(first.LineNumber, "purchase_request.duplicate", "Request key already exists."));
        }

        if (existingReceiptKeys.Contains(first.ReceiptKey))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(first.LineNumber, "receiving_receipt.duplicate", "Receipt key already exists."));
        }

        if (!seenOrderKeys.Add(first.OrderKey))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(first.LineNumber, "purchase_order.duplicate_in_file", "Order key appears in multiple import groups."));
        }

        if (!seenRequestKeys.Add(first.RequestKey))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(first.LineNumber, "purchase_request.duplicate_in_file", "Request key appears in multiple import groups."));
        }

        if (!seenReceiptKeys.Add(first.ReceiptKey))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(first.LineNumber, "receiving_receipt.duplicate_in_file", "Receipt key appears in multiple import groups."));
        }

        if (!suppliersByKey.ContainsKey(first.SupplierKey))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(first.LineNumber, "supplier.not_found", "supplier_key was not found."));
        }

        if (!binsByKey.ContainsKey(first.InventoryBinKey))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(first.LineNumber, "inventory_bin.not_found", "Active inventory bin key was not found."));
        }

        var seenPartKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            ValidateGroupValue(row, first, x => x.RequestKey, "purchase_request.mismatch", "Rows for the same order key must use the same request key.", issues);
            ValidateGroupValue(row, first, x => x.ReceiptKey, "receiving_receipt.mismatch", "Rows for the same order key must use the same receipt key.", issues);
            ValidateGroupValue(row, first, x => x.SupplierKey, "supplier.mismatch", "Rows for the same order key must use the same supplier key.", issues);
            ValidateGroupValue(row, first, x => x.InventoryBinKey, "inventory_bin.mismatch", "Rows for the same order key must use the same inventory bin key.", issues);

            ValidateLength(row.LineNumber, "part_key", row.PartKey, 2, 128, issues);
            if (!partsByKey.ContainsKey(row.PartKey))
            {
                issues.Add(new PurchaseHistoryCsvImportIssue(row.LineNumber, "part.not_found", "Part key was not found."));
            }

            if (!seenPartKeys.Add(row.PartKey))
            {
                issues.Add(new PurchaseHistoryCsvImportIssue(row.LineNumber, "part.duplicate_in_order", "Part key appears more than once for the same historical order."));
            }

            if (row.QuantityOrdered <= 0)
            {
                issues.Add(new PurchaseHistoryCsvImportIssue(row.LineNumber, "quantity.invalid", "quantity_ordered must be greater than zero."));
            }

            if (row.QuantityReceived <= 0)
            {
                issues.Add(new PurchaseHistoryCsvImportIssue(row.LineNumber, "quantity.invalid", "quantity_received must be greater than zero."));
            }

            if (row.QuantityReceived != row.QuantityOrdered)
            {
                issues.Add(new PurchaseHistoryCsvImportIssue(row.LineNumber, "purchase_history.partial_not_supported", "Initial purchase history import requires quantity_received to equal quantity_ordered."));
            }
        }
    }

    private static void ValidateGroupValue(
        ImportRow row,
        ImportRow first,
        Func<ImportRow, string> selector,
        string code,
        string message,
        List<PurchaseHistoryCsvImportIssue> issues)
    {
        if (!string.Equals(selector(row), selector(first), StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(row.LineNumber, code, message));
        }
    }

    private static void ValidateLength(
        int lineNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        List<PurchaseHistoryCsvImportIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new PurchaseHistoryCsvImportIssue(
                lineNumber,
                "csv.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static decimal ParseQuantity(
        string value,
        string column,
        int lineNumber,
        List<PurchaseHistoryCsvImportIssue> issues)
    {
        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return decimal.Round(parsed, 4, MidpointRounding.AwayFromZero);
        }

        issues.Add(new PurchaseHistoryCsvImportIssue(lineNumber, "csv.decimal", $"{column} must be a decimal number."));
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
        string ReceiptKey,
        string SupplierKey,
        string PartKey,
        decimal QuantityOrdered,
        decimal QuantityReceived,
        string InventoryBinKey,
        string Title,
        string LineNotes,
        string OrderNotes,
        string ReceiptNotes);
}
