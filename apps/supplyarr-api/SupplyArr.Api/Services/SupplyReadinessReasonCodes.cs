namespace SupplyArr.Api.Services;

public static class SupplyReadinessReasonCodes
{
    public const string PartInactive = "part_inactive";

    public const string PartStockout = "part_stockout";

    public const string PartBelowReorder = "part_below_reorder";

    public const string InsufficientAvailableQuantity = "insufficient_available_quantity";

    public const string OpenBackorder = "open_backorder";

    public const string NoSupplierPartLink = "no_supplier_part_link";

    public const string SupplierInactive = "supplier_inactive";

    public const string SupplierApprovalRestricted = "supplier_approval_restricted";

    public const string SupplierApprovalBlocked = "supplier_approval_blocked";

    public const string SupplierApprovalPending = "supplier_approval_pending";

    public const string SupplierProcurementRestriction = "supplier_procurement_restriction";

    public const string SupplierMissingPartsCoverage = "supplier_missing_parts_coverage";

    public const string ComplianceDocumentExpired = "compliance_document_expired";

    public const string ComplianceDocumentPending = "compliance_document_pending";

    public const string OpenSupplierIncident = "open_supplier_incident";
}
