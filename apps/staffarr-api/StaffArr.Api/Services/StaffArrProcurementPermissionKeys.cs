namespace StaffArr.Api.Services;

public static class StaffArrProcurementPermissionKeys
{
    public const string PurchaseRequestsSubmit = "supplyarr.procurement.purchase_requests.submit";

    public const string PurchaseRequestsApprove = "supplyarr.procurement.purchase_requests.approve";

    public const string PurchaseOrdersIssue = "supplyarr.procurement.purchase_orders.issue";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        PurchaseRequestsSubmit,
        PurchaseRequestsApprove,
        PurchaseOrdersIssue,
    };
}

public static class StaffArrProcurementScopeTypes
{
    public const string Tenant = "tenant";

    public const string OrgUnit = "org_unit";

    public const string MonetaryLimit = "monetary_limit";
}
