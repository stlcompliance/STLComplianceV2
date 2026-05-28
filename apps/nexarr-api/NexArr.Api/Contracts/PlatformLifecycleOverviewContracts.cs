namespace NexArr.Api.Contracts;

public sealed record PlatformLifecycleLatestRunSummary(
    Guid RunId,
    string Outcome,
    DateTimeOffset ProcessedAt,
    int PrimaryCount,
    string PrimaryCountLabel);

public sealed record PlatformLifecycleWorkerStatus(
    string WorkerKey,
    string Label,
    string Description,
    bool IsEnabled,
    int PendingCount,
    PlatformLifecycleLatestRunSummary? LatestRun,
    string ServiceTokenScope,
    string PlatformSettingsPath,
    string SuiteAdminPath);

public sealed record PlatformLifecycleOverviewResponse(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<PlatformLifecycleWorkerStatus> Workers);
