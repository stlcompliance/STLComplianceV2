using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class SupplyReadinessService(SupplyArrDbContext db)
{
    private const int AttentionItemLimit = 25;
    private static readonly TimeSpan ComplianceExpiringSoonWindow = TimeSpan.FromDays(30);

    private static readonly HashSet<string> OpenPurchaseRequestStatuses =
    [
        PurchaseRequestStatuses.Draft,
        PurchaseRequestStatuses.Submitted,
    ];

    private static readonly HashSet<string> OpenPurchaseOrderStatuses =
    [
        PurchaseOrderStatuses.Draft,
        PurchaseOrderStatuses.Approved,
    ];

    public async Task<SupplyReadinessDashboardResponse> GetDashboardAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var parts = await db.Parts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var activeParts = parts
            .Where(x => string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var stockRows = await db.PartStockLevels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.PartId, x.QuantityOnHand, x.QuantityReserved })
            .ToListAsync(cancellationToken);

        var stockByPart = stockRows
            .GroupBy(x => x.PartId)
            .ToDictionary(
                g => g.Key,
                g => (
                    OnHand: g.Sum(x => x.QuantityOnHand),
                    Reserved: g.Sum(x => x.QuantityReserved)));

        var partsBelowReorder = activeParts.Count(part =>
        {
            if (part.ReorderPoint is not { } reorderPoint)
            {
                return false;
            }

            stockByPart.TryGetValue(part.Id, out var stock);
            var available = stock.OnHand - stock.Reserved;
            return available < reorderPoint;
        });

        var totalOnHand = stockRows.Sum(x => x.QuantityOnHand);
        var totalReserved = stockRows.Sum(x => x.QuantityReserved);

        var openBackorderCount = await db.Backorders.CountAsync(
            x => x.TenantId == tenantId && x.Status == BackorderStatuses.Open,
            cancellationToken);

        var purchaseRequests = await db.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new DocumentRow(
                x.Id,
                x.RequestKey,
                x.Title,
                x.Status,
                x.UpdatedAt,
                null))
            .ToListAsync(cancellationToken);

        var openPurchaseRequestCount = purchaseRequests.Count(x =>
            OpenPurchaseRequestStatuses.Contains(x.Status));

        var purchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new DocumentRow(
                x.Id,
                x.OrderKey,
                x.Title,
                x.Status,
                x.UpdatedAt,
                x.IssuedAt))
            .ToListAsync(cancellationToken);

        var openPurchaseOrderCount = purchaseOrders.Count(x =>
            OpenPurchaseOrderStatuses.Contains(x.Status));

        var issuedPurchaseOrderCount = purchaseOrders.Count(x =>
            string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase));

        var maintainarrOpen = await db.MaintainArrDemandRefs.CountAsync(
            x => x.TenantId == tenantId
                && (x.Status == MaintainArrDemandRefStatuses.Received
                    || x.Status == MaintainArrDemandRefStatuses.PrDrafted),
            cancellationToken);
        var routarrOpen = await db.RoutArrDemandRefs.CountAsync(
            x => x.TenantId == tenantId
                && (x.Status == RoutArrDemandRefStatuses.Received
                    || x.Status == RoutArrDemandRefStatuses.PrDrafted),
            cancellationToken);
        var trainarrOpen = await db.TrainArrDemandRefs.CountAsync(
            x => x.TenantId == tenantId
                && (x.Status == TrainArrDemandRefStatuses.Received
                    || x.Status == TrainArrDemandRefStatuses.PrDrafted),
            cancellationToken);
        var staffarrOpen = await db.StaffArrDemandRefs.CountAsync(
            x => x.TenantId == tenantId
                && (x.Status == StaffArrDemandRefStatuses.Received
                    || x.Status == StaffArrDemandRefStatuses.PrDrafted),
            cancellationToken);

        var openDemandRefCount = maintainarrOpen + routarrOpen + trainarrOpen + staffarrOpen;

        var complianceDocuments = await db.PartyComplianceDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new ComplianceDocRow(
                x.Id,
                x.ExternalPartyId,
                x.DocumentKey,
                x.ReviewStatus,
                x.ExpiresAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var complianceAttentionCount = complianceDocuments.Count(doc =>
            IsComplianceAttention(doc.ReviewStatus, doc.ExpiresAt, now));

        var activeVendorRestrictionCount = await db.VendorRestrictions.CountAsync(
            x => x.TenantId == tenantId && x.Status == VendorRestrictionStatuses.Active,
            cancellationToken);

        var procurementExceptions = await db.ProcurementExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && ProcurementExceptionStatuses.Active.Contains(x.Status))
            .Select(x => new DocumentRow(
                x.Id,
                x.ExceptionKey,
                x.Title,
                x.Status,
                x.UpdatedAt,
                null))
            .ToListAsync(cancellationToken);

        var activeProcurementExceptionCount = procurementExceptions.Count;

        var backorders = openBackorderCount > 0
            ? await db.Backorders
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Status == BackorderStatuses.Open)
                .OrderByDescending(x => x.UpdatedAt)
                .Take(AttentionItemLimit)
                .Select(x => new DocumentRow(
                    x.Id,
                    x.BackorderKey,
                    x.Notes,
                    x.Status,
                    x.UpdatedAt,
                    null))
                .ToListAsync(cancellationToken)
            : [];

        var attentionItems = BuildAttentionItems(
            now,
            activeParts,
            stockByPart,
            purchaseRequests,
            purchaseOrders,
            complianceDocuments,
            backorders,
            procurementExceptions,
            openBackorderCount,
            activeVendorRestrictionCount,
            activeProcurementExceptionCount);

        return new SupplyReadinessDashboardResponse(
            now,
            new SupplyReadinessTotalsResponse(
                activeParts.Count,
                partsBelowReorder,
                stockRows.Count,
                totalOnHand,
                totalReserved,
                totalOnHand - totalReserved,
                openBackorderCount,
                openPurchaseRequestCount,
                openPurchaseOrderCount,
                issuedPurchaseOrderCount,
                openDemandRefCount,
                complianceAttentionCount,
                activeVendorRestrictionCount,
                activeProcurementExceptionCount),
            [
                new(DemandRefSources.MaintainArr, maintainarrOpen),
                new(DemandRefSources.RoutArr, routarrOpen),
                new(DemandRefSources.TrainArr, trainarrOpen),
                new(DemandRefSources.StaffArr, staffarrOpen),
            ],
            attentionItems);
    }

    private static bool IsComplianceAttention(string reviewStatus, DateTimeOffset? expiresAt, DateTimeOffset now)
    {
        if (string.Equals(reviewStatus, PartyComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (expiresAt is null)
        {
            return false;
        }

        if (expiresAt.Value <= now)
        {
            return true;
        }

        return expiresAt.Value <= now.Add(ComplianceExpiringSoonWindow);
    }

    private static List<SupplyReadinessAttentionItemResponse> BuildAttentionItems(
        DateTimeOffset now,
        IReadOnlyList<Part> activeParts,
        IReadOnlyDictionary<Guid, (decimal OnHand, decimal Reserved)> stockByPart,
        IReadOnlyList<DocumentRow> purchaseRequests,
        IReadOnlyList<DocumentRow> purchaseOrders,
        IReadOnlyList<ComplianceDocRow> complianceDocuments,
        IReadOnlyList<DocumentRow> backorders,
        IReadOnlyList<DocumentRow> procurementExceptions,
        int openBackorderCount,
        int activeVendorRestrictionCount,
        int activeProcurementExceptionCount)
    {
        var items = new List<SupplyReadinessAttentionItemResponse>();

        foreach (var part in activeParts)
        {
            if (part.ReorderPoint is not { } reorderPoint)
            {
                continue;
            }

            stockByPart.TryGetValue(part.Id, out var stock);
            var available = stock.OnHand - stock.Reserved;
            if (available >= reorderPoint)
            {
                continue;
            }

            items.Add(new SupplyReadinessAttentionItemResponse(
                "stock",
                $"{part.PartKey} below reorder",
                $"Available {available} · reorder point {reorderPoint}",
                "below_reorder",
                part.UpdatedAt,
                "part",
                part.Id));
        }

        foreach (var row in purchaseRequests.Where(x => OpenPurchaseRequestStatuses.Contains(x.Status))
            .OrderByDescending(x => x.UpdatedAt)
            .Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "procurement",
                $"PR {row.Key} · {row.Status}",
                row.Title,
                row.Status,
                row.UpdatedAt,
                "purchase_request",
                row.Id));
        }

        foreach (var row in purchaseOrders.Where(x =>
                OpenPurchaseOrderStatuses.Contains(x.Status)
                    || string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.IssuedAt ?? x.UpdatedAt)
            .Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "procurement",
                $"PO {row.Key} · {row.Status}",
                row.Title,
                row.Status,
                row.IssuedAt ?? row.UpdatedAt,
                "purchase_order",
                row.Id));
        }

        foreach (var row in backorders.Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "backorder",
                $"Backorder {row.Key}",
                string.IsNullOrWhiteSpace(row.Title) ? "Open backorder" : row.Title,
                row.Status,
                row.UpdatedAt,
                "backorder",
                row.Id));
        }

        if (openBackorderCount > backorders.Count)
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "backorder",
                $"{openBackorderCount} open backorders",
                "Review receiving and purchasing queues for fulfillment.",
                "summary",
                now,
                null,
                null));
        }

        foreach (var doc in complianceDocuments.Where(x => IsComplianceAttention(x.ReviewStatus, x.ExpiresAt, now))
            .OrderByDescending(x => x.UpdatedAt)
            .Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "compliance",
                $"Compliance doc {doc.DocumentKey}",
                $"Review status {doc.ReviewStatus}",
                doc.ReviewStatus,
                doc.UpdatedAt,
                "party_compliance_document",
                doc.Id));
        }

        foreach (var row in procurementExceptions.Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "exception",
                $"Exception {row.Key}",
                row.Title,
                row.Status,
                row.UpdatedAt,
                "procurement_exception",
                row.Id));
        }

        if (activeVendorRestrictionCount > 0)
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "restriction",
                $"{activeVendorRestrictionCount} active vendor restrictions",
                "Procurement may be blocked for affected vendors.",
                "summary",
                now,
                null,
                null));
        }

        if (activeProcurementExceptionCount > procurementExceptions.Count)
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "exception",
                $"{activeProcurementExceptionCount} active procurement exceptions",
                "Investigate open exceptions on PR/PO/RFQ subjects.",
                "summary",
                now,
                null,
                null));
        }

        return items
            .OrderByDescending(x => x.OccurredAt ?? DateTimeOffset.MinValue)
            .Take(AttentionItemLimit)
            .ToList();
    }

    private sealed record DocumentRow(
        Guid Id,
        string Key,
        string Title,
        string Status,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? IssuedAt);

    private sealed record ComplianceDocRow(
        Guid Id,
        Guid ExternalPartyId,
        string DocumentKey,
        string ReviewStatus,
        DateTimeOffset? ExpiresAt,
        DateTimeOffset UpdatedAt);
}
