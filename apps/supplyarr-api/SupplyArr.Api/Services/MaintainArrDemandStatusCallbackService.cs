using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class MaintainArrDemandStatusCallbackService(
    SupplyArrDbContext db,
    MaintainArrDemandStatusClient maintainArrDemandStatusClient,
    ISupplyArrAuditService audit)
{
    public Task NotifyPrDraftedAsync(
        MaintainArrDemandRef demandRef,
        Guid purchaseRequestId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        NotifyDemandRefAsync(
            demandRef,
            WorkOrderPartsDemandStatusEventTypes.PrDrafted,
            MaintainArrDemandRefProcurementStatuses.PrDrafted,
            purchaseRequestId,
            null,
            null,
            null,
            "Purchase request draft created from MaintainArr demand.",
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
            WorkOrderPartsDemandStatusEventTypes.PrSubmitted,
            MaintainArrDemandRefProcurementStatuses.PrSubmitted,
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
            WorkOrderPartsDemandStatusEventTypes.PrApproved,
            MaintainArrDemandRefProcurementStatuses.PrApproved,
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
            WorkOrderPartsDemandStatusEventTypes.PrRejected,
            MaintainArrDemandRefProcurementStatuses.PrRejected,
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
            WorkOrderPartsDemandStatusEventTypes.PoCreated,
            MaintainArrDemandRefProcurementStatuses.PoCreated,
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
            WorkOrderPartsDemandStatusEventTypes.PoIssued,
            MaintainArrDemandRefProcurementStatuses.PoIssued,
            null,
            "Purchase order issued to vendor.",
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
            ? WorkOrderPartsDemandStatusEventTypes.ReceivingComplete
            : WorkOrderPartsDemandStatusEventTypes.ReceivingPosted;
        var procurementStatus = fullyReceived
            ? MaintainArrDemandRefProcurementStatuses.ReceivedComplete
            : MaintainArrDemandRefProcurementStatuses.PartiallyReceived;
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
        var demandRefs = await db.MaintainArrDemandRefs
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
        MaintainArrDemandRef demandRef,
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

        var callbackPublicationId = CreateCallbackPublicationId(
            demandRef.TenantId,
            demandRef.Id,
            eventType,
            sourceRecordId);

        await maintainArrDemandStatusClient.PublishStatusAsync(
            new MaintainArrDemandStatusCallbackPayload(
                demandRef.TenantId,
                demandRef.MaintainarrPublicationId,
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
            "demand_status.maintainarr.callback",
            demandRef.TenantId,
            null,
            "maintainarr_demand_ref",
            demandRef.Id.ToString(),
            eventType,
            cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<MaintainArrDemandRef>> LoadDemandRefsByPurchaseOrderAsync(
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

        return await db.MaintainArrDemandRefs
            .Where(x => x.TenantId == tenantId && x.PurchaseRequestId == purchaseOrder.PurchaseRequestId)
            .ToListAsync(cancellationToken);
    }

    private static Guid CreateCallbackPublicationId(
        Guid tenantId,
        Guid demandRefId,
        string eventType,
        Guid sourceRecordId)
    {
        var input = $"{tenantId:N}:{demandRefId:N}:{eventType}:{sourceRecordId:N}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash.AsSpan(0, 16));
    }
}

public static class WorkOrderPartsDemandStatusEventTypes
{
    public const string PrDrafted = "pr_drafted";

    public const string PrSubmitted = "pr_submitted";

    public const string PrApproved = "pr_approved";

    public const string PrRejected = "pr_rejected";

    public const string PoCreated = "po_created";

    public const string PoIssued = "po_issued";

    public const string ReceivingPosted = "receiving_posted";

    public const string ReceivingComplete = "receiving_complete";
}
