using System.Text;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PartsInventoryReportService(SupplyArrDbContext db)
{
    private const int DetailListLimit = 50;

    public async Task<PartsInventoryReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? partStatus,
        bool? activePartsOnly,
        bool? belowReorderOnly,
        Guid? inventoryLocationId,
        CancellationToken cancellationToken = default)
    {
        var parts = await db.Parts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.PartKey)
            .ToListAsync(cancellationToken);

        if (activePartsOnly == true)
        {
            parts = parts
                .Where(x => string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        else if (!string.IsNullOrWhiteSpace(partStatus))
        {
            var normalized = partStatus.Trim().ToLowerInvariant();
            parts = parts
                .Where(x => string.Equals(x.Status, normalized, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var partIds = parts.Select(x => x.Id).ToList();

        var stockRows = await db.PartStockLevels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && partIds.Contains(x.PartId))
            .Select(x => new StockRow(
                x.PartId,
                x.InventoryBinId,
                x.QuantityOnHand,
                x.QuantityReserved))
            .ToListAsync(cancellationToken);

        if (inventoryLocationId is not null)
        {
            var binIdsAtLocation = await db.InventoryBins
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.InventoryLocationId == inventoryLocationId.Value)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            stockRows = stockRows.Where(x => binIdsAtLocation.Contains(x.InventoryBinId)).ToList();
            parts = parts
                .Where(p => stockRows.Any(s => s.PartId == p.Id))
                .ToList();
            partIds = parts.Select(x => x.Id).ToList();
        }

        var stockByPart = stockRows
            .GroupBy(x => x.PartId)
            .ToDictionary(
                g => g.Key,
                g => new PartStockTotals(
                    g.Sum(x => x.QuantityOnHand),
                    g.Sum(x => x.QuantityReserved)));

        var linkCounts = partIds.Count == 0
            ? []
            : await db.PartVendorLinks
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && partIds.Contains(x.PartId))
                .GroupBy(x => x.PartId)
                .Select(g => new { PartId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

        var linkByPart = linkCounts.ToDictionary(x => x.PartId, x => x.Count);

        var partSummaries = parts
            .Select(part =>
            {
                stockByPart.TryGetValue(part.Id, out var stock);
                var onHand = stock?.OnHand ?? 0m;
                var reserved = stock?.Reserved ?? 0m;
                var available = onHand - reserved;
                var belowReorder = part.ReorderPoint is not null
                    && available < part.ReorderPoint.Value;

                return new PartsInventoryPartSummaryItemResponse(
                    part.Id,
                    part.PartKey,
                    part.DisplayName,
                    part.Status,
                    part.CategoryKey,
                    part.ReorderPoint,
                    part.ReorderQuantity,
                    onHand,
                    reserved,
                    available,
                    belowReorder,
                    linkByPart.GetValueOrDefault(part.Id));
            })
            .ToList();

        if (belowReorderOnly == true)
        {
            partSummaries = partSummaries.Where(x => x.BelowReorderPoint).ToList();
        }

        var locations = await BuildLocationSummariesAsync(tenantId, stockRows, cancellationToken);

        var allPartsForTotals = await db.Parts.AsNoTracking().CountAsync(x => x.TenantId == tenantId, cancellationToken);
        var activePartsForTotals = await db.Parts.AsNoTracking().CountAsync(
            x => x.TenantId == tenantId && x.Status == "active",
            cancellationToken);
        var locationCount = await db.InventoryLocations.AsNoTracking().CountAsync(x => x.TenantId == tenantId, cancellationToken);
        var binCount = await db.InventoryBins.AsNoTracking().CountAsync(x => x.TenantId == tenantId, cancellationToken);

        var totals = new PartsInventoryReportTotalsResponse(
            allPartsForTotals,
            activePartsForTotals,
            locationCount,
            binCount,
            partSummaries.Count(x => x.BelowReorderPoint),
            partSummaries.Count(x => x.QuantityOnHand <= 0m),
            partSummaries.Sum(x => x.QuantityOnHand),
            partSummaries.Sum(x => x.QuantityReserved),
            partSummaries.Sum(x => x.QuantityAvailable));

        return new PartsInventoryReportSummaryResponse(
            DateTimeOffset.UtcNow,
            totals,
            locations,
            partSummaries);
    }

    public async Task<PartsInventoryPartDetailResponse> GetPartDetailAsync(
        Guid tenantId,
        Guid partId,
        CancellationToken cancellationToken = default)
    {
        var part = await db.Parts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partId, cancellationToken)
            ?? throw new StlApiException("part.not_found", "Part was not found.", 404);

        var summaryResponse = await GetSummaryAsync(tenantId, null, null, null, null, cancellationToken);
        var summary = summaryResponse.Parts.FirstOrDefault(x => x.PartId == partId)
            ?? BuildPartSummary(part, 0m, 0m, 0);

        var stockByBin = await (
            from stock in db.PartStockLevels.AsNoTracking()
            join bin in db.InventoryBins.AsNoTracking() on stock.InventoryBinId equals bin.Id
            join location in db.InventoryLocations.AsNoTracking() on bin.InventoryLocationId equals location.Id
            where stock.TenantId == tenantId && stock.PartId == partId
            orderby location.LocationKey, bin.BinKey
            select new PartsInventoryStockBinRowResponse(
                stock.Id,
                bin.Id,
                bin.BinKey,
                bin.Name,
                location.Id,
                location.LocationKey,
                location.Name,
                stock.QuantityOnHand,
                stock.QuantityReserved,
                stock.QuantityOnHand - stock.QuantityReserved))
            .Take(DetailListLimit)
            .ToListAsync(cancellationToken);

        var vendorLinks = await (
            from link in db.PartVendorLinks.AsNoTracking()
            join party in db.ExternalParties.AsNoTracking() on link.ExternalPartyId equals party.Id
            where link.TenantId == tenantId && link.PartId == partId
            orderby link.IsPreferred descending, party.DisplayName
            select new PartsInventoryPartVendorLinkRowResponse(
                link.Id,
                party.Id,
                party.PartyKey,
                party.DisplayName,
                link.VendorPartNumber,
                link.IsPreferred))
            .Take(DetailListLimit)
            .ToListAsync(cancellationToken);

        return new PartsInventoryPartDetailResponse(summary, stockByBin, vendorLinks);
    }

    public async Task<PartsInventoryLocationDetailResponse> GetLocationDetailAsync(
        Guid tenantId,
        Guid inventoryLocationId,
        CancellationToken cancellationToken = default)
    {
        var location = await db.InventoryLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == inventoryLocationId, cancellationToken)
            ?? throw new StlApiException("inventory_location.not_found", "Inventory location was not found.", 404);

        var stockRows = await (
            from stock in db.PartStockLevels.AsNoTracking()
            join bin in db.InventoryBins.AsNoTracking() on stock.InventoryBinId equals bin.Id
            where stock.TenantId == tenantId && bin.InventoryLocationId == inventoryLocationId
            select new StockRow(stock.PartId, stock.InventoryBinId, stock.QuantityOnHand, stock.QuantityReserved))
            .ToListAsync(cancellationToken);

        var summaryResponse = await GetSummaryAsync(tenantId, null, null, null, inventoryLocationId, cancellationToken);
        var summary = summaryResponse.Locations.FirstOrDefault(x => x.InventoryLocationId == inventoryLocationId)
            ?? new PartsInventoryLocationSummaryItemResponse(
                location.Id,
                location.LocationKey,
                location.Name,
                location.Status,
                0,
                0,
                0m,
                0m,
                0m);

        var bins = await db.InventoryBins
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.InventoryLocationId == inventoryLocationId)
            .OrderBy(x => x.BinKey)
            .ToListAsync(cancellationToken);

        var binRows = bins
            .Select(bin =>
            {
                var binStock = stockRows.Where(x => x.InventoryBinId == bin.Id).ToList();
                return new PartsInventoryLocationBinRowResponse(
                    bin.Id,
                    bin.BinKey,
                    bin.Name,
                    bin.Status,
                    binStock.Select(x => x.PartId).Distinct().Count(),
                    binStock.Sum(x => x.QuantityOnHand),
                    binStock.Sum(x => x.QuantityReserved));
            })
            .ToList();

        var partRows = stockRows
            .GroupBy(x => x.PartId)
            .Select(g => new
            {
                PartId = g.Key,
                OnHand = g.Sum(x => x.QuantityOnHand),
                Reserved = g.Sum(x => x.QuantityReserved),
            })
            .ToList();

        var partIds = partRows.Select(x => x.PartId).ToList();
        var partEntities = await db.Parts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && partIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var parts = partRows
            .Join(
                partEntities,
                row => row.PartId,
                part => part.Id,
                (row, part) => new PartsInventoryLocationPartRowResponse(
                    part.Id,
                    part.PartKey,
                    part.DisplayName,
                    row.OnHand,
                    row.Reserved,
                    row.OnHand - row.Reserved))
            .OrderBy(x => x.PartKey)
            .Take(DetailListLimit)
            .ToList();

        return new PartsInventoryLocationDetailResponse(summary, binRows, parts);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportPartsSummaryCsvAsync(
        Guid tenantId,
        string? partStatus,
        bool? activePartsOnly,
        bool? belowReorderOnly,
        Guid? inventoryLocationId,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(
            tenantId,
            partStatus,
            activePartsOnly,
            belowReorderOnly,
            inventoryLocationId,
            cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(
            "partKey,displayName,status,categoryKey,reorderPoint,reorderQuantity,quantityOnHand,quantityReserved,quantityAvailable,belowReorderPoint,vendorLinkCount");

        foreach (var part in summary.Parts)
        {
            builder.Append(CsvEscape(part.PartKey));
            builder.Append(',');
            builder.Append(CsvEscape(part.DisplayName));
            builder.Append(',');
            builder.Append(CsvEscape(part.Status));
            builder.Append(',');
            builder.Append(CsvEscape(part.CategoryKey));
            builder.Append(',');
            builder.Append(part.ReorderPoint?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(part.ReorderQuantity?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(part.QuantityOnHand);
            builder.Append(',');
            builder.Append(part.QuantityReserved);
            builder.Append(',');
            builder.Append(part.QuantityAvailable);
            builder.Append(',');
            builder.Append(part.BelowReorderPoint ? "true" : "false");
            builder.AppendLine(part.VendorLinkCount.ToString());
        }

        var fileName = $"supplyarr-parts-inventory-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private async Task<IReadOnlyList<PartsInventoryLocationSummaryItemResponse>> BuildLocationSummariesAsync(
        Guid tenantId,
        IReadOnlyList<StockRow> stockRows,
        CancellationToken cancellationToken)
    {
        var locations = await db.InventoryLocations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.LocationKey)
            .ToListAsync(cancellationToken);

        if (locations.Count == 0)
        {
            return [];
        }

        var bins = await db.InventoryBins
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var binsByLocation = bins.GroupBy(x => x.InventoryLocationId).ToDictionary(g => g.Key, g => g.Count());

        return locations
            .Select(location =>
            {
                var locationBinIds = bins
                    .Where(x => x.InventoryLocationId == location.Id)
                    .Select(x => x.Id)
                    .ToHashSet();

                var locationStock = stockRows.Where(x => locationBinIds.Contains(x.InventoryBinId)).ToList();
                var onHand = locationStock.Sum(x => x.QuantityOnHand);
                var reserved = locationStock.Sum(x => x.QuantityReserved);

                return new PartsInventoryLocationSummaryItemResponse(
                    location.Id,
                    location.LocationKey,
                    location.Name,
                    location.Status,
                    binsByLocation.GetValueOrDefault(location.Id),
                    locationStock.Select(x => x.PartId).Distinct().Count(),
                    onHand,
                    reserved,
                    onHand - reserved);
            })
            .ToList();
    }

    private static PartsInventoryPartSummaryItemResponse BuildPartSummary(
        Part part,
        decimal onHand,
        decimal reserved,
        int vendorLinkCount)
    {
        var available = onHand - reserved;
        var belowReorder = part.ReorderPoint is not null && available < part.ReorderPoint.Value;
        return new PartsInventoryPartSummaryItemResponse(
            part.Id,
            part.PartKey,
            part.DisplayName,
            part.Status,
            part.CategoryKey,
            part.ReorderPoint,
            part.ReorderQuantity,
            onHand,
            reserved,
            available,
            belowReorder,
            vendorLinkCount);
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private sealed record StockRow(Guid PartId, Guid InventoryBinId, decimal QuantityOnHand, decimal QuantityReserved);

    private sealed record PartStockTotals(decimal OnHand, decimal Reserved);
}
