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
    DateTimeOffset UpdatedAt)
    : SupplierReturnLineResponse(
        LineId,
        LineNumber,
        PartId,
        PartKey,
        PartDisplayName,
        PurchaseOrderLineId,
        PurchaseOrderLineNumber,
        Quantity,
        Notes,
        CreatedAt,
        UpdatedAt);

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
    Guid? VendorPartyId,
    string? VendorPartyKey,
    string? VendorDisplayName,
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

public sealed record VendorReturnResponse(
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
    Guid? VendorPartyId,
    string? VendorPartyKey,
    string? VendorDisplayName,
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
    DateTimeOffset UpdatedAt)
    : SupplierReturnResponse(
        ReturnId,
        ReturnKey,
        Status,
        SourceType,
        SupplierId,
        SupplierKey,
        SupplierDisplayName,
        ParentSupplierId,
        ParentSupplierDisplayName,
        SupplierUnitKind,
        SupplierServiceTypes,
        VendorPartyId,
        VendorPartyKey,
        VendorDisplayName,
        PurchaseOrderId,
        PurchaseOrderKey,
        PurchaseRequestId,
        PurchaseRequestKey,
        InventoryBinId,
        InventoryBinKey,
        InventoryBinName,
        InventoryLocationId,
        InventoryLocationKey,
        InventoryLocationName,
        RmaNumber,
        Notes,
        CreatedByUserId,
        PostedByUserId,
        PostedAt,
        CancelledByUserId,
        CancelledAt,
        CancellationReason,
        Lines,
        CreatedAt,
        UpdatedAt);

public sealed record CreateVendorReturnFromStockLineRequest(
    Guid PartId,
    decimal Quantity,
    string? Notes);

public record CreateSupplierReturnFromStockRequest(
    string ReturnKey,
    Guid? SupplierUnitId,
    Guid? SupplierId,
    Guid? VendorPartyId,
    Guid InventoryBinId,
    string? RmaNumber,
    string? Notes,
    IReadOnlyList<CreateVendorReturnFromStockLineRequest> Lines);

public sealed record CreateVendorReturnFromStockRequest(
    string ReturnKey,
    Guid? SupplierUnitId,
    Guid? SupplierId,
    Guid? VendorPartyId,
    Guid InventoryBinId,
    string? RmaNumber,
    string? Notes,
    IReadOnlyList<CreateVendorReturnFromStockLineRequest> Lines)
    : CreateSupplierReturnFromStockRequest(
        ReturnKey,
        SupplierUnitId,
        SupplierId,
        VendorPartyId,
        InventoryBinId,
        RmaNumber,
        Notes,
        Lines)
{
    public CreateVendorReturnFromStockRequest(
        string returnKey,
        Guid vendorPartyId,
        Guid inventoryBinId,
        string? rmaNumber,
        string? notes,
        IReadOnlyList<CreateVendorReturnFromStockLineRequest> lines)
        : this(returnKey, vendorPartyId, vendorPartyId, vendorPartyId, inventoryBinId, rmaNumber, notes, lines)
    {
    }

    public CreateVendorReturnFromStockRequest(
        string returnKey,
        Guid? vendorPartyId,
        Guid inventoryBinId,
        string? rmaNumber,
        string? notes,
        IReadOnlyList<CreateVendorReturnFromStockLineRequest> lines)
        : this(returnKey, vendorPartyId, vendorPartyId, vendorPartyId, inventoryBinId, rmaNumber, notes, lines)
    {
    }
}

public record CreateSupplierReturnFromPurchaseOrderLineRequest(
    string ReturnKey,
    Guid InventoryBinId,
    decimal? Quantity,
    string? RmaNumber,
    string? Notes);

public sealed record CreateVendorReturnFromPurchaseOrderLineRequest(
    string ReturnKey,
    Guid InventoryBinId,
    decimal? Quantity,
    string? RmaNumber,
    string? Notes)
    : CreateSupplierReturnFromPurchaseOrderLineRequest(ReturnKey, InventoryBinId, Quantity, RmaNumber, Notes);

public record CancelSupplierReturnRequest(string Reason);

public sealed record CancelVendorReturnRequest(string Reason) : CancelSupplierReturnRequest(Reason);
