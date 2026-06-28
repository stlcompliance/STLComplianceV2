namespace NexArr.Api.Services;

public static class TenantLifecycleRules
{
    public const int DefaultBatchSize = 25;
    public const int DefaultSuspendGraceDays = 7;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? DefaultBatchSize, 1, 100);

    public static int NormalizeSuspendGraceDays(int? graceDays) =>
        Math.Clamp(graceDays ?? DefaultSuspendGraceDays, 0, 365);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);
}
