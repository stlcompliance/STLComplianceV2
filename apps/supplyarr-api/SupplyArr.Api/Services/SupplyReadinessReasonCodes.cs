namespace SupplyArr.Api.Services;

public static class SupplyReadinessReasonCodes
{
    public const string PartInactive = "part_inactive";

    public const string PartStockout = "part_stockout";

    public const string PartBelowReorder = "part_below_reorder";

    public const string InsufficientAvailableQuantity = "insufficient_available_quantity";

    public const string OpenBackorder = "open_backorder";

    public const string NoVendorPartLink = "no_vendor_part_link";

    public const string VendorInactive = "vendor_inactive";

    public const string VendorApprovalRestricted = "vendor_approval_restricted";

    public const string VendorApprovalBlocked = "vendor_approval_blocked";

    public const string VendorApprovalPending = "vendor_approval_pending";

    public const string VendorProcurementRestriction = "vendor_procurement_restriction";

    public const string ComplianceDocumentExpired = "compliance_document_expired";

    public const string ComplianceDocumentPending = "compliance_document_pending";

    public const string OpenSupplierIncident = "open_supplier_incident";
}
