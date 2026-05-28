using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class ProcurementCoordinationRules
{
    public const int DefaultReadStalenessHours = ProcurementCoordinationDefaults.StalenessHours;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? ProcurementCoordinationDefaults.StalenessHours, 1, 168);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsStale(DateTimeOffset? computedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (computedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return computedAt < threshold;
    }

    public static bool IsPending(
        DateTimeOffset sourceUpdatedAt,
        DateTimeOffset? computedAt,
        DateTimeOffset asOfUtc,
        int stalenessHours)
    {
        if (computedAt is null || sourceUpdatedAt > computedAt)
        {
            return true;
        }

        return IsStale(computedAt, asOfUtc, stalenessHours);
    }

    public static int? ComputeReceiptProgressPercent(decimal quantityOrdered, decimal quantityReceived)
    {
        if (quantityOrdered <= 0)
        {
            return quantityReceived > 0 ? 100 : 0;
        }

        var percent = (int)Math.Round(quantityReceived / quantityOrdered * 100m, MidpointRounding.AwayFromZero);
        return Math.Clamp(percent, 0, 100);
    }

    public static bool IsTerminalStage(string coordinationStage) =>
        string.Equals(coordinationStage, ProcurementCoordinationStages.Fulfilled, StringComparison.OrdinalIgnoreCase)
        || string.Equals(coordinationStage, ProcurementCoordinationStages.Cancelled, StringComparison.OrdinalIgnoreCase)
        || string.Equals(coordinationStage, ProcurementCoordinationStages.Rejected, StringComparison.OrdinalIgnoreCase);

    public static bool IsActivePurchaseRequestStatus(string status) =>
        string.Equals(status, PurchaseRequestStatuses.Submitted, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, PurchaseRequestStatuses.Approved, StringComparison.OrdinalIgnoreCase);

    public static bool IsActivePurchaseOrderStatus(string status) =>
        PurchaseOrderStatuses.Open.Contains(status)
        || string.Equals(status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase);
}
