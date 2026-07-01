using SupplyArr.Api.Contracts;
using SupplyArr.Api.Entities;
using System.Text.Json;

namespace SupplyArr.Api.Services;

public static class ProcurementCoordinationBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static ProcurementCoordinationComputation BuildFromPurchaseRequest(
        PurchaseRequest purchaseRequest,
        bool hasOpenPurchaseOrder,
        DateTimeOffset asOfUtc)
    {
        var lineCount = purchaseRequest.Lines.Count;
        var supplierDisplayName = purchaseRequest.Supplier?.DisplayName ?? string.Empty;
        var supplierKey = purchaseRequest.Supplier?.SupplierKey;
        var sourceUpdatedAt = purchaseRequest.UpdatedAt;

        var (stage, nextAction, isTerminal) = ResolvePurchaseRequestStage(
            purchaseRequest.Status,
            hasOpenPurchaseOrder);

        var summary = new ProcurementCoordinationSummaryResponse(
            Guid.Empty,
            ProcurementCoordinationSubjectTypes.PurchaseRequest,
            purchaseRequest.Id,
            purchaseRequest.RequestKey,
            purchaseRequest.Title,
            stage,
            nextAction,
            purchaseRequest.Id,
            null,
            purchaseRequest.SupplierId,
            supplierKey,
            supplierDisplayName,
            purchaseRequest.Supplier?.ParentSupplierId,
            purchaseRequest.Supplier?.ParentSupplier?.DisplayName,
            purchaseRequest.Supplier?.UnitKind,
            ParseServiceTypes(purchaseRequest.Supplier?.ServiceTypesJson),
            purchaseRequest.Status,
            lineCount,
            0m,
            0m,
            null,
            isTerminal,
            sourceUpdatedAt,
            asOfUtc,
            false);

        var events = BuildPurchaseRequestEvents(purchaseRequest);
        return new ProcurementCoordinationComputation(summary, events);
    }

    public static ProcurementCoordinationComputation BuildFromPurchaseOrder(
        PurchaseOrder purchaseOrder,
        DateTimeOffset asOfUtc)
    {
        var lineCount = purchaseOrder.Lines.Count;
        var quantityOrdered = purchaseOrder.Lines.Sum(x => x.QuantityOrdered);
        var quantityReceived = purchaseOrder.Lines.Sum(x => x.QuantityReceived);
        var receiptProgressPercent = ProcurementCoordinationRules.ComputeReceiptProgressPercent(
            quantityOrdered,
            quantityReceived);
        var supplierDisplayName = purchaseOrder.Supplier?.DisplayName ?? string.Empty;
        var supplierKey = purchaseOrder.Supplier?.SupplierKey;
        var sourceUpdatedAt = MaxUpdatedAt(
            purchaseOrder.UpdatedAt,
            purchaseOrder.Lines.Count > 0 ? purchaseOrder.Lines.Max(x => x.UpdatedAt) : purchaseOrder.UpdatedAt);

        var (stage, nextAction, isTerminal) = ResolvePurchaseOrderStage(
            purchaseOrder.Status,
            quantityOrdered,
            quantityReceived);

        var summary = new ProcurementCoordinationSummaryResponse(
            Guid.Empty,
            ProcurementCoordinationSubjectTypes.PurchaseOrder,
            purchaseOrder.Id,
            purchaseOrder.OrderKey,
            purchaseOrder.Title,
            stage,
            nextAction,
            purchaseOrder.PurchaseRequestId,
            purchaseOrder.Id,
            purchaseOrder.SupplierId,
            supplierKey,
            supplierDisplayName,
            purchaseOrder.Supplier?.ParentSupplierId,
            purchaseOrder.Supplier?.ParentSupplier?.DisplayName,
            purchaseOrder.Supplier?.UnitKind,
            ParseServiceTypes(purchaseOrder.Supplier?.ServiceTypesJson),
            purchaseOrder.Status,
            lineCount,
            quantityOrdered,
            quantityReceived,
            receiptProgressPercent,
            isTerminal,
            sourceUpdatedAt,
            asOfUtc,
            false);

        var events = BuildPurchaseOrderEvents(purchaseOrder, quantityOrdered, quantityReceived);
        return new ProcurementCoordinationComputation(summary, events);
    }

    private static (string Stage, string NextAction, bool IsTerminal) ResolvePurchaseRequestStage(
        string status,
        bool hasOpenPurchaseOrder)
    {
        if (string.Equals(status, PurchaseRequestStatuses.Submitted, StringComparison.OrdinalIgnoreCase))
        {
            return (
                ProcurementCoordinationStages.AwaitingPrApproval,
                "Approve or reject purchase request",
                false);
        }

        if (string.Equals(status, PurchaseRequestStatuses.Approved, StringComparison.OrdinalIgnoreCase))
        {
            if (hasOpenPurchaseOrder)
            {
                throw new InvalidOperationException(
                    "Approved purchase request with open purchase order should be coordinated at PO level.");
            }

            return (
                ProcurementCoordinationStages.AwaitingPoCreation,
                "Create purchase order from approved request",
                false);
        }

        if (string.Equals(status, PurchaseRequestStatuses.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            return (
                ProcurementCoordinationStages.Rejected,
                "No action required",
                true);
        }

        throw new InvalidOperationException($"Purchase request status '{status}' is not eligible for coordination.");
    }

    private static (string Stage, string NextAction, bool IsTerminal) ResolvePurchaseOrderStage(
        string status,
        decimal quantityOrdered,
        decimal quantityReceived)
    {
        if (string.Equals(status, PurchaseOrderStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            return (
                ProcurementCoordinationStages.PoAwaitingApproval,
                "Approve purchase order",
                false);
        }

        if (string.Equals(status, PurchaseOrderStatuses.Approved, StringComparison.OrdinalIgnoreCase))
        {
            return (
                ProcurementCoordinationStages.PoAwaitingIssue,
                "Issue purchase order to supplier",
                false);
        }

        if (string.Equals(status, PurchaseOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            return (
                ProcurementCoordinationStages.Cancelled,
                "No action required",
                true);
        }

        if (string.Equals(status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase))
        {
            if (quantityReceived <= 0)
            {
                return (
                    ProcurementCoordinationStages.AwaitingReceipt,
                    "Post receiving receipt",
                    false);
            }

            if (quantityReceived < quantityOrdered)
            {
                return (
                    ProcurementCoordinationStages.PartialReceipt,
                    "Complete receiving for remaining quantity",
                    false);
            }

            return (
                ProcurementCoordinationStages.Fulfilled,
                "No action required",
                true);
        }

        throw new InvalidOperationException($"Purchase order status '{status}' is not eligible for coordination.");
    }

    private static IReadOnlyList<ProcurementCoordinationEventResponse> BuildPurchaseRequestEvents(
        PurchaseRequest purchaseRequest)
    {
        var events = new List<ProcurementCoordinationEventResponse>();
        var sequence = 1;

        if (purchaseRequest.SubmittedAt.HasValue)
        {
            events.Add(new ProcurementCoordinationEventResponse(
                ProcurementCoordinationEventKinds.PrSubmitted,
                "Purchase request submitted",
                purchaseRequest.Title,
                purchaseRequest.SubmittedAt.Value,
                sequence++,
                "purchase_request",
                purchaseRequest.Id.ToString()));
        }

        if (purchaseRequest.ApprovedAt.HasValue)
        {
            events.Add(new ProcurementCoordinationEventResponse(
                ProcurementCoordinationEventKinds.PrApproved,
                "Purchase request approved",
                null,
                purchaseRequest.ApprovedAt.Value,
                sequence++,
                "purchase_request",
                purchaseRequest.Id.ToString()));
        }

        if (purchaseRequest.RejectedAt.HasValue)
        {
            events.Add(new ProcurementCoordinationEventResponse(
                ProcurementCoordinationEventKinds.PrRejected,
                "Purchase request rejected",
                string.IsNullOrWhiteSpace(purchaseRequest.RejectionReason)
                    ? null
                    : purchaseRequest.RejectionReason,
                purchaseRequest.RejectedAt.Value,
                sequence,
                "purchase_request",
                purchaseRequest.Id.ToString()));
        }

        return events;
    }

    private static IReadOnlyList<ProcurementCoordinationEventResponse> BuildPurchaseOrderEvents(
        PurchaseOrder purchaseOrder,
        decimal quantityOrdered,
        decimal quantityReceived)
    {
        var events = new List<ProcurementCoordinationEventResponse>
        {
            new(
                ProcurementCoordinationEventKinds.PoCreated,
                "Purchase order created",
                purchaseOrder.Title,
                purchaseOrder.CreatedAt,
                1,
                "purchase_order",
                purchaseOrder.Id.ToString()),
        };

        var sequence = 2;

        if (purchaseOrder.ApprovedAt.HasValue)
        {
            events.Add(new ProcurementCoordinationEventResponse(
                ProcurementCoordinationEventKinds.PoApproved,
                "Purchase order approved",
                null,
                purchaseOrder.ApprovedAt.Value,
                sequence++,
                "purchase_order",
                purchaseOrder.Id.ToString()));
        }

        if (purchaseOrder.IssuedAt.HasValue)
        {
            events.Add(new ProcurementCoordinationEventResponse(
                ProcurementCoordinationEventKinds.PoIssued,
                "Purchase order issued",
                null,
                purchaseOrder.IssuedAt.Value,
                sequence++,
                "purchase_order",
                purchaseOrder.Id.ToString()));
        }

        if (purchaseOrder.CancelledAt.HasValue)
        {
            events.Add(new ProcurementCoordinationEventResponse(
                ProcurementCoordinationEventKinds.PoCancelled,
                "Purchase order cancelled",
                string.IsNullOrWhiteSpace(purchaseOrder.CancellationReason)
                    ? null
                    : purchaseOrder.CancellationReason,
                purchaseOrder.CancelledAt.Value,
                sequence++,
                "purchase_order",
                purchaseOrder.Id.ToString()));
        }

        if (string.Equals(purchaseOrder.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase)
            && quantityReceived > 0)
        {
            var eventKind = quantityReceived >= quantityOrdered
                ? ProcurementCoordinationEventKinds.ReceiptComplete
                : ProcurementCoordinationEventKinds.ReceiptProgress;

            events.Add(new ProcurementCoordinationEventResponse(
                eventKind,
                quantityReceived >= quantityOrdered ? "Receiving complete" : "Partial receiving posted",
                $"{quantityReceived} of {quantityOrdered} received",
                purchaseOrder.UpdatedAt,
                sequence,
                "purchase_order",
                purchaseOrder.Id.ToString()));
        }

        return events;
    }

    private static DateTimeOffset MaxUpdatedAt(DateTimeOffset first, DateTimeOffset second) =>
        first >= second ? first : second;

    private static IReadOnlyList<string> ParseServiceTypes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

public sealed record ProcurementCoordinationComputation(
    ProcurementCoordinationSummaryResponse Summary,
    IReadOnlyList<ProcurementCoordinationEventResponse> Events);

