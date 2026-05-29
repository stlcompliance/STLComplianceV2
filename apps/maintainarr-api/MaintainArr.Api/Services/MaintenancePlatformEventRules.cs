namespace MaintainArr.Api.Services;

public static class MaintenancePlatformEventRules
{
    public const int DefaultBatchSize = 50;
    public const int DefaultMaxAttempts = 5;
    public const int DefaultRetryIntervalMinutes = 15;
    public const int DefaultEventListLimit = 25;
    public const int DefaultRunListLimit = 10;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? DefaultBatchSize, 1, 200);

    public static int NormalizeMaxAttempts(int? maxAttempts) =>
        Math.Clamp(maxAttempts ?? DefaultMaxAttempts, 1, 20);

    public static int NormalizeRetryIntervalMinutes(int? retryIntervalMinutes) =>
        Math.Clamp(retryIntervalMinutes ?? DefaultRetryIntervalMinutes, 1, 24 * 60);

    public static int NormalizeEventListLimit(int? limit) =>
        Math.Clamp(limit ?? DefaultEventListLimit, 1, 200);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? DefaultRunListLimit, 1, 100);

    public static bool ShouldEmitForTenant(TenantMaintenancePlatformEventSettingsSnapshot? settings) =>
        settings?.IsEnabled != false;

    public static DateTimeOffset ComputeNextRetryAt(DateTimeOffset now, int retryIntervalMinutes) =>
        now.AddMinutes(NormalizeRetryIntervalMinutes(retryIntervalMinutes));

    public static string BuildReadinessChangedIdempotencyKey(
        Guid assetId,
        string previousReadinessStatus,
        string readinessStatus,
        string previousLifecycleStatus,
        string lifecycleStatus) =>
        $"asset.readiness_changed:asset:{assetId:D}:{previousReadinessStatus}>{readinessStatus}:{previousLifecycleStatus}>{lifecycleStatus}"
            .ToLowerInvariant();

    public static string BuildLifecycleTransitionIdempotencyKey(
        string eventKind,
        Guid assetId,
        string previousLifecycleStatus,
        string lifecycleStatus) =>
        $"{eventKind}:asset:{assetId:D}:{previousLifecycleStatus}>{lifecycleStatus}".ToLowerInvariant();

    public static bool HasReadinessTransition(
        string? previousReadinessStatus,
        string readinessStatus,
        string? previousLifecycleStatus,
        string lifecycleStatus)
    {
        if (previousReadinessStatus is null && previousLifecycleStatus is null)
        {
            return true;
        }

        return !string.Equals(previousReadinessStatus, readinessStatus, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(previousLifecycleStatus, lifecycleStatus, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsOutOfServiceTransition(string? previousLifecycleStatus, string lifecycleStatus) =>
        !string.Equals(previousLifecycleStatus, "out_of_service", StringComparison.OrdinalIgnoreCase)
        && string.Equals(lifecycleStatus, "out_of_service", StringComparison.OrdinalIgnoreCase);

    public static bool IsReturnedToServiceTransition(string? previousLifecycleStatus, string lifecycleStatus) =>
        string.Equals(previousLifecycleStatus, "out_of_service", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(lifecycleStatus, "out_of_service", StringComparison.OrdinalIgnoreCase);
}

public sealed record TenantMaintenancePlatformEventSettingsSnapshot(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes);
