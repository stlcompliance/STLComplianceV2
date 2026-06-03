using System.Text;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PurchasingReportService(SupplyArrDbContext db)
{
    private const int DetailListLimit = 50;

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
        Guid? vendorPartyId,
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

        if (vendorPartyId is not null)
        {
            purchaseRequests = purchaseRequests
                .Where(x => x.VendorPartyId == vendorPartyId)
                .ToList();
            purchaseOrders = purchaseOrders
                .Where(x => x.VendorPartyId == vendorPartyId)
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

        var vendorIds = purchaseRequests
            .Where(x => x.VendorPartyId != null)
            .Select(x => x.VendorPartyId!.Value)
            .Concat(purchaseOrders.Select(x => x.VendorPartyId))
            .Distinct()
            .ToList();

        var vendors = vendorIds.Count == 0
            ? []
            : await db.ExternalParties
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && vendorIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var blockedVendorCount = await db.ExternalParties
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && (x.PartyType == "vendor" || x.PartyType == "supplier")
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

        var vendorDocumentExpiringSoonCount = await db.PartyComplianceDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var partVendorLinks = await db.PartVendorLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new
            {
                x.Id,
                x.ExternalPartyId,
                x.PartId,
                x.CatalogUnitPrice,
                x.CatalogLeadTimeDays,
            })
            .ToListAsync(cancellationToken);

        var currentLeadTimeByLinkId = await db.PartVendorLeadTimeSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.EffectiveFrom <= now
                && (x.EffectiveTo == null || x.EffectiveTo > now))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.PartVendorLinkId,
                x.LeadTimeDays,
            })
            .ToListAsync(cancellationToken);

        var currentLeadTimeByLinkIdLookup = currentLeadTimeByLinkId
            .GroupBy(x => x.PartVendorLinkId)
            .ToDictionary(g => g.Key, g => g.First().LeadTimeDays);

        var leadTimeSamples = partVendorLinks
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

        var partVendorLookup = partVendorLinks.ToDictionary(
            x => (x.ExternalPartyId, x.PartId),
            x => x);

        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1);
        var estimatedSpendThisMonth = purchaseOrders
            .Where(x => x.IssuedAt is DateTimeOffset issuedAt && issuedAt >= monthStart && issuedAt < monthEnd)
            .SelectMany(x => x.Lines.Select(line => new { PurchaseOrder = x, Line = line }))
            .Sum(item =>
            {
                if (!partVendorLookup.TryGetValue((item.PurchaseOrder.VendorPartyId, item.Line.PartId), out var link)
                    || link.CatalogUnitPrice is not decimal unitPrice
                    || unitPrice <= 0)
                {
                    return 0m;
                }

                return unitPrice * item.Line.QuantityOrdered;
            });

        string VendorName(Guid? id) =>
            id is not null && vendors.TryGetValue(id.Value, out var party)
                ? party.DisplayName
                : string.Empty;

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

            documents.Add(new PurchasingDocumentSummaryItemResponse(
                "purchase_request",
                pr.Id,
                pr.RequestKey,
                pr.Title,
                pr.Status,
                pr.VendorPartyId,
                VendorName(pr.VendorPartyId),
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

            documents.Add(new PurchasingDocumentSummaryItemResponse(
                "purchase_order",
                po.Id,
                po.OrderKey,
                po.Title,
                po.Status,
                po.VendorPartyId,
                VendorName(po.VendorPartyId),
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
            vendorDocumentExpiringSoonCount.Count(x =>
                x.ReviewStatus == PartyComplianceDocumentReviewStatuses.Approved
                && x.ExpiresAt != null
                && x.ExpiresAt >= now
                && x.ExpiresAt <= now.AddDays(30)),
            blockedVendorCount.Count(x => BlockedApprovalStatuses.Contains(x.ApprovalStatus)),
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

        var summaryResponse = await GetSummaryAsync(tenantId, null, pr.VendorPartyId, cancellationToken);
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

        var summaryResponse = await GetSummaryAsync(tenantId, null, po.VendorPartyId, cancellationToken);
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
        Guid? vendorPartyId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, openDocumentsOnly, vendorPartyId, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "documentType,documentKey,title,status,vendorDisplayName,lineCount,quantityOrdered,quantityReceived,updatedAt");

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
            builder.Append(CsvEscape(doc.VendorDisplayName));
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
        var vendorName = pr.VendorPartyId is null
            ? string.Empty
            : await db.ExternalParties
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == pr.VendorPartyId)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var lineStats = await db.PurchaseRequestLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PurchaseRequestId == pr.Id)
            .GroupBy(x => x.PurchaseRequestId)
            .Select(g => new { Count = g.Count(), Qty = g.Sum(l => l.QuantityRequested) })
            .FirstOrDefaultAsync(cancellationToken);

        return new PurchasingDocumentSummaryItemResponse(
            "purchase_request",
            pr.Id,
            pr.RequestKey,
            pr.Title,
            pr.Status,
            pr.VendorPartyId,
            vendorName,
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
        var vendorName = await db.ExternalParties
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == po.VendorPartyId)
            .Select(x => x.DisplayName)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return new PurchasingDocumentSummaryItemResponse(
            "purchase_order",
            po.Id,
            po.OrderKey,
            po.Title,
            po.Status,
            po.VendorPartyId,
            vendorName,
            po.Lines.Count,
            po.Lines.Sum(x => x.QuantityOrdered),
            po.Lines.Sum(x => x.QuantityReceived),
            po.UpdatedAt);
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
