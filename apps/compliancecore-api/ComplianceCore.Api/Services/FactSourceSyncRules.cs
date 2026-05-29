namespace ComplianceCore.Api.Services;

public static class FactSourceSyncRules
{
    public const int DefaultIntervalMinutes = 60;

    public const int MinIntervalMinutes = 5;

    public const int MaxIntervalMinutes = 1440;

    public const int DefaultBatchSize = 50;

    public const int MaxBatchSize = 200;

    public const string SyncIdempotencyPrefix = "fact_source_sync:";

    public const string SyncSourceEntityType = "fact_source_sync";

    public const string SyncSourceEventKind = "background_sync";

    public static int NormalizeIntervalMinutes(int? intervalMinutes) =>
        Math.Clamp(intervalMinutes ?? DefaultIntervalMinutes, MinIntervalMinutes, MaxIntervalMinutes);

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? DefaultBatchSize, 1, MaxBatchSize);

    public static string NormalizeScopeKey(string? scopeKey) =>
        string.IsNullOrWhiteSpace(scopeKey) ? "tenant" : scopeKey.Trim().ToLowerInvariant();

    public static bool IsDue(DateTimeOffset? lastAttemptAt, int intervalMinutes, DateTimeOffset asOf)
    {
        if (lastAttemptAt is null)
        {
            return true;
        }

        return lastAttemptAt.Value.AddMinutes(intervalMinutes) <= asOf;
    }

    public static string BuildIdempotencyKey(Guid factSourceId) =>
        $"{SyncIdempotencyPrefix}{factSourceId:N}";

    public static string ResolveHealthStatus(
        DateTimeOffset? lastSuccessAt,
        DateTimeOffset? lastFailureAt,
        int intervalMinutes,
        DateTimeOffset asOf)
    {
        if (lastSuccessAt is null && lastFailureAt is null)
        {
            return Entities.FactSourceSyncStatuses.Pending;
        }

        if (lastFailureAt is not null
            && (lastSuccessAt is null || lastFailureAt > lastSuccessAt))
        {
            return Entities.FactSourceSyncStatuses.Failed;
        }

        if (lastSuccessAt is null)
        {
            return Entities.FactSourceSyncStatuses.Pending;
        }

        return lastSuccessAt.Value.AddMinutes(intervalMinutes) >= asOf
            ? Entities.FactSourceSyncStatuses.Healthy
            : Entities.FactSourceSyncStatuses.Stale;
    }
}
