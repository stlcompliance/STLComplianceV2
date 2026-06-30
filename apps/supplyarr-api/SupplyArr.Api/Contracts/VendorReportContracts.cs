namespace SupplyArr.Api.Contracts;

public record SupplierApprovalStatusSummaryResponse(string ApprovalStatus, int Count);

public sealed record VendorApprovalStatusSummaryResponse(string ApprovalStatus, int Count)
    : SupplierApprovalStatusSummaryResponse(ApprovalStatus, Count);

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
    DateTimeOffset? LastReceivingPostedAt)
{
    public Guid VendorPartyId => SupplierId;

    public string PartyKey => SupplierKey;

    public string DisplayName => SupplierDisplayName;
}

public sealed record VendorReportSummaryItemResponse(
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
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
    DateTimeOffset? LastReceivingPostedAt)
    : SupplierReportSummaryItemResponse(
        SupplierId,
        SupplierKey,
        SupplierDisplayName,
        ParentSupplierId,
        ParentSupplierDisplayName,
        SupplierUnitKind,
        SupplierServiceTypes,
        ApprovalStatus,
        Status,
        PartVendorLinkCount,
        PreferredPartLinkCount,
        OpenPurchaseRequestCount,
        OpenPurchaseOrderCount,
        IssuedPurchaseOrderCount,
        PostedReceivingReceiptCount,
        OpenBackorderCount,
        OpenPurchaseOrderLineQuantity,
        AverageLeadTimeDays,
        LeadTimeSampleCount,
        OnTimeDeliveryRate,
        OnTimeDeliverySampleCount,
        LastPurchaseOrderAt,
        LastReceivingPostedAt);

public sealed record SupplierReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<SupplierApprovalStatusSummaryResponse> ApprovalStatusCounts,
    IReadOnlyList<SupplierReportSummaryItemResponse> Suppliers);

public sealed record VendorReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<VendorApprovalStatusSummaryResponse> ApprovalStatusCounts,
    IReadOnlyList<VendorReportSummaryItemResponse> Vendors);

public record SupplierReportPurchaseRequestRowResponse(
    Guid PurchaseRequestId,
    string RequestKey,
    string Title,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record VendorReportPurchaseRequestRowResponse(
    Guid PurchaseRequestId,
    string RequestKey,
    string Title,
    string Status,
    DateTimeOffset UpdatedAt)
    : SupplierReportPurchaseRequestRowResponse(
        PurchaseRequestId,
        RequestKey,
        Title,
        Status,
        UpdatedAt);

public record SupplierReportPurchaseOrderRowResponse(
    Guid PurchaseOrderId,
    string OrderKey,
    string Title,
    string Status,
    int LineCount,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    DateTimeOffset UpdatedAt);

public sealed record VendorReportPurchaseOrderRowResponse(
    Guid PurchaseOrderId,
    string OrderKey,
    string Title,
    string Status,
    int LineCount,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    DateTimeOffset UpdatedAt)
    : SupplierReportPurchaseOrderRowResponse(
        PurchaseOrderId,
        OrderKey,
        Title,
        Status,
        LineCount,
        QuantityOrdered,
        QuantityReceived,
        UpdatedAt);

public record SupplierReportPartLinkRowResponse(
    Guid PartVendorLinkId,
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
    string VendorPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogAvailabilityStatus)
{
    public Guid VendorPartyId => SupplierId;

    public string PartyKey => SupplierKey;

    public string DisplayName => SupplierDisplayName;
}

public sealed record VendorReportPartLinkRowResponse(
    Guid PartVendorLinkId,
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
    string VendorPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogAvailabilityStatus)
    : SupplierReportPartLinkRowResponse(
        PartVendorLinkId,
        SupplierId,
        SupplierKey,
        SupplierDisplayName,
        ParentSupplierId,
        ParentSupplierDisplayName,
        SupplierUnitKind,
        SupplierServiceTypes,
        PartId,
        PartKey,
        PartDisplayName,
        VendorPartNumber,
        IsPreferred,
        CatalogUnitPrice,
        CatalogAvailabilityStatus);

public sealed record SupplierReportDetailResponse(
    SupplierReportSummaryItemResponse Summary,
    IReadOnlyList<SupplierReportPurchaseRequestRowResponse> RecentPurchaseRequests,
    IReadOnlyList<SupplierReportPurchaseOrderRowResponse> RecentPurchaseOrders,
    IReadOnlyList<SupplierReportPartLinkRowResponse> PartLinks);

public sealed record VendorReportDetailResponse(
    VendorReportSummaryItemResponse Summary,
    IReadOnlyList<VendorReportPurchaseRequestRowResponse> RecentPurchaseRequests,
    IReadOnlyList<VendorReportPurchaseOrderRowResponse> RecentPurchaseOrders,
    IReadOnlyList<VendorReportPartLinkRowResponse> PartLinks);
