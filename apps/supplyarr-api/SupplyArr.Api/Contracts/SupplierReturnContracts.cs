namespace SupplyArr.Api.Contracts;

public record SupplierReturnLineResponse(
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

public record SupplierReturnResponse(
    Guid ReturnId,
    string ReturnKey,
    string Status,
    string SourceType,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
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
    IReadOnlyList<SupplierReturnLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateSupplierReturnFromStockLineRequest(
    Guid PartId,
    decimal Quantity,
    string? Notes);

public record CreateSupplierReturnFromStockRequest(
    string ReturnKey,
    Guid? SupplierUnitId,
    Guid? SupplierId,
    Guid InventoryBinId,
    string? RmaNumber,
    string? Notes,
    IReadOnlyList<CreateSupplierReturnFromStockLineRequest> Lines);

public record CreateSupplierReturnFromPurchaseOrderLineRequest(
    string ReturnKey,
    Guid InventoryBinId,
    decimal? Quantity,
    string? RmaNumber,
    string? Notes);

public record CancelSupplierReturnRequest(string Reason);
