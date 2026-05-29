namespace NexArr.Api.Contracts;

public sealed record PlatformServiceTokenInventorySummary(
    int ActiveCount,
    int ExpiringWithin24HoursCount,
    int ExpiredRetainedCount,
    int RevokedRetainedCount,
    int PendingCleanupCount);

public sealed record PlatformWorkerOrchestrationWorkerStatus(
    string WorkerKey,
    string Label,
    string Description,
    bool IsEnabled,
    int PendingCount,
    PlatformLifecycleLatestRunSummary? LatestRun,
    string ServiceTokenScope,
    string SuiteAdminPath);

public sealed record PlatformWorkerHealthOrchestrationStatusResponse(
    DateTimeOffset GeneratedAt,
    string PlatformHealthStatus,
    IReadOnlyList<ProductHealthProbeResult> ProductHealth,
    PlatformServiceTokenInventorySummary ServiceTokens,
    int ActiveServiceClientCount,
    IReadOnlyList<PlatformWorkerOrchestrationWorkerStatus> Workers);

public sealed record TriggerServiceTokenCleanupOrchestrationResponse(
    DateTimeOffset AsOfUtc,
    int PurgedCount,
    int SkippedCount);

public sealed record TriggerEntitlementReconciliationOrchestrationResponse(
    DateTimeOffset AsOfUtc,
    int GrantedCount,
    int RevokedCount,
    int SkippedCount);

public sealed record TriggerTenantLifecycleOrchestrationResponse(
    DateTimeOffset AsOfUtc,
    int SuspendedCount,
    int ReactivatedCount,
    int SkippedCount);
