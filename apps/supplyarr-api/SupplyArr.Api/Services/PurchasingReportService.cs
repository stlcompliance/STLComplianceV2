using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PurchasingReportService(SupplyArrDbContext db)
{
    private const int DetailListLimit = 50;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> BlockedApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "restricted",
        "inactive",
    };

    private static readonly HashSet<string> OpenPurchaseRequestStatuses =
    [
        PurchaseRequestStatuses.Draft,
        PurchaseRequestStatuses.Submitted,
    ];

    public async Task<PurchasingReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        bool? openDocumentsOnly,
        Guid? supplierId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var purchaseRequests = await db.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var purchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        if (supplierId is not null)
        {
            purchaseRequests = purchaseRequests
                .Where(x => x.SupplierId == supplierId)
                .ToList();
            purchaseOrders = purchaseOrders
                .Where(x => x.SupplierId == supplierId)
                .ToList();
        }

        var purchaseOrderIds = purchaseOrders.Select(x => x.Id).ToList();
        var receivingReceipts = purchaseOrderIds.Count == 0
            ? []
            : await db.ReceivingReceipts
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && purchaseOrderIds.Contains(x.PurchaseOrderId))
                .ToListAsync(cancellationToken);

        var backorders = purchaseOrderIds.Count == 0
            ? []
            : await db.Backorders
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && purchaseOrderIds.Contains(x.PurchaseOrderId))
                .ToListAsync(cancellationToken);

        var supplierIds = purchaseRequests
            .Where(x => x.SupplierId != null)
            .Select(x => x.SupplierId!.Value)
            .Concat(purchaseOrders.Select(x => x.SupplierId))
            .Distinct()
            .ToList();

        var suppliers = supplierIds.Count == 0
            ? []
            : await db.Suppliers
                .AsNoTracking()
                .Include(x => x.ParentSupplier)
                .Where(x => x.TenantId == tenantId && supplierIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var blockedSupplierCount = await db.Suppliers
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && true
                && x.Status == "active")
            .ToListAsync(cancellationToken);

        var procurementExceptionCount = await db.ProcurementExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var receivingExceptionCount = await db.ReceivingExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var warrantyClaimCount = await db.WarrantyClaims
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var supplierDocumentExpiringSoonCount = await db.SupplierComplianceDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var partSupplierLinks = await db.PartSupplierLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new
            {
                x.Id,
                x.SupplierId,
                x.PartId,
                x.CatalogUnitPrice,
                x.CatalogLeadTimeDays,
            })
            .ToListAsync(cancellationToken);

        var currentLeadTimeByLinkId = await db.PartSupplierLeadTimeSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.EffectiveFrom <= now
                && (x.EffectiveTo == null || x.EffectiveTo > now))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.PartSupplierLinkId,
                x.LeadTimeDays,
            })
            .ToListAsync(cancellationToken);

        var currentLeadTimeByLinkIdLookup = currentLeadTimeByLinkId
            .GroupBy(x => x.PartSupplierLinkId)
            .ToDictionary(g => g.Key, g => g.First().LeadTimeDays);

        var leadTimeSamples = partSupplierLinks
            .Select(link =>
            {
                if (currentLeadTimeByLinkIdLookup.TryGetValue(link.Id, out var leadTimeDays))
                {
                    return leadTimeDays > 0 ? (int?)leadTimeDays : null;
                }

                return link.CatalogLeadTimeDays is > 0 ? link.CatalogLeadTimeDays : null;
            })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        var averageLeadTimeDays = leadTimeSamples.Count > 0
            ? (int?)Math.Round(leadTimeSamples.Average())
            : null;

        var partSupplierLookup = partSupplierLinks.ToDictionary(
            x => (x.SupplierId, x.PartId),
            x => x);

        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1);
        var estimatedSpendThisMonth = purchaseOrders
            .Where(x => x.IssuedAt is DateTimeOffset issuedAt && issuedAt >= monthStart && issuedAt < monthEnd)
            .SelectMany(x => x.Lines.Select(line => new { PurchaseOrder = x, Line = line }))
            .Sum(item =>
            {
                if (!partSupplierLookup.TryGetValue((item.PurchaseOrder.SupplierId, item.Line.PartId), out var link)
                    || link.CatalogUnitPrice is not decimal unitPrice
                    || unitPrice <= 0)
                {
                    return 0m;
                }

                return unitPrice * item.Line.QuantityOrdered;
            });

        SupplierRecordSnapshot? Supplier(Guid? id) =>
            id is not null && suppliers.TryGetValue(id.Value, out var supplier)
                ? MapSupplierSnapshot(supplier)
                : null;

        var prLineCounts = await db.PurchaseRequestLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.PurchaseRequestId)
            .Select(g => new { PurchaseRequestId = g.Key, Count = g.Count(), Qty = g.Sum(l => l.QuantityRequested) })
            .ToDictionaryAsync(x => x.PurchaseRequestId, cancellationToken);

        var documents = new List<PurchasingDocumentSummaryItemResponse>();

        foreach (var pr in purchaseRequests.OrderByDescending(x => x.UpdatedAt))
        {
            prLineCounts.TryGetValue(pr.Id, out var prLines);
            var isOpen = OpenPurchaseRequestStatuses.Contains(pr.Status);
            if (openDocumentsOnly == true && !isOpen)
            {
                continue;
            }

            var supplier = Supplier(pr.SupplierId);
            documents.Add(new PurchasingDocumentSummaryItemResponse(
                "purchase_request",
                pr.Id,
                pr.RequestKey,
                pr.Title,
                pr.Status,
                supplier?.SupplierId,
                supplier?.SupplierKey,
                supplier?.SupplierDisplayName ?? string.Empty,
                supplier?.ParentSupplierId,
                supplier?.ParentSupplierDisplayName,
                supplier?.SupplierUnitKind,
                supplier?.SupplierServiceTypes ?? [],
                supplier?.SupplierType,
                prLines?.Count ?? 0,
                prLines?.Qty ?? 0m,
                0m,
                pr.UpdatedAt));
        }

        foreach (var po in purchaseOrders.OrderByDescending(x => x.UpdatedAt))
        {
            var lineCount = po.Lines.Count;
            var qtyOrdered = po.Lines.Sum(x => x.QuantityOrdered);
            var qtyReceived = po.Lines.Sum(x => x.QuantityReceived);
            var isOpen = PurchaseOrderStatuses.Open.Contains(po.Status)
                || string.Equals(po.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase);

            if (openDocumentsOnly == true && !isOpen)
            {
                continue;
            }

            var supplier = Supplier(po.SupplierId);
            documents.Add(new PurchasingDocumentSummaryItemResponse(
                "purchase_order",
                po.Id,
                po.OrderKey,
                po.Title,
                po.Status,
                supplier?.SupplierId,
                supplier?.SupplierKey,
                supplier?.SupplierDisplayName ?? string.Empty,
                supplier?.ParentSupplierId,
                supplier?.ParentSupplierDisplayName,
                supplier?.SupplierUnitKind,
                supplier?.SupplierServiceTypes ?? [],
                supplier?.SupplierType,
                lineCount,
                qtyOrdered,
                qtyReceived,
                po.UpdatedAt));
        }

        documents = documents.OrderByDescending(x => x.UpdatedAt).ToList();

        var totals = new PurchasingReportTotalsResponse(
            purchaseRequests.Count,
            purchaseRequests.Count(x => OpenPurchaseRequestStatuses.Contains(x.Status)),
            purchaseOrders.Count,
            purchaseOrders.Count(x => PurchaseOrderStatuses.Open.Contains(x.Status)),
            purchaseOrders.Count(x =>
                string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase)),
            receivingReceipts.Count(x =>
                string.Equals(x.Status, ReceivingReceiptStatuses.Draft, StringComparison.OrdinalIgnoreCase)),
            receivingReceipts.Count(x =>
                ReceivingReceiptStatuses.IsPostedLike(x.Status)),
            backorders.Count(x =>
                string.Equals(x.Status, BackorderStatuses.Open, StringComparison.OrdinalIgnoreCase)),
            purchaseOrders
                .Where(x => PurchaseOrderStatuses.Open.Contains(x.Status)
                    || string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Lines)
                .Sum(x => x.QuantityOrdered),
            purchaseOrders.SelectMany(x => x.Lines).Sum(x => x.QuantityReceived));

        var analytics = new PurchasingProcurementAnalyticsResponse(
            purchaseRequests.Count(x => string.Equals(x.Status, PurchaseRequestStatuses.Submitted, StringComparison.OrdinalIgnoreCase)),
            purchaseRequests.Count(x => x.IsEmergency),
            procurementExceptionCount.Count(x => ProcurementExceptionStatuses.Active.Contains(x.Status)),
            receivingExceptionCount.Count(x => string.Equals(x.Status, ReceivingExceptionStatuses.Open, StringComparison.OrdinalIgnoreCase)),
            warrantyClaimCount.Count(x => WarrantyClaimStatuses.Open.Contains(x.Status)),
            supplierDocumentExpiringSoonCount.Count(x =>
                x.ReviewStatus == SupplierComplianceDocumentReviewStatuses.Approved
                && x.ExpiresAt != null
                && x.ExpiresAt >= now
                && x.ExpiresAt <= now.AddDays(30)),
            blockedSupplierCount.Count(x => BlockedApprovalStatuses.Contains(x.ApprovalStatus)),
            averageLeadTimeDays,
            estimatedSpendThisMonth);

        var prStatusCounts = purchaseRequests
            .GroupBy(x => x.Status)
            .Select(g => new PurchasingStatusCountResponse(g.Key, g.Count()))
            .OrderBy(x => x.Status)
            .ToList();

        var poStatusCounts = purchaseOrders
            .GroupBy(x => x.Status)
            .Select(g => new PurchasingStatusCountResponse(g.Key, g.Count()))
            .OrderBy(x => x.Status)
            .ToList();

        return new PurchasingReportSummaryResponse(
            DateTimeOffset.UtcNow,
            totals,
            analytics,
            prStatusCounts,
            poStatusCounts,
            documents);
    }

    public async Task<PurchasingPurchaseRequestDetailResponse> GetPurchaseRequestDetailAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default)
    {
        var pr = await db.PurchaseRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == purchaseRequestId, cancellationToken)
            ?? throw new StlApiException("purchase_request.not_found", "Purchase request was not found.", 404);

        var summaryResponse = await GetSummaryAsync(tenantId, null, pr.SupplierId, cancellationToken);
        var summary = summaryResponse.Documents.FirstOrDefault(x =>
                x.DocumentType == "purchase_request" && x.DocumentId == purchaseRequestId)
            ?? await BuildPurchaseRequestSummaryAsync(tenantId, pr, cancellationToken);

        var lines = await (
            from line in db.PurchaseRequestLines.AsNoTracking()
            join part in db.Parts.AsNoTracking() on line.PartId equals part.Id
            where line.TenantId == tenantId && line.PurchaseRequestId == purchaseRequestId
            orderby line.LineNumber
            select new PurchasingRequestLineRowResponse(
                line.Id,
                line.LineNumber,
                part.Id,
                part.PartKey,
                part.DisplayName,
                line.QuantityRequested,
                line.UnitOfMeasure))
            .Take(DetailListLimit)
            .ToListAsync(cancellationToken);

        var linkedPo = await db.PurchaseOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PurchaseRequestId == purchaseRequestId,
                cancellationToken);

        return new PurchasingPurchaseRequestDetailResponse(
            summary,
            lines,
            linkedPo?.Id,
            linkedPo?.OrderKey);
    }

    public async Task<PurchasingPurchaseOrderDetailResponse> GetPurchaseOrderDetailAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        var po = await db.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == purchaseOrderId, cancellationToken)
            ?? throw new StlApiException("purchase_order.not_found", "Purchase order was not found.", 404);

        var summaryResponse = await GetSummaryAsync(tenantId, null, po.SupplierId, cancellationToken);
        var summary = summaryResponse.Documents.FirstOrDefault(x =>
                x.DocumentType == "purchase_order" && x.DocumentId == purchaseOrderId)
            ?? await BuildPurchaseOrderSummaryAsync(tenantId, po, cancellationToken);

        var lines = await (
            from line in db.PurchaseOrderLines.AsNoTracking()
            join part in db.Parts.AsNoTracking() on line.PartId equals part.Id
            where line.TenantId == tenantId && line.PurchaseOrderId == purchaseOrderId
            orderby line.LineNumber
            select new PurchasingOrderLineRowResponse(
                line.Id,
                line.LineNumber,
                part.Id,
                part.PartKey,
                part.DisplayName,
                line.QuantityOrdered,
                line.QuantityReceived,
                line.QuantityOrdered - line.QuantityReceived))
            .Take(DetailListLimit)
            .ToListAsync(cancellationToken);

        var receipts = await db.ReceivingReceipts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => new PurchasingReceivingRowResponse(
                x.Id,
                x.ReceiptKey,
                x.Status,
                x.PostedAt))
            .ToListAsync(cancellationToken);

        var backorders = await db.Backorders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PurchaseOrderId == purchaseOrderId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => new PurchasingBackorderRowResponse(
                x.Id,
                x.BackorderKey,
                x.Status,
                x.QuantityBackordered,
                x.QuantityFulfilled))
            .ToListAsync(cancellationToken);

        return new PurchasingPurchaseOrderDetailResponse(summary, lines, receipts, backorders);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        bool? openDocumentsOnly,
        Guid? supplierId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, openDocumentsOnly, supplierId, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "documentType,documentKey,title,status,supplierKey,supplierDisplayName,lineCount,quantityOrdered,quantityReceived,updatedAt");

        foreach (var doc in summary.Documents)
        {
            builder.Append(CsvEscape(doc.DocumentType));
            builder.Append(',');
            builder.Append(CsvEscape(doc.DocumentKey));
            builder.Append(',');
            builder.Append(CsvEscape(doc.Title));
            builder.Append(',');
            builder.Append(CsvEscape(doc.Status));
            builder.Append(',');
            builder.Append(CsvEscape(doc.SupplierKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(doc.SupplierDisplayName));
            builder.Append(',');
            builder.Append(doc.LineCount);
            builder.Append(',');
            builder.Append(doc.QuantityOrdered);
            builder.Append(',');
            builder.Append(doc.QuantityReceived);
            builder.Append(',');
            builder.AppendLine(doc.UpdatedAt.ToString("O"));
        }

        var fileName = $"supplyarr-purchasing-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private async Task<PurchasingDocumentSummaryItemResponse> BuildPurchaseRequestSummaryAsync(
        Guid tenantId,
        PurchaseRequest pr,
        CancellationToken cancellationToken)
    {
        var supplier = pr.SupplierId is null
            ? null
            : await db.Suppliers
                .AsNoTracking()
                .Include(x => x.ParentSupplier)
                .Where(x => x.TenantId == tenantId && x.Id == pr.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

        var lineStats = await db.PurchaseRequestLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PurchaseRequestId == pr.Id)
            .GroupBy(x => x.PurchaseRequestId)
            .Select(g => new { Count = g.Count(), Qty = g.Sum(l => l.QuantityRequested) })
            .FirstOrDefaultAsync(cancellationToken);

        var supplierSnapshot = supplier is null ? null : MapSupplierSnapshot(supplier);

        return new PurchasingDocumentSummaryItemResponse(
            "purchase_request",
            pr.Id,
            pr.RequestKey,
            pr.Title,
            pr.Status,
            supplierSnapshot?.SupplierId,
            supplierSnapshot?.SupplierKey,
            supplierSnapshot?.SupplierDisplayName ?? string.Empty,
            supplierSnapshot?.ParentSupplierId,
            supplierSnapshot?.ParentSupplierDisplayName,
            supplierSnapshot?.SupplierUnitKind,
            supplierSnapshot?.SupplierServiceTypes ?? [],
            supplierSnapshot?.SupplierType,
            lineStats?.Count ?? 0,
            lineStats?.Qty ?? 0m,
            0m,
            pr.UpdatedAt);
    }

    private async Task<PurchasingDocumentSummaryItemResponse> BuildPurchaseOrderSummaryAsync(
        Guid tenantId,
        PurchaseOrder po,
        CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .Include(x => x.ParentSupplier)
            .Where(x => x.TenantId == tenantId && x.Id == po.SupplierId)
            .FirstOrDefaultAsync(cancellationToken);

        var supplierSnapshot = supplier is null ? null : MapSupplierSnapshot(supplier);

        return new PurchasingDocumentSummaryItemResponse(
            "purchase_order",
            po.Id,
            po.OrderKey,
            po.Title,
            po.Status,
            supplierSnapshot?.SupplierId,
            supplierSnapshot?.SupplierKey,
            supplierSnapshot?.SupplierDisplayName ?? string.Empty,
            supplierSnapshot?.ParentSupplierId,
            supplierSnapshot?.ParentSupplierDisplayName,
            supplierSnapshot?.SupplierUnitKind,
            supplierSnapshot?.SupplierServiceTypes ?? [],
            supplierSnapshot?.SupplierType,
            po.Lines.Count,
            po.Lines.Sum(x => x.QuantityOrdered),
            po.Lines.Sum(x => x.QuantityReceived),
            po.UpdatedAt);
    }

    private static SupplierRecordSnapshot MapSupplierSnapshot(Supplier supplier) =>
        new(
            supplier.Id,
            supplier.SupplierKey,
            supplier.DisplayName,
            supplier.ParentSupplierId,
            supplier.ParentSupplier?.DisplayName,
            supplier.UnitKind,
            DeserializeServiceTypes(supplier.ServiceTypesJson),
            "supplier");

    private static IReadOnlyList<string> DeserializeServiceTypes(string? serviceTypesJson)
    {
        if (string.IsNullOrWhiteSpace(serviceTypesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(serviceTypesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private sealed record SupplierRecordSnapshot(
        Guid SupplierId,
        string SupplierKey,
        string SupplierDisplayName,
        Guid? ParentSupplierId,
        string? ParentSupplierDisplayName,
        string? SupplierUnitKind,
        IReadOnlyList<string> SupplierServiceTypes,
        string SupplierType);
}


