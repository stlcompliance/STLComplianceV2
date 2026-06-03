namespace SupplyArr.Api.Contracts;

public sealed record VendorApprovalStatusSummaryResponse(string ApprovalStatus, int Count);

public sealed record VendorReportSummaryItemResponse(
    Guid VendorPartyId,
    string PartyKey,
    string DisplayName,
    string ApprovalStatus,
    string Status,
    int PartVendorLinkCount,
    int PreferredPartLinkCount,
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

public sealed record VendorReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<VendorApprovalStatusSummaryResponse> ApprovalStatusCounts,
    IReadOnlyList<VendorReportSummaryItemResponse> Vendors);

public sealed record VendorReportPurchaseRequestRowResponse(
    Guid PurchaseRequestId,
    string RequestKey,
    string Title,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record VendorReportPurchaseOrderRowResponse(
    Guid PurchaseOrderId,
    string OrderKey,
    string Title,
    string Status,
    int LineCount,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    DateTimeOffset UpdatedAt);

public sealed record VendorReportPartLinkRowResponse(
    Guid PartVendorLinkId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    string VendorPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogAvailabilityStatus);

public sealed record VendorReportDetailResponse(
    VendorReportSummaryItemResponse Summary,
    IReadOnlyList<VendorReportPurchaseRequestRowResponse> RecentPurchaseRequests,
    IReadOnlyList<VendorReportPurchaseOrderRowResponse> RecentPurchaseOrders,
    IReadOnlyList<VendorReportPartLinkRowResponse> PartLinks);
