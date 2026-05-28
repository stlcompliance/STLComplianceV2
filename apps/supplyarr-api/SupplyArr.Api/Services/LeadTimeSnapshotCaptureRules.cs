using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class LeadTimeSnapshotCaptureRules
{
    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? LeadTimeSnapshotWorkerDefaults.StalenessHours, 1, 168);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static bool IsStale(DateTimeOffset? lastCapturedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (lastCapturedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return lastCapturedAt < threshold;
    }

    public static bool NeedsCapture(int? catalogLeadTimeDays, int? currentLeadTimeDays)
    {
        if (catalogLeadTimeDays is null || catalogLeadTimeDays < 0)
        {
            return false;
        }

        if (currentLeadTimeDays is null)
        {
            return true;
        }

        return catalogLeadTimeDays.Value != currentLeadTimeDays.Value;
    }

    public static string BuildWorkerSnapshotKey(Guid partVendorLinkId, DateTimeOffset effectiveFrom) =>
        $"worker-lt-{partVendorLinkId:N}-{effectiveFrom:yyyyMMddHHmmss}";

    public static int NormalizeLeadTimeDays(int leadTimeDays)
    {
        if (leadTimeDays < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leadTimeDays), "Lead time days cannot be negative.");
        }

        return leadTimeDays;
    }
}
