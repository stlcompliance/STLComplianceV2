namespace SupplyArr.Api.Contracts;

public sealed record PurchasingStatusCountResponse(string Status, int Count);

public sealed record PurchasingReportTotalsResponse(
    int PurchaseRequestCount,
    int OpenPurchaseRequestCount,
    int PurchaseOrderCount,
    int OpenPurchaseOrderCount,
    int IssuedPurchaseOrderCount,
    int DraftReceivingReceiptCount,
    int PostedReceivingReceiptCount,
    int OpenBackorderCount,
    decimal OpenPurchaseOrderLineQuantity,
    decimal PurchaseOrderQuantityReceived);

public sealed record PurchasingProcurementAnalyticsResponse(
    int PendingPurchaseRequestCount,
    int EmergencyPurchaseRequestCount,
    int ActiveProcurementExceptionCount,
    int OpenReceivingExceptionCount,
    int OpenWarrantyClaimCount,
    int SupplierDocumentExpiringSoonCount,
    int BlockedSupplierCount,
    int? AverageLeadTimeDays,
    decimal EstimatedSpendThisMonth);

public sealed record PurchasingDocumentSummaryItemResponse(
    string DocumentType,
    Guid DocumentId,
    string DocumentKey,
    string Title,
    string Status,
    Guid? SupplierId,
    string? SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string? SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string? SupplierType,
    int LineCount,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    DateTimeOffset UpdatedAt);

public sealed record PurchasingReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    PurchasingReportTotalsResponse Totals,
    PurchasingProcurementAnalyticsResponse Analytics,
    IReadOnlyList<PurchasingStatusCountResponse> PurchaseRequestStatusCounts,
    IReadOnlyList<PurchasingStatusCountResponse> PurchaseOrderStatusCounts,
    IReadOnlyList<PurchasingDocumentSummaryItemResponse> Documents);

public sealed record PurchasingRequestLineRowResponse(
    Guid LineId,
    int LineNumber,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    decimal QuantityRequested,
    string UnitOfMeasure);

public sealed record PurchasingPurchaseRequestDetailResponse(
    PurchasingDocumentSummaryItemResponse Summary,
    IReadOnlyList<PurchasingRequestLineRowResponse> Lines,
    Guid? LinkedPurchaseOrderId,
    string? LinkedPurchaseOrderKey);

public sealed record PurchasingOrderLineRowResponse(
    Guid LineId,
    int LineNumber,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    decimal QuantityRemaining);

public sealed record PurchasingReceivingRowResponse(
    Guid ReceivingReceiptId,
    string ReceiptKey,
    string Status,
    DateTimeOffset? PostedAt);

public sealed record PurchasingBackorderRowResponse(
    Guid BackorderId,
    string BackorderKey,
    string Status,
    decimal QuantityBackordered,
    decimal QuantityFulfilled);

public sealed record PurchasingPurchaseOrderDetailResponse(
    PurchasingDocumentSummaryItemResponse Summary,
    IReadOnlyList<PurchasingOrderLineRowResponse> Lines,
    IReadOnlyList<PurchasingReceivingRowResponse> ReceivingReceipts,
    IReadOnlyList<PurchasingBackorderRowResponse> Backorders);
