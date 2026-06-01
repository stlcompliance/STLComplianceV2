using System.Text;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class VendorReportService(SupplyArrDbContext db)
{
    private const int DetailListLimit = 25;
    private static readonly string[] PostedReceivingStatuses =
    [
        ReceivingReceiptStatuses.Posted,
        ReceivingReceiptStatuses.Received,
        ReceivingReceiptStatuses.PartiallyReceived,
        ReceivingReceiptStatuses.Overreceived,
        ReceivingReceiptStatuses.Underreceived,
        ReceivingReceiptStatuses.Damaged,
        ReceivingReceiptStatuses.WrongItem,
        ReceivingReceiptStatuses.PendingInspection,
        ReceivingReceiptStatuses.Quarantined,
        ReceivingReceiptStatuses.Returned,
        ReceivingReceiptStatuses.Closed,
    ];

    public async Task<VendorReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? approvalStatus,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var vendors = await db.ExternalParties
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PartyType == "vendor")
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(approvalStatus))
        {
            var normalized = approvalStatus.Trim().ToLowerInvariant();
            vendors = vendors
                .Where(x => string.Equals(x.ApprovalStatus, normalized, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (activeOnly == true)
        {
            vendors = vendors
                .Where(x => string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var vendorIds = vendors.Select(x => x.Id).ToList();
        if (vendorIds.Count == 0)
        {
            return new VendorReportSummaryResponse(
                DateTimeOffset.UtcNow,
                [],
                []);
        }

        var linkStats = await db.PartVendorLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && vendorIds.Contains(x.ExternalPartyId))
            .GroupBy(x => x.ExternalPartyId)
            .Select(g => new
            {
                VendorPartyId = g.Key,
                Count = g.Count(),
                PreferredCount = g.Count(x => x.IsPreferred),
            })
            .ToListAsync(cancellationToken);

        var prStats = await db.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.VendorPartyId != null
                && vendorIds.Contains(x.VendorPartyId.Value)
                && (x.Status == PurchaseRequestStatuses.Draft || x.Status == PurchaseRequestStatuses.Submitted))
            .GroupBy(x => x.VendorPartyId!.Value)
            .Select(g => new { VendorPartyId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var poStats = await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && vendorIds.Contains(x.VendorPartyId))
            .Select(x => new
            {
                x.VendorPartyId,
                x.Status,
                x.UpdatedAt,
                LineQuantity = x.Lines.Sum(line => line.QuantityOrdered),
            })
            .ToListAsync(cancellationToken);

        var poGrouped = poStats
            .GroupBy(x => x.VendorPartyId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var receivingStats = await (
            from receipt in db.ReceivingReceipts.AsNoTracking()
            join purchaseOrder in db.PurchaseOrders.AsNoTracking()
                on receipt.PurchaseOrderId equals purchaseOrder.Id
            where receipt.TenantId == tenantId
                && PostedReceivingStatuses.Contains(receipt.Status)
                && vendorIds.Contains(purchaseOrder.VendorPartyId)
            select new
            {
                purchaseOrder.VendorPartyId,
                receipt.PostedAt,
            }).ToListAsync(cancellationToken);

        var receivingGrouped = receivingStats
            .GroupBy(x => x.VendorPartyId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Count = g.Count(),
                    LastPostedAt = g.Max(x => x.PostedAt),
                });

        var backorderStats = await (
            from backorder in db.Backorders.AsNoTracking()
            join purchaseOrder in db.PurchaseOrders.AsNoTracking()
                on backorder.PurchaseOrderId equals purchaseOrder.Id
            where backorder.TenantId == tenantId
                && backorder.Status == BackorderStatuses.Open
                && vendorIds.Contains(purchaseOrder.VendorPartyId)
            group backorder by purchaseOrder.VendorPartyId
            into grouped
            select new { VendorPartyId = grouped.Key, Count = grouped.Count() })
            .ToListAsync(cancellationToken);

        var linkByVendor = linkStats.ToDictionary(x => x.VendorPartyId);
        var prByVendor = prStats.ToDictionary(x => x.VendorPartyId);
        var backorderByVendor = backorderStats.ToDictionary(x => x.VendorPartyId);

        var items = vendors.Select(vendor =>
        {
            linkByVendor.TryGetValue(vendor.Id, out var links);
            prByVendor.TryGetValue(vendor.Id, out var openPr);
            backorderByVendor.TryGetValue(vendor.Id, out var openBackorders);
            poGrouped.TryGetValue(vendor.Id, out var pos);
            receivingGrouped.TryGetValue(vendor.Id, out var receiving);

            var openPoCount = pos?.Count(x => PurchaseOrderStatuses.Open.Contains(x.Status)) ?? 0;
            var issuedPoCount = pos?.Count(x =>
                string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase)) ?? 0;
            var openLineQty = pos?
                .Where(x => PurchaseOrderStatuses.Open.Contains(x.Status)
                    || string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.LineQuantity) ?? 0m;
            var lastPoAt = pos?.Max(x => (DateTimeOffset?)x.UpdatedAt);

            return new VendorReportSummaryItemResponse(
                vendor.Id,
                vendor.PartyKey,
                vendor.DisplayName,
                vendor.ApprovalStatus,
                vendor.Status,
                links?.Count ?? 0,
                links?.PreferredCount ?? 0,
                openPr?.Count ?? 0,
                openPoCount,
                issuedPoCount,
                receiving?.Count ?? 0,
                openBackorders?.Count ?? 0,
                openLineQty,
                lastPoAt,
                receiving?.LastPostedAt);
        }).ToList();

        var approvalCounts = vendors
            .GroupBy(x => x.ApprovalStatus)
            .Select(g => new VendorApprovalStatusSummaryResponse(g.Key, g.Count()))
            .OrderBy(x => x.ApprovalStatus)
            .ToList();

        return new VendorReportSummaryResponse(DateTimeOffset.UtcNow, approvalCounts, items);
    }

    public async Task<VendorReportDetailResponse> GetDetailAsync(
        Guid tenantId,
        Guid vendorPartyId,
        CancellationToken cancellationToken = default)
    {
        var vendor = await db.ExternalParties
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == vendorPartyId && x.PartyType == "vendor",
                cancellationToken)
            ?? throw new StlApiException("vendor.not_found", "Vendor was not found.", 404);

        var summaryResponse = await GetSummaryAsync(tenantId, null, null, cancellationToken);
        var summary = summaryResponse.Vendors.FirstOrDefault(x => x.VendorPartyId == vendorPartyId)
            ?? MapSingleVendorSummary(vendor);

        var recentPurchaseRequests = await db.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VendorPartyId == vendorPartyId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => new VendorReportPurchaseRequestRowResponse(
                x.Id,
                x.RequestKey,
                x.Title,
                x.Status,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var recentPurchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VendorPartyId == vendorPartyId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => new VendorReportPurchaseOrderRowResponse(
                x.Id,
                x.OrderKey,
                x.Title,
                x.Status,
                x.Lines.Count,
                x.Lines.Sum(line => line.QuantityOrdered),
                x.Lines.Sum(line => line.QuantityReceived),
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var partLinks = await db.PartVendorLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExternalPartyId == vendorPartyId)
            .OrderByDescending(x => x.IsPreferred)
            .ThenBy(x => x.VendorPartNumber)
            .Take(DetailListLimit)
            .Select(x => new VendorReportPartLinkRowResponse(
                x.Id,
                x.PartId,
                x.Part.PartKey,
                x.Part.DisplayName,
                x.VendorPartNumber,
                x.IsPreferred,
                x.CatalogUnitPrice,
                x.CatalogAvailabilityStatus))
            .ToListAsync(cancellationToken);

        return new VendorReportDetailResponse(summary, recentPurchaseRequests, recentPurchaseOrders, partLinks);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        string? approvalStatus,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, approvalStatus, activeOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "partyKey,displayName,approvalStatus,status,partVendorLinkCount,preferredPartLinkCount,openPurchaseRequestCount,openPurchaseOrderCount,issuedPurchaseOrderCount,postedReceivingReceiptCount,openBackorderCount,openPurchaseOrderLineQuantity,lastPurchaseOrderAt,lastReceivingPostedAt");

        foreach (var vendor in summary.Vendors)
        {
            builder.Append(CsvEscape(vendor.PartyKey));
            builder.Append(',');
            builder.Append(CsvEscape(vendor.DisplayName));
            builder.Append(',');
            builder.Append(CsvEscape(vendor.ApprovalStatus));
            builder.Append(',');
            builder.Append(CsvEscape(vendor.Status));
            builder.Append(',');
            builder.Append(vendor.PartVendorLinkCount);
            builder.Append(',');
            builder.Append(vendor.PreferredPartLinkCount);
            builder.Append(',');
            builder.Append(vendor.OpenPurchaseRequestCount);
            builder.Append(',');
            builder.Append(vendor.OpenPurchaseOrderCount);
            builder.Append(',');
            builder.Append(vendor.IssuedPurchaseOrderCount);
            builder.Append(',');
            builder.Append(vendor.PostedReceivingReceiptCount);
            builder.Append(',');
            builder.Append(vendor.OpenBackorderCount);
            builder.Append(',');
            builder.Append(vendor.OpenPurchaseOrderLineQuantity);
            builder.Append(',');
            builder.Append(vendor.LastPurchaseOrderAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.AppendLine(vendor.LastReceivingPostedAt?.ToString("O") ?? string.Empty);
        }

        var fileName = $"supplyarr-vendor-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static VendorReportSummaryItemResponse MapSingleVendorSummary(ExternalParty vendor) =>
        new(
            vendor.Id,
            vendor.PartyKey,
            vendor.DisplayName,
            vendor.ApprovalStatus,
            vendor.Status,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0m,
            null,
            null);

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
