using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class IncidentSupplyDemandStatusIngestionService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<IngestSupplyarrDemandStatusResponse> IngestAsync(
        IngestSupplyarrDemandStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var existing = await db.IncidentSupplyDemandStatusEvents
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

        var lines = await db.IncidentSupplyDemandLines
            .Where(x =>
                x.TenantId == request.TenantId
                && x.StaffarrPublicationId == request.StaffarrPublicationId)
            .ToListAsync(cancellationToken);

        if (lines.Count == 0)
        {
            throw new StlApiException(
                "trip_parts_demand_status.publication_not_found",
                "No published incident supply demand lines were found for the Staffarr publication id.",
                404);
        }

        var now = DateTimeOffset.UtcNow;
        var statusEvent = new IncidentSupplyDemandStatusEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StaffarrPublicationId = request.StaffarrPublicationId,
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

        db.IncidentSupplyDemandStatusEvents.Add(statusEvent);

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
            lines[0].IncidentId.ToString(),
            cancellationToken: cancellationToken);

        return new IngestSupplyarrDemandStatusResponse(
            statusEvent.Id,
            statusEvent.ProcurementStatus,
            lines.Count,
            false);
    }

    private static void ValidateRequest(IngestSupplyarrDemandStatusRequest request)
    {
        if (request.StaffarrPublicationId == Guid.Empty)
        {
            throw new StlApiException(
                "trip_parts_demand_status.validation",
                "Staffarr publication id is required.",
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

        if (!IncidentSupplyDemandStatusEventTypes.All.Contains(request.EventType))
        {
            throw new StlApiException(
                "trip_parts_demand_status.validation",
                "Unsupported demand status event type.",
                400);
        }

        if (!IncidentSupplyDemandProcurementStatuses.All.Contains(request.ProcurementStatus))
        {
            throw new StlApiException(
                "trip_parts_demand_status.validation",
                "Unsupported procurement status.",
                400);
        }
    }
}

