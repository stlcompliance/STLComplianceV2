namespace StaffArr.Api.Services;

public static class PersonExportDeliveryRules
{
    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 10, 1, 100);

    public static int NormalizeIntervalHours(int? intervalHours) =>
        Math.Clamp(intervalHours ?? 24, 1, 720);

    public static bool IsDue(DateTimeOffset? lastDeliveredAt, DateTimeOffset asOfUtc, int intervalHours)
    {
        if (lastDeliveredAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-intervalHours);
        return lastDeliveredAt < threshold;
    }

    public static string TruncateSkipReason(string reason) =>
        reason.Length <= 256 ? reason : reason[..256];
}
