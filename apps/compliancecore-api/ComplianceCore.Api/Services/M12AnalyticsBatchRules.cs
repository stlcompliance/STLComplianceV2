namespace ComplianceCore.Api.Services;

public static class M12AnalyticsBatchRules
{
    public const int DefaultIntervalHours = 24;

    public const int MinIntervalHours = 1;

    public const int MaxIntervalHours = 168;

    public const int DefaultBatchSize = 25;

    public const int MaxBatchSize = 100;

    public const int MaxScopeKeyLength = 256;

    public static int NormalizeIntervalHours(int? intervalHours) =>
        Math.Clamp(intervalHours ?? DefaultIntervalHours, MinIntervalHours, MaxIntervalHours);

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? DefaultBatchSize, 1, MaxBatchSize);

    public static string NormalizeScopeKey(string? scopeKey) =>
        string.IsNullOrWhiteSpace(scopeKey) ? "tenant" : scopeKey.Trim();

    public static bool IsDue(DateTimeOffset? lastRunAt, int intervalHours, DateTimeOffset asOf)
    {
        if (lastRunAt is null)
        {
            return true;
        }

        return lastRunAt.Value.AddHours(intervalHours) <= asOf;
    }
}
