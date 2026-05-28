using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public static class StaffarrPublicationRules
{
    public const int DefaultMaxAttempts = 10;

    public const int DefaultRetryIntervalMinutes = 5;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 25, 1, 200);

    public static int NormalizeMaxAttempts(int? maxAttempts) =>
        Math.Clamp(maxAttempts ?? DefaultMaxAttempts, 1, 50);

    public static int NormalizeRetryIntervalMinutes(int? retryIntervalMinutes) =>
        Math.Clamp(retryIntervalMinutes ?? DefaultRetryIntervalMinutes, 1, 24 * 60);

    public static int NormalizeDeliveryListLimit(int? limit) =>
        limit is null or < 1 ? 20 : Math.Min(limit.Value, 100);

    public static bool ShouldRetryForTenant(TenantStaffarrPublicationSettingsSnapshot? settings) =>
        settings?.IsEnabled != false;

    public static DateTimeOffset ComputeNextRetryAt(DateTimeOffset now, int retryIntervalMinutes) =>
        now.AddMinutes(NormalizeRetryIntervalMinutes(retryIntervalMinutes));
}

public sealed record TenantStaffarrPublicationSettingsSnapshot(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes);
