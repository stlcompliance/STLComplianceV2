using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class StaffArrDemandStatusCallbackService(
    SupplyArrDbContext db,
    StaffArrDemandStatusClient StaffArrDemandStatusClient,
    ISupplyArrAuditService audit)
{
    public Task NotifyPrDraftedAsync(
        StaffArrDemandRef demandRef,
        Guid purchaseRequestId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        NotifyDemandRefAsync(
            demandRef,
            SupplyArrDemandStatusEventTypes.PrDrafted,
            StaffArrDemandRefProcurementStatuses.PrDrafted,
            purchaseRequestId,
            null,
            null,
            null,
            "Purchase request draft created from StaffArr demand.",
            occurredAt,
            cancellationToken);

    public Task NotifyPurchaseRequestSubmittedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        NotifyByPurchaseRequestAsync(
            tenantId,
            purchaseRequestId,
            SupplyArrDemandStatusEventTypes.PrSubmitted,
            StaffArrDemandRefProcurementStatuses.PrSubmitted,
            null,
            null,
            "Purchase request submitted for approval.",
            occurredAt,
            cancellationToken);

    public Task NotifyPurchaseRequestApprovedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        NotifyByPurchaseRequestAsync(
            tenantId,
            purchaseRequestId,
            SupplyArrDemandStatusEventTypes.PrApproved,
            StaffArrDemandRefProcurementStatuses.PrApproved,
            null,
            null,
            "Purchase request approved.",
            occurredAt,
            cancellationToken);

    public Task NotifyPurchaseRequestRejectedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        string reason,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        NotifyByPurchaseRequestAsync(
            tenantId,
            purchaseRequestId,
            SupplyArrDemandStatusEventTypes.PrRejected,
            StaffArrDemandRefProcurementStatuses.PrRejected,
            null,
            null,
            string.IsNullOrWhiteSpace(reason) ? "Purchase request rejected." : reason.Trim(),
            occurredAt,
            cancellationToken);

    public Task NotifyPurchaseOrderCreatedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        Guid purchaseOrderId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        NotifyByPurchaseRequestAsync(
            tenantId,
            purchaseRequestId,
            SupplyArrDemandStatusEventTypes.PoCreated,
            StaffArrDemandRefProcurementStatuses.PoCreated,
            purchaseOrderId,
            null,
            "Purchase order created from approved purchase request.",
            occurredAt,
            cancellationToken);

    public Task NotifyPurchaseOrderIssuedAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        NotifyByPurchaseOrderAsync(
            tenantId,
            purchaseOrderId,
            SupplyArrDemandStatusEventTypes.PoIssued,
            StaffArrDemandRefProcurementStatuses.PoIssued,
            null,
            "Purchase order issued to supplier.",
            occurredAt,
            cancellationToken);

    public async Task NotifyReceivingPostedAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        Guid receivingReceiptId,
        decimal quantityReceivedDelta,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var demandRefs = await LoadDemandRefsByPurchaseOrderAsync(tenantId, purchaseOrderId, cancellationToken);
        if (demandRefs.Count == 0)
        {
            return;
        }

        var purchaseOrder = await db.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.Lines)
            .FirstAsync(x => x.TenantId == tenantId && x.Id == purchaseOrderId, cancellationToken);

        var fullyReceived = purchaseOrder.Lines.All(x => x.QuantityReceived >= x.QuantityOrdered);
        var eventType = fullyReceived
            ? SupplyArrDemandStatusEventTypes.ReceivingComplete
            : SupplyArrDemandStatusEventTypes.ReceivingPosted;
        var procurementStatus = fullyReceived
            ? StaffArrDemandRefProcurementStatuses.ReceivedComplete
            : StaffArrDemandRefProcurementStatuses.PartiallyReceived;
        var message = fullyReceived
            ? "All purchase order lines fully received."
            : "Receiving receipt posted against purchase order.";

        foreach (var demandRef in demandRefs)
        {
            await NotifyDemandRefAsync(
                demandRef,
                eventType,
                procurementStatus,
                demandRef.PurchaseRequestId,
                purchaseOrderId,
                receivingReceiptId,
                quantityReceivedDelta,
                message,
                occurredAt,
                cancellationToken);
        }
    }

    private async Task NotifyByPurchaseRequestAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        string eventType,
        string procurementStatus,
        Guid? purchaseOrderId,
        decimal? quantityReceivedDelta,
        string message,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var demandRefs = await db.StaffArrDemandRefs
            .Where(x => x.TenantId == tenantId && x.PurchaseRequestId == purchaseRequestId)
            .ToListAsync(cancellationToken);

        foreach (var demandRef in demandRefs)
        {
            await NotifyDemandRefAsync(
                demandRef,
                eventType,
                procurementStatus,
                purchaseRequestId,
                purchaseOrderId ?? demandRef.PurchaseOrderId,
                null,
                quantityReceivedDelta,
                message,
                occurredAt,
                cancellationToken);
        }
    }

    private async Task NotifyByPurchaseOrderAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        string eventType,
        string procurementStatus,
        decimal? quantityReceivedDelta,
        string message,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var demandRefs = await LoadDemandRefsByPurchaseOrderAsync(tenantId, purchaseOrderId, cancellationToken);
        foreach (var demandRef in demandRefs)
        {
            await NotifyDemandRefAsync(
                demandRef,
                eventType,
                procurementStatus,
                demandRef.PurchaseRequestId,
                purchaseOrderId,
                null,
                quantityReceivedDelta,
                message,
                occurredAt,
                cancellationToken);
        }
    }

    private async Task NotifyDemandRefAsync(
        StaffArrDemandRef demandRef,
        string eventType,
        string procurementStatus,
        Guid? purchaseRequestId,
        Guid? purchaseOrderId,
        Guid? receivingReceiptId,
        decimal? quantityReceivedDelta,
        string message,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        if (purchaseOrderId.HasValue)
        {
            demandRef.PurchaseOrderId = purchaseOrderId;
        }

        demandRef.ProcurementStatus = procurementStatus;
        demandRef.LastStatusCallbackAt = occurredAt;
        demandRef.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var sourceRecordId = receivingReceiptId
            ?? purchaseOrderId
            ?? purchaseRequestId
            ?? demandRef.Id;

        var callbackPublicationId = DemandStatusCallbackPublicationId.Create(
            demandRef.TenantId,
            demandRef.Id,
            eventType,
            sourceRecordId);

        await StaffArrDemandStatusClient.PublishStatusAsync(
            new StaffArrDemandStatusCallbackPayload(
                demandRef.TenantId,
                demandRef.StaffarrPublicationId,
                demandRef.Id,
                callbackPublicationId,
                eventType,
                procurementStatus,
                purchaseRequestId ?? demandRef.PurchaseRequestId,
                purchaseOrderId ?? demandRef.PurchaseOrderId,
                receivingReceiptId,
                quantityReceivedDelta,
                message,
                occurredAt),
            cancellationToken);

        await audit.WriteAsync(
            "demand_status.StaffArr.callback",
            demandRef.TenantId,
            null,
            "StaffArr_demand_ref",
            demandRef.Id.ToString(),
            eventType,
            cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<StaffArrDemandRef>> LoadDemandRefsByPurchaseOrderAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await db.PurchaseOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == purchaseOrderId, cancellationToken);

        if (purchaseOrder?.PurchaseRequestId is null)
        {
            return [];
        }

        return await db.StaffArrDemandRefs
            .Where(x => x.TenantId == tenantId && x.PurchaseRequestId == purchaseOrder.PurchaseRequestId)
            .ToListAsync(cancellationToken);
    }
}
