namespace SupplyArr.Api.Contracts;



public sealed record VendorReturnLineResponse(

    Guid LineId,

    int LineNumber,

    Guid PartId,

    string PartKey,

    string PartDisplayName,

    Guid? PurchaseOrderLineId,

    int? PurchaseOrderLineNumber,

    decimal Quantity,

    string Notes,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record VendorReturnResponse(

    Guid ReturnId,

    string ReturnKey,

    string Status,

    string SourceType,

    Guid VendorPartyId,

    string VendorPartyKey,

    string VendorDisplayName,

    Guid? PurchaseOrderId,

    string? PurchaseOrderKey,

    Guid? PurchaseRequestId,

    string? PurchaseRequestKey,

    Guid InventoryBinId,

    string InventoryBinKey,

    string InventoryBinName,

    Guid InventoryLocationId,

    string InventoryLocationKey,

    string InventoryLocationName,

    string RmaNumber,

    string Notes,

    Guid CreatedByUserId,

    Guid? PostedByUserId,

    DateTimeOffset? PostedAt,

    Guid? CancelledByUserId,

    DateTimeOffset? CancelledAt,

    string CancellationReason,

    IReadOnlyList<VendorReturnLineResponse> Lines,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record CreateVendorReturnFromStockLineRequest(

    Guid PartId,

    decimal Quantity,

    string? Notes);



public sealed record CreateVendorReturnFromStockRequest(

    string ReturnKey,

    Guid VendorPartyId,

    Guid InventoryBinId,

    string? RmaNumber,

    string? Notes,

    IReadOnlyList<CreateVendorReturnFromStockLineRequest> Lines);



public sealed record CreateVendorReturnFromPurchaseOrderLineRequest(

    string ReturnKey,

    Guid InventoryBinId,

    decimal? Quantity,

    string? RmaNumber,

    string? Notes);



public sealed record CancelVendorReturnRequest(string Reason);

