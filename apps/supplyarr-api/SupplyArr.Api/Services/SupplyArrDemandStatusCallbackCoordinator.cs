namespace SupplyArr.Api.Services;

public sealed class SupplyArrDemandStatusCallbackCoordinator(
    MaintainArrDemandStatusCallbackService maintainArr,
    RoutArrDemandStatusCallbackService routArr,
    TrainArrDemandStatusCallbackService trainArr,
    StaffArrDemandStatusCallbackService staffArr)
{
    public Task NotifyPurchaseRequestSubmittedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        Task.WhenAll(
            maintainArr.NotifyPurchaseRequestSubmittedAsync(tenantId, purchaseRequestId, occurredAt, cancellationToken),
            routArr.NotifyPurchaseRequestSubmittedAsync(tenantId, purchaseRequestId, occurredAt, cancellationToken),
            trainArr.NotifyPurchaseRequestSubmittedAsync(tenantId, purchaseRequestId, occurredAt, cancellationToken),
            staffArr.NotifyPurchaseRequestSubmittedAsync(tenantId, purchaseRequestId, occurredAt, cancellationToken));

    public Task NotifyPurchaseRequestApprovedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        Task.WhenAll(
            maintainArr.NotifyPurchaseRequestApprovedAsync(tenantId, purchaseRequestId, occurredAt, cancellationToken),
            routArr.NotifyPurchaseRequestApprovedAsync(tenantId, purchaseRequestId, occurredAt, cancellationToken),
            trainArr.NotifyPurchaseRequestApprovedAsync(tenantId, purchaseRequestId, occurredAt, cancellationToken),
            staffArr.NotifyPurchaseRequestApprovedAsync(tenantId, purchaseRequestId, occurredAt, cancellationToken));

    public Task NotifyPurchaseRequestRejectedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        string reason,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        Task.WhenAll(
            maintainArr.NotifyPurchaseRequestRejectedAsync(tenantId, purchaseRequestId, reason, occurredAt, cancellationToken),
            routArr.NotifyPurchaseRequestRejectedAsync(tenantId, purchaseRequestId, reason, occurredAt, cancellationToken),
            trainArr.NotifyPurchaseRequestRejectedAsync(tenantId, purchaseRequestId, reason, occurredAt, cancellationToken),
            staffArr.NotifyPurchaseRequestRejectedAsync(tenantId, purchaseRequestId, reason, occurredAt, cancellationToken));

    public Task NotifyPurchaseOrderCreatedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        Guid purchaseOrderId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        Task.WhenAll(
            maintainArr.NotifyPurchaseOrderCreatedAsync(tenantId, purchaseRequestId, purchaseOrderId, occurredAt, cancellationToken),
            routArr.NotifyPurchaseOrderCreatedAsync(tenantId, purchaseRequestId, purchaseOrderId, occurredAt, cancellationToken),
            trainArr.NotifyPurchaseOrderCreatedAsync(tenantId, purchaseRequestId, purchaseOrderId, occurredAt, cancellationToken),
            staffArr.NotifyPurchaseOrderCreatedAsync(tenantId, purchaseRequestId, purchaseOrderId, occurredAt, cancellationToken));

    public Task NotifyPurchaseOrderIssuedAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        Task.WhenAll(
            maintainArr.NotifyPurchaseOrderIssuedAsync(tenantId, purchaseOrderId, occurredAt, cancellationToken),
            routArr.NotifyPurchaseOrderIssuedAsync(tenantId, purchaseOrderId, occurredAt, cancellationToken),
            trainArr.NotifyPurchaseOrderIssuedAsync(tenantId, purchaseOrderId, occurredAt, cancellationToken),
            staffArr.NotifyPurchaseOrderIssuedAsync(tenantId, purchaseOrderId, occurredAt, cancellationToken));

    public Task NotifyReceivingPostedAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        Guid receivingReceiptId,
        decimal quantityReceivedDelta,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default) =>
        Task.WhenAll(
            maintainArr.NotifyReceivingPostedAsync(tenantId, purchaseOrderId, receivingReceiptId, quantityReceivedDelta, occurredAt, cancellationToken),
            routArr.NotifyReceivingPostedAsync(tenantId, purchaseOrderId, receivingReceiptId, quantityReceivedDelta, occurredAt, cancellationToken),
            trainArr.NotifyReceivingPostedAsync(tenantId, purchaseOrderId, receivingReceiptId, quantityReceivedDelta, occurredAt, cancellationToken),
            staffArr.NotifyReceivingPostedAsync(tenantId, purchaseOrderId, receivingReceiptId, quantityReceivedDelta, occurredAt, cancellationToken));
}
