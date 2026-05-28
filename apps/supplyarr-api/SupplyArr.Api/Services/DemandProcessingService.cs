using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class DemandProcessingService(
    SupplyArrDbContext db,
    DemandProcessingWorkerService workerService,
    ISupplyArrAuditService audit)
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

        var processedItems = states
            .Select(state => MapSummaryFromState(
                state,
                demandRefStatuses.GetValueOrDefault(state.DemandRefId, "unknown")))
            .ToList();

        var pendingItems = pendingResponse.Items
            .Select(item => MapSummaryFromPending(item, demandRefStatuses.GetValueOrDefault(item.DemandRefId, "received")))
            .ToList();

        return new DemandProcessingDashboardResponse(
            pendingResponse.Items.Count,
            processedItems.Count(x => x.ProcessingOutcome == DemandProcessingOutcomes.StockShort),
            processedItems.Count(x => x.ProcessingOutcome == DemandProcessingOutcomes.StockAvailable),
            processedItems.Count(x => x.ProcessingOutcome == DemandProcessingOutcomes.PrDrafted),
            processedItems,
            pendingItems);
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
                cancellationToken);

        var header = await LoadDemandRefHeaderAsync(tenantId, demandRefId, cancellationToken);

        var summary = state is null
            ? MapSummaryFromPending(
                new PendingDemandProcessingItem(
                    demandRefId,
                    header.Source,
                    header.SourceRefKey,
                    header.Title,
                    header.ReceivedAt,
                    null,
                    null),
                header.Status)
            : MapSummaryFromState(state, header.Status);

        if (header.PurchaseRequestId.HasValue && summary.PurchaseRequestId != header.PurchaseRequestId)
        {
            summary = summary with { PurchaseRequestId = header.PurchaseRequestId };
        }

        var lines = await LoadDemandRefLinesForDetailAsync(
            tenantId,
            header.Source,
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

        return new DemandProcessingDetailResponse(summary, lineSummaries);
    }

    public async Task<DemandProcessingOperatorActionResponse> RetryProcessingAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid demandRefId,
        CancellationToken cancellationToken = default)
    {
        var result = await workerService.ProcessDemandRefNowAsync(tenantId, demandRefId, cancellationToken);

        await audit.WriteAsync(
            "supplyarr.demand_processing.retry",
            tenantId,
            actorUserId,
            "demand_ref",
            demandRefId.ToString(),
            result.ProcessingOutcome,
            cancellationToken: cancellationToken);

        var detail = await GetDetailAsync(tenantId, demandRefId, cancellationToken);
        return new DemandProcessingOperatorActionResponse("retry_processing", result, detail);
    }

    public async Task<DemandProcessingOperatorActionResponse> CreatePurchaseRequestDraftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid demandRefId,
        CancellationToken cancellationToken = default)
    {
        var created = await workerService.CreatePurchaseRequestDraftForOperatorAsync(
            tenantId,
            actorUserId,
            demandRefId,
            cancellationToken);

        await audit.WriteAsync(
            "supplyarr.demand_processing.create_pr_draft",
            tenantId,
            actorUserId,
            "demand_ref",
            demandRefId.ToString(),
            created.PurchaseRequestId.ToString(),
            cancellationToken: cancellationToken);

        var result = new DemandProcessingResult(
            demandRefId,
            string.Empty,
            string.Empty,
            DemandProcessingOutcomes.PrDrafted,
            DemandProcessingRecommendedActions.PrAutoCreated,
            0,
            created.PurchaseRequestId,
            null);

        var detail = await GetDetailAsync(tenantId, demandRefId, cancellationToken);
        return new DemandProcessingOperatorActionResponse("create_pr_draft", result, detail);
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

    private async Task<(
        string Source,
        string Status,
        string SourceRefKey,
        string Title,
        DateTimeOffset ReceivedAt,
        Guid? PurchaseRequestId)>
        LoadDemandRefHeaderAsync(
            Guid tenantId,
            Guid demandRefId,
            CancellationToken cancellationToken)
    {
        var maintainarr = await db.MaintainArrDemandRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == demandRefId)
            .Select(x => new { x.Status, x.MaintainarrWorkOrderNumber, x.Title, x.ReceivedAt, x.PurchaseRequestId })
            .FirstOrDefaultAsync(cancellationToken);
        if (maintainarr is not null)
        {
            return (
                DemandRefSources.MaintainArr,
                maintainarr.Status,
                maintainarr.MaintainarrWorkOrderNumber,
                maintainarr.Title,
                maintainarr.ReceivedAt,
                maintainarr.PurchaseRequestId);
        }

        var routarr = await db.RoutArrDemandRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == demandRefId)
            .Select(x => new { x.Status, x.RoutarrTripNumber, x.Title, x.ReceivedAt, x.PurchaseRequestId })
            .FirstOrDefaultAsync(cancellationToken);
        if (routarr is not null)
        {
            return (
                DemandRefSources.RoutArr,
                routarr.Status,
                routarr.RoutarrTripNumber,
                routarr.Title,
                routarr.ReceivedAt,
                routarr.PurchaseRequestId);
        }

        var trainarr = await db.TrainArrDemandRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == demandRefId)
            .Select(x => new { x.Status, x.TrainarrAssignmentRefKey, x.Title, x.ReceivedAt, x.PurchaseRequestId })
            .FirstOrDefaultAsync(cancellationToken);
        if (trainarr is not null)
        {
            return (
                DemandRefSources.TrainArr,
                trainarr.Status,
                trainarr.TrainarrAssignmentRefKey,
                trainarr.Title,
                trainarr.ReceivedAt,
                trainarr.PurchaseRequestId);
        }

        var staffarr = await db.StaffArrDemandRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == demandRefId)
            .Select(x => new { x.Status, x.StaffarrIncidentTitle, x.Title, x.ReceivedAt, x.PurchaseRequestId })
            .FirstOrDefaultAsync(cancellationToken);
        if (staffarr is not null)
        {
            return (
                DemandRefSources.StaffArr,
                staffarr.Status,
                staffarr.StaffarrIncidentTitle,
                staffarr.Title,
                staffarr.ReceivedAt,
                staffarr.PurchaseRequestId);
        }

        throw new StlApiException(
            "demand_processing.demand_ref_not_found",
            "Demand reference was not found.",
            404);
    }

    private async Task<IReadOnlyList<DemandRefLineDetail>> LoadDemandRefLinesForDetailAsync(
        Guid tenantId,
        string demandRefSource,
        Guid demandRefId,
        CancellationToken cancellationToken)
    {
        var (_, lines) = await LoadDemandRefDetailAsync(tenantId, demandRefSource, demandRefId, cancellationToken);
        return lines;
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
                ?? throw new StlApiException(
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
                ?? throw new StlApiException(
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
                ?? throw new StlApiException(
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
                ?? throw new StlApiException(
                    "demand_processing.demand_ref_not_found",
                    "Demand reference was not found.",
                    404);

            return (demandRef.Status, demandRef.Lines.Select(x => new DemandRefLineDetail(
                x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested)).ToList());
        }

        throw new StlApiException(
            "demand_processing.demand_ref_not_found",
            "Demand reference was not found.",
            404);
    }

    private static DemandProcessingSummaryResponse MapSummaryFromState(
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
            state.LastProcessedAt,
            BuildSourceLink(state.DemandRefSource, state.MaintainarrWorkOrderNumber, state.Title));

    private static DemandProcessingSummaryResponse MapSummaryFromPending(
        PendingDemandProcessingItem item,
        string demandRefStatus) =>
        new(
            null,
            item.DemandRefId,
            item.DemandRefSource,
            item.SourceRefKey,
            item.Title,
            demandRefStatus,
            item.LastProcessingOutcome,
            null,
            null,
            null,
            null,
            null,
            null,
            item.ReceivedAt,
            item.LastProcessedAt,
            BuildSourceLink(item.DemandRefSource, item.SourceRefKey, item.Title));

    private static DemandProcessingSourceLinkResponse BuildSourceLink(
        string demandRefSource,
        string sourceRefKey,
        string title) =>
        demandRefSource switch
        {
            DemandRefSources.MaintainArr => new(
                "maintainarr",
                $"MaintainArr work order {sourceRefKey}",
                sourceRefKey),
            DemandRefSources.RoutArr => new(
                "routarr",
                $"RoutArr trip {sourceRefKey}",
                sourceRefKey),
            DemandRefSources.TrainArr => new(
                "trainarr",
                $"TrainArr assignment {sourceRefKey}",
                sourceRefKey),
            DemandRefSources.StaffArr => new(
                "staffarr",
                $"StaffArr incident {sourceRefKey}",
                sourceRefKey),
            _ => new(demandRefSource, title, sourceRefKey),
        };

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
