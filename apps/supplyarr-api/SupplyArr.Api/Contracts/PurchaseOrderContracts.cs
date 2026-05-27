namespace SupplyArr.Api.Contracts;

public sealed record PurchaseOrderLineResponse(
    Guid LineId,
    int LineNumber,
    Guid? PurchaseRequestLineId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    decimal QuantityRemaining,
    string UnitOfMeasure,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PurchaseOrderResponse(
    Guid PurchaseOrderId,
    string OrderKey,
    string Title,
    string Notes,
    string Status,
    Guid PurchaseRequestId,
    string PurchaseRequestKey,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName,
    Guid CreatedByUserId,
    DateTimeOffset? ApprovedAt,
    Guid? ApprovedByUserId,
    DateTimeOffset? IssuedAt,
    Guid? IssuedByUserId,
    DateTimeOffset? CancelledAt,
    Guid? CancelledByUserId,
    string CancellationReason,
    IReadOnlyList<PurchaseOrderLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePurchaseOrderFromPurchaseRequestRequest(
    string OrderKey,
    string? Title,
    string? Notes);

public sealed record UpdatePurchaseOrderRequest(
    string Title,
    string Notes);

public sealed record AddPurchaseOrderLineRequest(
    Guid PartId,
    decimal QuantityOrdered,
    string Notes);

public sealed record UpdatePurchaseOrderLineRequest(
    decimal QuantityOrdered,
    string Notes);

public sealed record CancelPurchaseOrderRequest(string Reason);
