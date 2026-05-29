using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public static class FactSourceSyncStatuses
{
    public const string Healthy = "healthy";
    public const string Stale = "stale";
    public const string Failed = "failed";
    public const string Pending = "pending";
    public const string Skipped = "skipped";
}

public sealed class TenantFactSourceSyncWorkerSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public string DefaultScopeKey { get; set; } = "tenant";

    public int IntervalMinutes { get; set; } = 60;

    public DateTimeOffset? LastBatchRunAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class FactSourceSyncStatus
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid FactSourceId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public string HealthStatus { get; set; } = FactSourceSyncStatuses.Pending;

    public DateTimeOffset? LastAttemptAt { get; set; }

    public DateTimeOffset? LastSuccessAt { get; set; }

    public DateTimeOffset? LastFailureAt { get; set; }

    public string? LastErrorMessage { get; set; }

    public int ConsecutiveFailureCount { get; set; }

    public Guid? LastMirrorId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public FactSource? FactSource { get; set; }
}
