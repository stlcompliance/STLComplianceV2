using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public class SupplierReportService(SupplyArrDbContext db)
{
    private const int DetailListLimit = 25;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
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

    public Task<SupplierReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? approvalStatus,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        return GetSupplierSummaryAsync(tenantId, approvalStatus, activeOnly, cancellationToken);
    }

    public async Task<SupplierReportSummaryResponse> GetSupplierSummaryAsync(
        Guid tenantId,
        string? approvalStatus,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var suppliers = await db.Suppliers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Include(x => x.ParentSupplier)
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(approvalStatus))
        {
            var normalized = approvalStatus.Trim().ToLowerInvariant();
            suppliers = suppliers
                .Where(x => string.Equals(x.ApprovalStatus, normalized, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (activeOnly == true)
        {
            suppliers = suppliers
                .Where(x => string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var supplierIds = suppliers.Select(x => x.Id).ToList();
        if (supplierIds.Count == 0)
        {
            return new SupplierReportSummaryResponse(now, [], []);
        }

        var supplierLinks = await db.PartSupplierLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && supplierIds.Contains(x.SupplierId))
            .Select(x => new SupplierLinkSummaryRow(
                x.Id,
                x.SupplierId,
                x.PartId,
                x.CatalogLeadTimeDays))
            .ToListAsync(cancellationToken);

        var supplierLinkIds = supplierLinks.Select(x => x.PartSupplierLinkId).ToList();
        var leadTimeSnapshots = await db.PartSupplierLeadTimeSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && supplierLinkIds.Contains(x.PartSupplierLinkId)
                && x.EffectiveFrom <= now
                && (x.EffectiveTo == null || x.EffectiveTo > now))
            .Select(x => new
            {
                x.PartSupplierLinkId,
                x.LeadTimeDays,
                x.EffectiveFrom,
                x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        var leadTimeByLinkId = leadTimeSnapshots
            .GroupBy(x => x.PartSupplierLinkId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.EffectiveFrom)
                    .ThenByDescending(x => x.UpdatedAt)
                    .First()
                    .LeadTimeDays);

        var supplierLinkByPart = supplierLinks
            .GroupBy(x => (x.SupplierId, x.PartId))
            .ToDictionary(g => g.Key, g => g.First());

        var scorecardMetricsBySupplier = BuildSupplierScorecardMetrics(
            tenantId,
            supplierIds,
            supplierLinks,
            supplierLinkByPart,
            leadTimeByLinkId);

        var linkStats = await db.PartSupplierLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && supplierIds.Contains(x.SupplierId))
            .GroupBy(x => x.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                Count = g.Count(),
                PreferredCount = g.Count(x => x.IsPreferred),
            })
            .ToListAsync(cancellationToken);

        var prStats = await db.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.SupplierId != null
                && supplierIds.Contains(x.SupplierId.Value)
                && (x.Status == PurchaseRequestStatuses.Draft || x.Status == PurchaseRequestStatuses.Submitted))
            .GroupBy(x => x.SupplierId!.Value)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var poStats = await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && supplierIds.Contains(x.SupplierId))
            .Select(x => new
            {
                SupplierId = x.SupplierId,
                x.Status,
                x.UpdatedAt,
                LineQuantity = x.Lines.Sum(line => line.QuantityOrdered),
            })
            .ToListAsync(cancellationToken);

        var poGrouped = poStats
            .GroupBy(x => x.SupplierId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var receivingStats = await (
            from receipt in db.ReceivingReceipts.AsNoTracking()
            join purchaseOrder in db.PurchaseOrders.AsNoTracking()
                on receipt.PurchaseOrderId equals purchaseOrder.Id
            where receipt.TenantId == tenantId
                && PostedReceivingStatuses.Contains(receipt.Status)
                && supplierIds.Contains(purchaseOrder.SupplierId)
            select new
            {
                SupplierId = purchaseOrder.SupplierId,
                receipt.PostedAt,
            }).ToListAsync(cancellationToken);

        var receivingGrouped = receivingStats
            .GroupBy(x => x.SupplierId)
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
                && supplierIds.Contains(purchaseOrder.SupplierId)
            group backorder by purchaseOrder.SupplierId
            into grouped
            select new { SupplierId = grouped.Key, Count = grouped.Count() })
            .ToListAsync(cancellationToken);

        var linkBySupplier = linkStats.ToDictionary(x => x.SupplierId);
        var prBySupplier = prStats.ToDictionary(x => x.SupplierId);
        var backorderBySupplier = backorderStats.ToDictionary(x => x.SupplierId);

        var items = suppliers.Select(supplier =>
        {
            linkBySupplier.TryGetValue(supplier.Id, out var links);
            prBySupplier.TryGetValue(supplier.Id, out var openPr);
            backorderBySupplier.TryGetValue(supplier.Id, out var openBackorders);
            poGrouped.TryGetValue(supplier.Id, out var pos);
            receivingGrouped.TryGetValue(supplier.Id, out var receiving);
            scorecardMetricsBySupplier.TryGetValue(supplier.Id, out var scorecardMetrics);

            var openPoCount = pos?.Count(x => PurchaseOrderStatuses.Open.Contains(x.Status)) ?? 0;
            var issuedPoCount = pos?.Count(x =>
                string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase)) ?? 0;
            var openLineQty = pos?
                .Where(x => PurchaseOrderStatuses.Open.Contains(x.Status)
                    || string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.LineQuantity) ?? 0m;
            var lastPoAt = pos?.Max(x => (DateTimeOffset?)x.UpdatedAt);

            return new SupplierReportSummaryItemResponse(
                supplier.Id,
                supplier.SupplierKey,
                supplier.DisplayName,
                supplier.ParentSupplierId,
                supplier.ParentSupplier?.DisplayName,
                supplier.UnitKind,
                ParseServiceTypes(supplier.ServiceTypesJson),
                supplier.ApprovalStatus,
                supplier.Status,
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

        var approvalCounts = suppliers
            .GroupBy(x => x.ApprovalStatus)
            .Select(g => new SupplierApprovalStatusSummaryResponse(g.Key, g.Count()))
            .OrderBy(x => x.ApprovalStatus)
            .ToList();

        return new SupplierReportSummaryResponse(now, approvalCounts, items);
    }

    public Task<SupplierReportDetailResponse> GetDetailAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return GetSupplierDetailAsync(tenantId, supplierId, cancellationToken);
    }

    public async Task<SupplierReportDetailResponse> GetSupplierDetailAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .Include(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == supplierId,
                cancellationToken)
            ?? throw new StlApiException("supplier.not_found", "Supplier was not found.", 404);

        var summaryResponse = await GetSupplierSummaryAsync(tenantId, null, null, cancellationToken);
        var summary = summaryResponse.Suppliers.FirstOrDefault(x => x.SupplierId == supplierId)
            ?? MapSingleSupplierSummary(supplier);

        var recentPurchaseRequests = await db.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SupplierId == supplierId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => new SupplierReportPurchaseRequestRowResponse(
                x.Id,
                x.RequestKey,
                x.Title,
                x.Status,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var recentPurchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SupplierId == supplierId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(DetailListLimit)
            .Select(x => new SupplierReportPurchaseOrderRowResponse(
                x.Id,
                x.OrderKey,
                x.Title,
                x.Status,
                x.Lines.Count,
                x.Lines.Sum(line => line.QuantityOrdered),
                x.Lines.Sum(line => line.QuantityReceived),
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var partLinks = await db.PartSupplierLinks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SupplierId == supplierId)
            .OrderByDescending(x => x.IsPreferred)
            .ThenBy(x => x.SupplierPartNumber)
            .Take(DetailListLimit)
            .Select(x => new SupplierReportPartLinkRowResponse(
                x.Id,
                x.Supplier.Id,
                x.Supplier.SupplierKey,
                x.Supplier.DisplayName,
                x.Supplier.ParentSupplierId,
                x.Supplier.ParentSupplier != null ? x.Supplier.ParentSupplier.DisplayName : null,
                x.Supplier.UnitKind,
                ParseServiceTypes(x.Supplier.ServiceTypesJson),
                x.PartId,
                x.Part.PartKey,
                x.Part.DisplayName,
                x.SupplierPartNumber,
                x.IsPreferred,
                x.CatalogUnitPrice,
                x.CatalogAvailabilityStatus))
            .ToListAsync(cancellationToken);

        return new SupplierReportDetailResponse(
            summary,
            recentPurchaseRequests,
            recentPurchaseOrders,
            partLinks);
    }

    public Task<(string ContentType, string FileName, byte[] Content)> ExportSupplierSummaryCsvAsync(
        Guid tenantId,
        string? approvalStatus,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        return ExportSupplierSummaryCsvInternalAsync(tenantId, approvalStatus, activeOnly, cancellationToken);
    }

    public Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        string? approvalStatus,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        return ExportSupplierSummaryCsvInternalAsync(tenantId, approvalStatus, activeOnly, cancellationToken);
    }

    private async Task<(string ContentType, string FileName, byte[] Content)> ExportSupplierSummaryCsvInternalAsync(
        Guid tenantId,
        string? approvalStatus,
        bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var summary = await GetSupplierSummaryAsync(tenantId, approvalStatus, activeOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "supplierKey,supplierDisplayName,parentSupplierDisplayName,supplierUnitKind,supplierServiceTypes,approvalStatus,status,partSupplierLinkCount,preferredPartSupplierLinkCount,openPurchaseRequestCount,openPurchaseOrderCount,issuedPurchaseOrderCount,postedReceivingReceiptCount,openBackorderCount,openPurchaseOrderLineQuantity,averageLeadTimeDays,leadTimeSampleCount,onTimeDeliveryRate,onTimeDeliverySampleCount,lastPurchaseOrderAt,lastReceivingPostedAt");

        foreach (var supplier in summary.Suppliers)
        {
            builder.Append(CsvEscape(supplier.SupplierKey));
            builder.Append(',');
            builder.Append(CsvEscape(supplier.SupplierDisplayName));
            builder.Append(',');
            builder.Append(CsvEscape(supplier.ParentSupplierDisplayName ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(supplier.SupplierUnitKind));
            builder.Append(',');
            builder.Append(CsvEscape(string.Join("; ", supplier.SupplierServiceTypes)));
            builder.Append(',');
            builder.Append(CsvEscape(supplier.ApprovalStatus));
            builder.Append(',');
            builder.Append(CsvEscape(supplier.Status));
            builder.Append(',');
            builder.Append(supplier.PartSupplierLinkCount);
            builder.Append(',');
            builder.Append(supplier.PreferredPartSupplierLinkCount);
            builder.Append(',');
            builder.Append(supplier.OpenPurchaseRequestCount);
            builder.Append(',');
            builder.Append(supplier.OpenPurchaseOrderCount);
            builder.Append(',');
            builder.Append(supplier.IssuedPurchaseOrderCount);
            builder.Append(',');
            builder.Append(supplier.PostedReceivingReceiptCount);
            builder.Append(',');
            builder.Append(supplier.OpenBackorderCount);
            builder.Append(',');
            builder.Append(supplier.OpenPurchaseOrderLineQuantity);
            builder.Append(',');
            builder.Append(supplier.AverageLeadTimeDays?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(supplier.LeadTimeSampleCount);
            builder.Append(',');
            builder.Append(supplier.OnTimeDeliveryRate?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(supplier.OnTimeDeliverySampleCount);
            builder.Append(',');
            builder.Append(supplier.LastPurchaseOrderAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.AppendLine(supplier.LastReceivingPostedAt?.ToString("O") ?? string.Empty);
        }

        var fileName = $"supplyarr-supplier-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static SupplierReportSummaryItemResponse MapSingleSupplierSummary(Supplier supplier) =>
        new(
            supplier.Id,
            supplier.SupplierKey,
            supplier.DisplayName,
            supplier.ParentSupplierId,
            supplier.ParentSupplier?.DisplayName,
            supplier.UnitKind,
            ParseServiceTypes(supplier.ServiceTypesJson),
            supplier.ApprovalStatus,
            supplier.Status,
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

    private static IReadOnlyList<string> ParseServiceTypes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private sealed record SupplierLinkSummaryRow(
        Guid PartSupplierLinkId,
        Guid SupplierId,
        Guid PartId,
        int? CatalogLeadTimeDays);

    private sealed record SupplierScorecardMetrics(
        int? AverageLeadTimeDays,
        int LeadTimeSampleCount,
        int? OnTimeDeliveryRate,
        int OnTimeDeliverySampleCount);

    private Dictionary<Guid, SupplierScorecardMetrics> BuildSupplierScorecardMetrics(
        Guid tenantId,
        IReadOnlyCollection<Guid> supplierIds,
        IReadOnlyCollection<SupplierLinkSummaryRow> supplierLinks,
        IReadOnlyDictionary<(Guid SupplierId, Guid PartId), SupplierLinkSummaryRow> supplierLinkByPart,
        IReadOnlyDictionary<Guid, int> leadTimeByLinkId)
    {
        var leadTimeBySupplier = supplierLinks
            .GroupBy(x => x.SupplierId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var leadTimes = g
                        .Select(link => ResolveLeadTimeDays(link, leadTimeByLinkId))
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToList();

                    return new SupplierScorecardMetrics(
                        leadTimes.Count > 0 ? (int?)Math.Round(leadTimes.Average()) : null,
                        leadTimes.Count,
                        null,
                        0);
                });

        var purchaseOrders = db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && supplierIds.Contains(x.SupplierId)
                && x.IssuedAt != null)
            .Select(x => new
            {
                x.Id,
                SupplierId = x.SupplierId,
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

        var metricsBySupplier = leadTimeBySupplier
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var supplierGroup in purchaseOrders.GroupBy(x => x.SupplierId))
        {
            leadTimeBySupplier.TryGetValue(supplierGroup.Key, out var leadTimeMetrics);

            var onTimeSampleCount = 0;
            var onTimeCount = 0;
            foreach (var purchaseOrder in supplierGroup)
            {
                if (!lineByPurchaseOrderId.TryGetValue(purchaseOrder.Id, out var lines))
                {
                    continue;
                }

                var lineLeadTimes = lines
                    .Select(line =>
                        ResolveLeadTimeDays(
                            supplierLinkByPart.TryGetValue((purchaseOrder.SupplierId, line.PartId), out var link)
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

            metricsBySupplier[supplierGroup.Key] = new SupplierScorecardMetrics(
                leadTimeMetrics?.AverageLeadTimeDays,
                leadTimeMetrics?.LeadTimeSampleCount ?? 0,
                onTimeSampleCount > 0
                    ? (int?)Math.Round((double)onTimeCount / onTimeSampleCount * 100)
                    : null,
                onTimeSampleCount);
        }

        return metricsBySupplier;
    }

    private static int? ResolveLeadTimeDays(
        SupplierLinkSummaryRow? supplierLink,
        IReadOnlyDictionary<Guid, int> leadTimeByLinkId)
    {
        if (supplierLink is null)
        {
            return null;
        }

        if (leadTimeByLinkId.TryGetValue(supplierLink.PartSupplierLinkId, out var leadTimeDays))
        {
            return leadTimeDays;
        }

        return supplierLink.CatalogLeadTimeDays is > 0 ? supplierLink.CatalogLeadTimeDays : null;
    }
}


