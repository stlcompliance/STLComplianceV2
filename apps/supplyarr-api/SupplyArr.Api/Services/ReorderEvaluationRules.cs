namespace SupplyArr.Api.Services;

public static class ReorderEvaluationRules
{
    public static bool HasReorderPolicy(decimal? reorderPoint) =>
        reorderPoint is >= 0;

    public static bool NeedsReorder(decimal availableQuantity, decimal reorderPoint) =>
        availableQuantity <= reorderPoint;

    public static decimal ResolveSuggestedQuantity(
        decimal availableQuantity,
        decimal reorderPoint,
        decimal? reorderQuantity)
    {
        if (reorderQuantity is > 0)
        {
            return reorderQuantity.Value;
        }

        var deficit = reorderPoint - availableQuantity;
        return deficit > 0 ? deficit : Math.Max(1, reorderPoint);
    }

    public static bool IsOpenPurchaseRequestStatus(string status) =>
        string.Equals(status, Entities.PurchaseRequestStatuses.Draft, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Entities.PurchaseRequestStatuses.Submitted, StringComparison.OrdinalIgnoreCase);
}
