using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class TripPartsDemandStatusIngestionService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public async Task<IngestSupplyarrDemandStatusResponse> IngestAsync(
        IngestSupplyarrDemandStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var existing = await db.TripPartsDemandStatusEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId
                    && x.SupplyarrCallbackPublicationId == request.SupplyarrCallbackPublicationId,
                cancellationToken);

        if (existing is not null)
        {
            return new IngestSupplyarrDemandStatusResponse(
                existing.Id,
                existing.ProcurementStatus,
                0,
                true);
        }

        var lines = await db.TripPartsDemandLines
            .Where(x =>
                x.TenantId == request.TenantId
                && x.RoutarrPublicationId == request.RoutarrPublicationId)
            .ToListAsync(cancellationToken);

        if (lines.Count == 0)
        {
            throw new StlApiException(
                "trip_parts_demand_status.publication_not_found",
                "No published trip parts demand lines were found for the RoutArr publication id.",
                404);
        }

        var now = DateTimeOffset.UtcNow;
        var statusEvent = new TripPartsDemandStatusEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            RoutarrPublicationId = request.RoutarrPublicationId,
            SupplyarrDemandRefId = request.SupplyarrDemandRefId,
            SupplyarrCallbackPublicationId = request.SupplyarrCallbackPublicationId,
            EventType = request.EventType.Trim().ToLowerInvariant(),
            ProcurementStatus = request.ProcurementStatus.Trim().ToLowerInvariant(),
            SupplyarrPurchaseRequestId = request.SupplyarrPurchaseRequestId,
            SupplyarrPurchaseOrderId = request.SupplyarrPurchaseOrderId,
            SupplyarrReceivingReceiptId = request.SupplyarrReceivingReceiptId,
            Message = request.Message?.Trim() ?? string.Empty,
            OccurredAt = request.OccurredAt,
            CreatedAt = now,
        };

        db.TripPartsDemandStatusEvents.Add(statusEvent);

        foreach (var line in lines)
        {
            line.ProcurementStatus = statusEvent.ProcurementStatus;
            line.ProcurementStatusMessage = statusEvent.Message;
            line.LastProcurementStatusAt = request.OccurredAt;

            if (request.SupplyarrPurchaseRequestId.HasValue)
            {
                line.SupplyarrPurchaseRequestId = request.SupplyarrPurchaseRequestId;
            }

            if (request.SupplyarrPurchaseOrderId.HasValue)
            {
                line.SupplyarrPurchaseOrderId = request.SupplyarrPurchaseOrderId;
            }

            if (request.QuantityReceivedDelta is > 0)
            {
                line.QuantityReceived += request.QuantityReceivedDelta.Value;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trip_parts_demand_status.ingest",
            request.TenantId,
            null,
            "supplyarr_demand_status_callback",
            statusEvent.Id.ToString(),
            lines[0].TripId.ToString(),
            cancellationToken: cancellationToken);

        return new IngestSupplyarrDemandStatusResponse(
            statusEvent.Id,
            statusEvent.ProcurementStatus,
            lines.Count,
            false);
    }

    private static void ValidateRequest(IngestSupplyarrDemandStatusRequest request)
    {
        if (request.RoutarrPublicationId == Guid.Empty)
        {
            throw new StlApiException(
                "trip_parts_demand_status.validation",
                "RoutArr publication id is required.",
                400);
        }

        if (request.SupplyarrDemandRefId == Guid.Empty)
        {
            throw new StlApiException(
                "trip_parts_demand_status.validation",
                "SupplyArr demand reference id is required.",
                400);
        }

        if (request.SupplyarrCallbackPublicationId == Guid.Empty)
        {
            throw new StlApiException(
                "trip_parts_demand_status.validation",
                "SupplyArr callback publication id is required.",
                400);
        }

        if (!TripPartsDemandStatusEventTypes.All.Contains(request.EventType))
        {
            throw new StlApiException(
                "trip_parts_demand_status.validation",
                "Unsupported demand status event type.",
                400);
        }

        if (!TripPartsDemandProcurementStatuses.All.Contains(request.ProcurementStatus))
        {
            throw new StlApiException(
                "trip_parts_demand_status.validation",
                "Unsupported procurement status.",
                400);
        }
    }
}
