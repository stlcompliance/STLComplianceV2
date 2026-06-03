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
        var now = DateTimeOffset.UtcNow;
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
                now,
                [],
                []);
        }

        var vendorLinks = await db.PartVendorLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && vendorIds.Contains(x.ExternalPartyId))
            .Select(x => new VendorLinkSummaryRow(
                x.Id,
                x.ExternalPartyId,
                x.PartId,
                x.CatalogLeadTimeDays))
            .ToListAsync(cancellationToken);

        var vendorLinkIds = vendorLinks.Select(x => x.PartVendorLinkId).ToList();
        var leadTimeSnapshots = await db.PartVendorLeadTimeSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && vendorLinkIds.Contains(x.PartVendorLinkId)
                && x.EffectiveFrom <= now
                && (x.EffectiveTo == null || x.EffectiveTo > now))
            .Select(x => new
            {
                x.PartVendorLinkId,
                x.LeadTimeDays,
                x.EffectiveFrom,
                x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        var leadTimeByLinkId = leadTimeSnapshots
            .GroupBy(x => x.PartVendorLinkId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.EffectiveFrom)
                    .ThenByDescending(x => x.UpdatedAt)
                    .First()
                    .LeadTimeDays);

        var vendorLinkByPart = vendorLinks
            .GroupBy(x => (x.ExternalPartyId, x.PartId))
            .ToDictionary(g => g.Key, g => g.First());

        var scorecardMetricsByVendor = BuildVendorScorecardMetrics(
            tenantId,
            vendorIds,
            vendorLinks,
            vendorLinkByPart,
            leadTimeByLinkId);

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
            scorecardMetricsByVendor.TryGetValue(vendor.Id, out var scorecardMetrics);

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
                scorecardMetrics?.AverageLeadTimeDays,
                scorecardMetrics?.LeadTimeSampleCount ?? 0,
                scorecardMetrics?.OnTimeDeliveryRate,
                scorecardMetrics?.OnTimeDeliverySampleCount ?? 0,
                lastPoAt,
                receiving?.LastPostedAt);
        }).ToList();

        var approvalCounts = vendors
            .GroupBy(x => x.ApprovalStatus)
            .Select(g => new VendorApprovalStatusSummaryResponse(g.Key, g.Count()))
            .OrderBy(x => x.ApprovalStatus)
            .ToList();

        return new VendorReportSummaryResponse(now, approvalCounts, items);
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
            "partyKey,displayName,approvalStatus,status,partVendorLinkCount,preferredPartLinkCount,openPurchaseRequestCount,openPurchaseOrderCount,issuedPurchaseOrderCount,postedReceivingReceiptCount,openBackorderCount,openPurchaseOrderLineQuantity,averageLeadTimeDays,leadTimeSampleCount,onTimeDeliveryRate,onTimeDeliverySampleCount,lastPurchaseOrderAt,lastReceivingPostedAt");

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
            builder.Append(vendor.AverageLeadTimeDays?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(vendor.LeadTimeSampleCount);
            builder.Append(',');
            builder.Append(vendor.OnTimeDeliveryRate?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(vendor.OnTimeDeliverySampleCount);
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
            0,
            null,
            0,
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

    private sealed record VendorLinkSummaryRow(
        Guid PartVendorLinkId,
        Guid ExternalPartyId,
        Guid PartId,
        int? CatalogLeadTimeDays);

    private sealed record VendorScorecardMetrics(
        int? AverageLeadTimeDays,
        int LeadTimeSampleCount,
        int? OnTimeDeliveryRate,
        int OnTimeDeliverySampleCount);

    private Dictionary<Guid, VendorScorecardMetrics> BuildVendorScorecardMetrics(
        Guid tenantId,
        IReadOnlyCollection<Guid> vendorIds,
        IReadOnlyCollection<VendorLinkSummaryRow> vendorLinks,
        IReadOnlyDictionary<(Guid ExternalPartyId, Guid PartId), VendorLinkSummaryRow> vendorLinkByPart,
        IReadOnlyDictionary<Guid, int> leadTimeByLinkId)
    {
        var vendorLeadTimeByVendor = vendorLinks
            .GroupBy(x => x.ExternalPartyId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var leadTimes = g
                        .Select(link => ResolveLeadTimeDays(link, leadTimeByLinkId))
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToList();

                    return new VendorScorecardMetrics(
                        leadTimes.Count > 0 ? (int?)Math.Round(leadTimes.Average()) : null,
                        leadTimes.Count,
                        null,
                        0);
                });

        var purchaseOrders = db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && vendorIds.Contains(x.VendorPartyId)
                && x.IssuedAt != null)
            .Select(x => new
            {
                x.Id,
                x.VendorPartyId,
                IssuedAt = x.IssuedAt!.Value,
            })
            .ToList();

        var purchaseOrderIds = purchaseOrders.Select(x => x.Id).ToList();
        var purchaseOrderLines = db.PurchaseOrderLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && purchaseOrderIds.Contains(x.PurchaseOrderId))
            .Select(x => new
            {
                x.PurchaseOrderId,
                x.PartId,
            })
            .ToList();

        var receiptRows = db.ReceivingReceipts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && purchaseOrderIds.Contains(x.PurchaseOrderId)
                && PostedReceivingStatuses.Contains(x.Status))
            .Select(x => new
            {
                x.PurchaseOrderId,
                PostedAt = x.PostedAt ?? x.CreatedAt,
            })
            .ToList();

        var lineByPurchaseOrderId = purchaseOrderLines
            .GroupBy(x => x.PurchaseOrderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var receiptByPurchaseOrderId = receiptRows
            .GroupBy(x => x.PurchaseOrderId)
            .ToDictionary(g => g.Key, g => g.Min(x => x.PostedAt));

        var metricsByVendor = vendorLeadTimeByVendor
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var vendorGroup in purchaseOrders.GroupBy(x => x.VendorPartyId))
        {
            vendorLeadTimeByVendor.TryGetValue(vendorGroup.Key, out var leadTimeMetrics);

            var onTimeSampleCount = 0;
            var onTimeCount = 0;
            foreach (var purchaseOrder in vendorGroup)
            {
                if (!lineByPurchaseOrderId.TryGetValue(purchaseOrder.Id, out var lines))
                {
                    continue;
                }

                var lineLeadTimes = lines
                    .Select(line =>
                        ResolveLeadTimeDays(
                            vendorLinkByPart.TryGetValue((purchaseOrder.VendorPartyId, line.PartId), out var link)
                                ? link
                                : null,
                            leadTimeByLinkId))
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();

                var expectedLeadTimeDays = lineLeadTimes.Count > 0
                    ? lineLeadTimes.Max()
                    : leadTimeMetrics?.AverageLeadTimeDays;

                if (expectedLeadTimeDays is null
                    || !receiptByPurchaseOrderId.TryGetValue(purchaseOrder.Id, out var postedAt))
                {
                    continue;
                }

                var actualDays = Math.Max(0d, (postedAt - purchaseOrder.IssuedAt).TotalDays);
                onTimeSampleCount++;
                if (actualDays <= expectedLeadTimeDays.Value)
                {
                    onTimeCount++;
                }
            }

            metricsByVendor[vendorGroup.Key] = new VendorScorecardMetrics(
                leadTimeMetrics?.AverageLeadTimeDays,
                leadTimeMetrics?.LeadTimeSampleCount ?? 0,
                onTimeSampleCount > 0
                    ? (int?)Math.Round((double)onTimeCount / onTimeSampleCount * 100)
                    : null,
                onTimeSampleCount);
        }

        return metricsByVendor;
    }

    private static int? ResolveLeadTimeDays(
        VendorLinkSummaryRow? vendorLink,
        IReadOnlyDictionary<Guid, int> leadTimeByLinkId)
    {
        if (vendorLink is null)
        {
            return null;
        }

        if (leadTimeByLinkId.TryGetValue(vendorLink.PartVendorLinkId, out var leadTimeDays))
        {
            return leadTimeDays;
        }

        return vendorLink.CatalogLeadTimeDays is > 0 ? vendorLink.CatalogLeadTimeDays : null;
    }
}
