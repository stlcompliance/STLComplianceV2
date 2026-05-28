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

        var demandRefStatuses = await LoadDemandRefStatusesAsync(tenantId, states, cancellationToken);

        var pendingResponse = await workerService.ListPendingAsync(
            tenantId,
            null,
            100,
            null,
            cancellationToken);

        var items = states
            .Select(state => MapSummary(
                state,
                demandRefStatuses.GetValueOrDefault(state.DemandRefId, "unknown")))
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

        var (demandRefStatus, lines) = await LoadDemandRefDetailAsync(
            tenantId,
            state.DemandRefSource,
            demandRefId,
            cancellationToken);

        var stockTotals = await LoadStockTotalsAsync(tenantId, cancellationToken);
        var lineSummaries = lines
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
                    line.LineId,
                    line.LineNumber,
                    line.PartId,
                    line.PartNumber,
                    line.QuantityRequested,
                    quantityAvailable,
                    isShort);
            })
            .ToList();

        return new DemandProcessingDetailResponse(
            MapSummary(state, demandRefStatus),
            lineSummaries);
    }

    private async Task<Dictionary<Guid, string>> LoadDemandRefStatusesAsync(
        Guid tenantId,
        IReadOnlyList<DemandProcessingState> states,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, string>();
        if (states.Count == 0)
        {
            return result;
        }

        var maintainarrIds = states
            .Where(x => string.Equals(x.DemandRefSource, DemandRefSources.MaintainArr, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.DemandRefId)
            .ToList();
        if (maintainarrIds.Count > 0)
        {
            var rows = await db.MaintainArrDemandRefs
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && maintainarrIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Status })
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                result[row.Id] = row.Status;
            }
        }

        var routarrIds = states
            .Where(x => string.Equals(x.DemandRefSource, DemandRefSources.RoutArr, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.DemandRefId)
            .ToList();
        if (routarrIds.Count > 0)
        {
            var rows = await db.RoutArrDemandRefs
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && routarrIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Status })
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                result[row.Id] = row.Status;
            }
        }

        var trainarrIds = states
            .Where(x => string.Equals(x.DemandRefSource, DemandRefSources.TrainArr, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.DemandRefId)
            .ToList();
        if (trainarrIds.Count > 0)
        {
            var rows = await db.TrainArrDemandRefs
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && trainarrIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Status })
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                result[row.Id] = row.Status;
            }
        }

        var staffarrIds = states
            .Where(x => string.Equals(x.DemandRefSource, DemandRefSources.StaffArr, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.DemandRefId)
            .ToList();
        if (staffarrIds.Count > 0)
        {
            var rows = await db.StaffArrDemandRefs
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && staffarrIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Status })
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                result[row.Id] = row.Status;
            }
        }

        return result;
    }

    private async Task<(string Status, IReadOnlyList<DemandRefLineDetail>)> LoadDemandRefDetailAsync(
        Guid tenantId,
        string demandRefSource,
        Guid demandRefId,
        CancellationToken cancellationToken)
    {
        if (string.Equals(demandRefSource, DemandRefSources.MaintainArr, StringComparison.OrdinalIgnoreCase))
        {
            var demandRef = await db.MaintainArrDemandRefs
                .AsNoTracking()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
                ?? throw new STLCompliance.Shared.Contracts.StlApiException(
                    "demand_processing.demand_ref_not_found",
                    "Demand reference was not found.",
                    404);

            return (demandRef.Status, demandRef.Lines.Select(x => new DemandRefLineDetail(
                x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested)).ToList());
        }

        if (string.Equals(demandRefSource, DemandRefSources.RoutArr, StringComparison.OrdinalIgnoreCase))
        {
            var demandRef = await db.RoutArrDemandRefs
                .AsNoTracking()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
                ?? throw new STLCompliance.Shared.Contracts.StlApiException(
                    "demand_processing.demand_ref_not_found",
                    "Demand reference was not found.",
                    404);

            return (demandRef.Status, demandRef.Lines.Select(x => new DemandRefLineDetail(
                x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested)).ToList());
        }

        if (string.Equals(demandRefSource, DemandRefSources.TrainArr, StringComparison.OrdinalIgnoreCase))
        {
            var demandRef = await db.TrainArrDemandRefs
                .AsNoTracking()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
                ?? throw new STLCompliance.Shared.Contracts.StlApiException(
                    "demand_processing.demand_ref_not_found",
                    "Demand reference was not found.",
                    404);

            return (demandRef.Status, demandRef.Lines.Select(x => new DemandRefLineDetail(
                x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested)).ToList());
        }

        if (string.Equals(demandRefSource, DemandRefSources.StaffArr, StringComparison.OrdinalIgnoreCase))
        {
            var demandRef = await db.StaffArrDemandRefs
                .AsNoTracking()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
                ?? throw new STLCompliance.Shared.Contracts.StlApiException(
                    "demand_processing.demand_ref_not_found",
                    "Demand reference was not found.",
                    404);

            return (demandRef.Status, demandRef.Lines.Select(x => new DemandRefLineDetail(
                x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested)).ToList());
        }

        throw new STLCompliance.Shared.Contracts.StlApiException(
            "demand_processing.demand_ref_not_found",
            "Demand reference was not found.",
            404);
    }

    private static DemandProcessingSummaryResponse MapSummary(
        DemandProcessingState state,
        string demandRefStatus) =>
        new(
            state.Id,
            state.DemandRefId,
            state.DemandRefSource,
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

    private sealed record DemandRefLineDetail(
        Guid LineId,
        int LineNumber,
        Guid? PartId,
        string PartNumber,
        decimal QuantityRequested);
}
