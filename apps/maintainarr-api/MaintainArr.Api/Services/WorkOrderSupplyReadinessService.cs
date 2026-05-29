using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class WorkOrderSupplyReadinessService(
    MaintainArrDbContext db,
    SupplyArrSupplyReadinessClient supplyReadinessClient)
{
    public async Task<WorkOrderSupplyReadinessResponse> GetAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await db.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken)
            ?? throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);

        var demandLines = await db.WorkOrderPartsDemandLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .OrderBy(x => x.LineNumber)
            .ToListAsync(cancellationToken);

        var generatedAt = DateTimeOffset.UtcNow;
        var lineResponses = new List<WorkOrderLineSupplyReadinessResponse>();
        var linesChecked = 0;
        var linesReady = 0;
        var linesBlocked = 0;
        var linesSkipped = 0;

        foreach (var line in demandLines)
        {
            if (line.SupplyarrPartId is not { } partId)
            {
                linesSkipped += 1;
                lineResponses.Add(new WorkOrderLineSupplyReadinessResponse(
                    line.Id,
                    line.LineNumber,
                    null,
                    line.PartNumber,
                    line.QuantityRequested,
                    line.Status,
                    null,
                    null,
                    "missing_supplyarr_part_id",
                    null,
                    null,
                    []));
                continue;
            }

            linesChecked += 1;
            var readiness = await supplyReadinessClient.GetPartReadinessAsync(
                tenantId,
                partId,
                line.QuantityRequested,
                cancellationToken);

            var blockers = readiness.Blockers
                .Select(blocker => new WorkOrderSupplyReadinessBlockerResponse(
                    blocker.ReasonCode,
                    blocker.Message,
                    blocker.SourceEntityType,
                    blocker.SourceEntityId,
                    blocker.RelatedEntityId))
                .ToList();

            if (string.Equals(readiness.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase))
            {
                linesReady += 1;
            }
            else
            {
                linesBlocked += 1;
            }

            lineResponses.Add(new WorkOrderLineSupplyReadinessResponse(
                line.Id,
                line.LineNumber,
                partId,
                line.PartNumber,
                line.QuantityRequested,
                line.Status,
                readiness.ReadinessStatus,
                readiness.ReadinessBasis,
                null,
                readiness.Availability.QuantityAvailable,
                readiness.CalculatedAt,
                blockers));
        }

        var overallStatus = ResolveOverallStatus(
            demandLines.Count,
            linesChecked,
            linesReady,
            linesBlocked,
            linesSkipped);

        return new WorkOrderSupplyReadinessResponse(
            workOrder.Id,
            workOrder.WorkOrderNumber,
            generatedAt,
            overallStatus,
            demandLines.Count,
            linesChecked,
            linesReady,
            linesBlocked,
            linesSkipped,
            lineResponses);
    }

    public static string ResolveOverallStatus(
        int totalDemandLines,
        int linesChecked,
        int linesReady,
        int linesBlocked,
        int linesSkipped)
    {
        if (totalDemandLines == 0)
        {
            return "no_demand";
        }

        if (linesChecked == 0)
        {
            return "unknown";
        }

        if (linesBlocked > 0)
        {
            return "not_ready";
        }

        return linesReady == linesChecked ? "ready" : "unknown";
    }
}
