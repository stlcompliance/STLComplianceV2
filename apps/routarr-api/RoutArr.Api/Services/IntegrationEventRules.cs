namespace RoutArr.Api.Services;

public static class IntegrationEventRules
{
    public const int DefaultBatchSize = 50;
    public const int MaxBatchSize = 200;
    public const int DefaultMaxAttempts = 5;
    public const int DefaultRetryIntervalMinutes = 15;
    public const int DefaultEventListLimit = 50;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? DefaultBatchSize, 1, MaxBatchSize);

    public static int NormalizeMaxAttempts(int? maxAttempts) =>
        Math.Clamp(maxAttempts ?? DefaultMaxAttempts, 1, 20);

    public static int NormalizeRetryIntervalMinutes(int? retryIntervalMinutes) =>
        Math.Clamp(retryIntervalMinutes ?? DefaultRetryIntervalMinutes, 1, 24 * 60);

    public static int NormalizeEventListLimit(int? limit) =>
        Math.Clamp(limit ?? DefaultEventListLimit, 1, 200);

    public static bool ShouldProcessForTenant(TenantIntegrationEventSettingsSnapshot? settings) =>
        settings?.IsEnabled != false;

    public static string BuildOutboxIdempotencyKey(
        string eventKind,
        string relatedEntityType,
        Guid relatedEntityId,
        string? suffix = null) =>
        suffix is null
            ? $"outbox:{eventKind}:{relatedEntityType}:{relatedEntityId:D}".ToLowerInvariant()
            : $"outbox:{eventKind}:{relatedEntityType}:{relatedEntityId:D}:{suffix}".ToLowerInvariant();

    public static DateTimeOffset ComputeNextRetryAt(DateTimeOffset now, int retryIntervalMinutes) =>
        now.AddMinutes(NormalizeRetryIntervalMinutes(retryIntervalMinutes));
}

public sealed record TenantIntegrationEventSettingsSnapshot(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes);
