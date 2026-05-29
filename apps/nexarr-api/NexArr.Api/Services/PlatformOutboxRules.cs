namespace NexArr.Api.Services;

public static class PlatformOutboxRules
{
    public const int DefaultMaxRetryAttempts = 5;
    public const int DefaultRetryIntervalMinutes = 5;
    public const int DefaultSchemaVersion = 1;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeMaxRetryAttempts(int? maxRetryAttempts) =>
        Math.Clamp(maxRetryAttempts ?? DefaultMaxRetryAttempts, 1, 20);

    public static int NormalizeRetryIntervalMinutes(int? retryIntervalMinutes) =>
        Math.Clamp(retryIntervalMinutes ?? DefaultRetryIntervalMinutes, 1, 24 * 60);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static int NormalizeEventListLimit(int? limit) =>
        Math.Clamp(limit ?? 25, 1, 200);

    public static string BuildIdempotencyKey(string eventType, string targetType, string targetId, string changeToken) =>
        $"{eventType}:{targetType}:{targetId}:{changeToken}";

    public static bool IsReadyForProcessing(DateTimeOffset? nextRetryAt, DateTimeOffset asOfUtc) =>
        nextRetryAt is null || nextRetryAt <= asOfUtc;

    public static DateTimeOffset ComputeNextRetryAt(int attemptCount, int retryIntervalMinutes, DateTimeOffset asOfUtc) =>
        asOfUtc.AddMinutes(retryIntervalMinutes * Math.Max(1, attemptCount));

    public static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
