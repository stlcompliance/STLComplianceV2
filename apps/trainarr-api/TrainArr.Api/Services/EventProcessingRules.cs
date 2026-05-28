namespace TrainArr.Api.Services;

public static class EventProcessingRules
{
    public const int DefaultMaxAttempts = 10;

    public const int DefaultRetryIntervalMinutes = 5;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 25, 1, 200);

    public static int NormalizeMaxAttempts(int? maxAttempts) =>
        Math.Clamp(maxAttempts ?? DefaultMaxAttempts, 1, 50);

    public static int NormalizeRetryIntervalMinutes(int? retryIntervalMinutes) =>
        Math.Clamp(retryIntervalMinutes ?? DefaultRetryIntervalMinutes, 1, 24 * 60);

    public static int NormalizeHistoryListLimit(int? limit) =>
        limit is null or < 1 ? 50 : Math.Min(limit.Value, 200);

    public static int NormalizeEventListLimit(int? limit) =>
        limit is null or < 1 ? 20 : Math.Min(limit.Value, 100);

    public static bool ShouldProcessForTenant(TenantEventProcessingSettingsSnapshot? settings) =>
        settings?.IsEnabled != false;

    public static DateTimeOffset ComputeNextRetryAt(DateTimeOffset now, int retryIntervalMinutes) =>
        now.AddMinutes(NormalizeRetryIntervalMinutes(retryIntervalMinutes));

    public static string BuildIdempotencyKey(string eventKind, string relatedEntityType, Guid relatedEntityId) =>
        $"{eventKind}:{relatedEntityType}:{relatedEntityId:D}".ToLowerInvariant();
}

public sealed record TenantEventProcessingSettingsSnapshot(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes);
