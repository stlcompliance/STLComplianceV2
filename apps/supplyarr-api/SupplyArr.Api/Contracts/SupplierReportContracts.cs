namespace SupplyArr.Api.Contracts;

public record SupplierApprovalStatusSummaryResponse(string ApprovalStatus, int Count);

public record SupplierReportSummaryItemResponse(
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string ApprovalStatus,
    string Status,
    int PartSupplierLinkCount,
    int PreferredPartSupplierLinkCount,
    int OpenPurchaseRequestCount,
    int OpenPurchaseOrderCount,
    int IssuedPurchaseOrderCount,
    int PostedReceivingReceiptCount,
    int OpenBackorderCount,
    decimal OpenPurchaseOrderLineQuantity,
    int? AverageLeadTimeDays,
    int LeadTimeSampleCount,
    int? OnTimeDeliveryRate,
    int OnTimeDeliverySampleCount,
    DateTimeOffset? LastPurchaseOrderAt,
    DateTimeOffset? LastReceivingPostedAt);

public sealed record SupplierReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<SupplierApprovalStatusSummaryResponse> ApprovalStatusCounts,
    IReadOnlyList<SupplierReportSummaryItemResponse> Suppliers);

public record SupplierReportPurchaseRequestRowResponse(
    Guid PurchaseRequestId,
    string RequestKey,
    string Title,
    string Status,
    DateTimeOffset UpdatedAt);

public record SupplierReportPurchaseOrderRowResponse(
    Guid PurchaseOrderId,
    string OrderKey,
    string Title,
    string Status,
    int LineCount,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    DateTimeOffset UpdatedAt);

public record SupplierReportPartLinkRowResponse(
    Guid PartSupplierLinkId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    string SupplierPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogAvailabilityStatus);

public sealed record SupplierReportDetailResponse(
    SupplierReportSummaryItemResponse Summary,
    IReadOnlyList<SupplierReportPurchaseRequestRowResponse> RecentPurchaseRequests,
    IReadOnlyList<SupplierReportPurchaseOrderRowResponse> RecentPurchaseOrders,
    IReadOnlyList<SupplierReportPartLinkRowResponse> PartLinks);
