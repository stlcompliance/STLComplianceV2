using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public static class StaffArrWorkerKeys
{
    public const string CertificationExpiration = "certification-expiration";
    public const string ReadinessRollup = "readiness-rollup";
    public const string PermissionProjection = "permission-projection";
    public const string PersonnelHistoryRollup = "personnel-history-rollup";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        CertificationExpiration,
        ReadinessRollup,
        PermissionProjection,
        PersonnelHistoryRollup,
    };
}

public sealed class TenantStaffArrWorkerSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string WorkerKey { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public int ScanIntervalMinutes { get; set; } = StaffArrWorkerSettingsDefaults.ScanIntervalMinutes;

    public int BatchSize { get; set; } = StaffArrWorkerSettingsDefaults.BatchSize;

    public int? StalenessHours { get; set; }

    public DateTimeOffset? LastRunAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class StaffArrWorkerRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string WorkerKey { get; set; } = string.Empty;

    public string Status { get; set; } = "success";

    public int CandidatesFound { get; set; }

    public int ProcessedCount { get; set; }

    public int SkippedCount { get; set; }

    public string? Summary { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class StaffArrWorkerSettingsDefaults
{
    public const int ScanIntervalMinutes = 30;
    public const int BatchSize = 50;
    public const int StalenessHours = 1;
}
