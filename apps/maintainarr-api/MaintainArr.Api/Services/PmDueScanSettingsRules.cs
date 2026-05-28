namespace MaintainArr.Api.Services;

public static class PmDueScanSettingsRules
{
    public static int NormalizeScanIntervalMinutes(int? minutes) =>
        minutes is null or < 1 or > 24 * 60 ? Entities.PmDueScanSettingsDefaults.ScanIntervalMinutes : minutes.Value;

    public static int NormalizeBatchSize(int? batchSize) =>
        batchSize is null or < 1 or > 500 ? Entities.PmDueScanSettingsDefaults.BatchSize : batchSize.Value;

    public static int NormalizeOverdueGraceDays(int? overdueGraceDays) =>
        overdueGraceDays is null or < 0 or > 30
            ? Entities.PmDueScanSettingsDefaults.OverdueGraceDays
            : overdueGraceDays.Value;

    public static int NormalizeRunListLimit(int? limit) =>
        limit is null or < 1 or > 50 ? 5 : limit.Value;

    public static bool IsScheduledRunDue(DateTimeOffset? lastRunAt, int scanIntervalMinutes, DateTimeOffset asOfUtc)
    {
        if (lastRunAt is null)
        {
            return true;
        }

        var interval = TimeSpan.FromMinutes(NormalizeScanIntervalMinutes(scanIntervalMinutes));
        return asOfUtc >= lastRunAt.Value + interval;
    }
}
