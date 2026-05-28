using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class DemandProcessingService(
    SupplyArrDbContext db,
    DemandProcessingWorkerService workerService)
{
    public async Task<DemandProcessingDashboardResponse> GetDashboardAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var states = await db.DemandProcessingStates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.LastProcessedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var demandRefIds = states.Select(x => x.DemandRefId).ToList();
        var demandRefStatuses = demandRefIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.MaintainArrDemandRefs
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && demandRefIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Status, cancellationToken);

        var pendingResponse = await workerService.ListPendingAsync(
            tenantId,
            null,
            100,
            null,
            cancellationToken);

        var items = states
            .Select(state => MapSummary(state, demandRefStatuses.GetValueOrDefault(state.DemandRefId, "unknown")))
            .ToList();

        return new DemandProcessingDashboardResponse(
            pendingResponse.Items.Count,
            items.Count(x => x.ProcessingOutcome == DemandProcessingOutcomes.StockShort),
            items.Count(x => x.ProcessingOutcome == DemandProcessingOutcomes.StockAvailable),
            items.Count(x => x.ProcessingOutcome == DemandProcessingOutcomes.PrDrafted),
            items);
    }

    public async Task<DemandProcessingDetailResponse> GetDetailAsync(
        Guid tenantId,
        Guid demandRefId,
        CancellationToken cancellationToken = default)
    {
        var state = await db.DemandProcessingStates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.DemandRefId == demandRefId,
                cancellationToken)
            ?? throw new STLCompliance.Shared.Contracts.StlApiException(
                "demand_processing.not_found",
                "Demand processing state was not found.",
                404);

        var demandRef = await db.MaintainArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
            ?? throw new STLCompliance.Shared.Contracts.StlApiException(
                "demand_processing.demand_ref_not_found",
                "Demand reference was not found.",
                404);

        var stockTotals = await LoadStockTotalsAsync(tenantId, cancellationToken);
        var lines = demandRef.Lines
            .OrderBy(x => x.LineNumber)
            .Select(line =>
            {
                decimal quantityAvailable = 0;
                var isShort = false;
                if (line.PartId is Guid partId)
                {
                    stockTotals.TryGetValue(partId, out var stock);
                    quantityAvailable = stock.OnHand - stock.Reserved;
                    isShort = quantityAvailable < line.QuantityRequested;
                }

                return new DemandProcessingLineSummary(
                    line.Id,
                    line.LineNumber,
                    line.PartId,
                    line.PartNumber,
                    line.QuantityRequested,
                    quantityAvailable,
                    isShort);
            })
            .ToList();

        return new DemandProcessingDetailResponse(
            MapSummary(state, demandRef.Status),
            lines);
    }

    private static DemandProcessingSummaryResponse MapSummary(
        DemandProcessingState state,
        string demandRefStatus) =>
        new(
            state.Id,
            state.DemandRefId,
            state.MaintainarrWorkOrderNumber,
            state.Title,
            demandRefStatus,
            state.ProcessingOutcome,
            state.RecommendedAction,
            state.LinesTotalCount,
            state.LinesCatalogCount,
            state.LinesShortCount,
            state.PurchaseRequestId,
            state.LastProcessingMessage,
            state.DemandReceivedAt,
            state.LastProcessedAt);

    private async Task<Dictionary<Guid, (decimal OnHand, decimal Reserved)>> LoadStockTotalsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rows = await db.PartStockLevels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.PartId)
            .Select(g => new
            {
                PartId = g.Key,
                OnHand = g.Sum(x => x.QuantityOnHand),
                Reserved = g.Sum(x => x.QuantityReserved),
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.PartId, x => (x.OnHand, x.Reserved));
    }
}
