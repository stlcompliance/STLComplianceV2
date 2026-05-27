using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class PurchaseRequestStatusRules
{
    public static bool CanTransition(string fromStatus, string toStatus)
    {
        var from = fromStatus.Trim().ToLowerInvariant();
        var to = toStatus.Trim().ToLowerInvariant();
        return from switch
        {
            PurchaseRequestStatuses.Draft => to is PurchaseRequestStatuses.Submitted
                or PurchaseRequestStatuses.Cancelled,
            PurchaseRequestStatuses.Submitted => to is PurchaseRequestStatuses.Approved
                or PurchaseRequestStatuses.Rejected
                or PurchaseRequestStatuses.Cancelled,
            _ => false
        };
    }
}
