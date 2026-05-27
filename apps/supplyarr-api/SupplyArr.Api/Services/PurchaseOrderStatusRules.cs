using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class PurchaseOrderStatusRules
{
    public static bool CanTransition(string fromStatus, string toStatus)
    {
        var from = fromStatus.Trim().ToLowerInvariant();
        var to = toStatus.Trim().ToLowerInvariant();
        return from switch
        {
            PurchaseOrderStatuses.Draft => to is PurchaseOrderStatuses.Approved
                or PurchaseOrderStatuses.Cancelled,
            PurchaseOrderStatuses.Approved => to is PurchaseOrderStatuses.Issued
                or PurchaseOrderStatuses.Cancelled,
            _ => false
        };
    }
}
