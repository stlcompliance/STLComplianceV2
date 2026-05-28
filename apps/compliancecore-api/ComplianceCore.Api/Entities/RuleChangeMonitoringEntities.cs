using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class RuleChangeEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RulePackId { get; set; }

    public string PackKey { get; set; } = string.Empty;

    public string ProgramKey { get; set; } = string.Empty;

    public string ChangeType { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string? FromStatus { get; set; }

    public string? ToStatus { get; set; }

    public int? FromVersion { get; set; }

    public int? ToVersion { get; set; }

    public string? PreviousContentHash { get; set; }

    public string? NewContentHash { get; set; }

    public string Source { get; set; } = RuleChangeSources.Api;

    public Guid? ActorUserId { get; set; }

    public Guid? ScanRunId { get; set; }

    public DateTimeOffset DetectedAt { get; set; }

    public RuleChangeScanRun? ScanRun { get; set; }
}

public sealed class RulePackMonitorSnapshot : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RulePackId { get; set; }

    public string PackKey { get; set; } = string.Empty;

    public int VersionNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? ContentHash { get; set; }

    public DateTimeOffset CapturedAt { get; set; }
}

public sealed class RuleChangeScanRun
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string Status { get; set; } = RuleChangeScanRunStatuses.InProgress;

    public int PacksScannedCount { get; set; }

    public int ChangesDetectedCount { get; set; }

    public string? ErrorMessage { get; set; }
}

public static class RuleChangeTypes
{
    public const string VersionCreated = "version_created";

    public const string StatusChanged = "status_changed";

    public const string ContentUpdated = "content_updated";

    public const string ScanDetected = "scan_detected";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        VersionCreated,
        StatusChanged,
        ContentUpdated,
        ScanDetected,
    };
}

public static class RuleChangeSources
{
    public const string Api = "api";

    public const string Worker = "worker";
}

public static class RuleChangeScanRunStatuses
{
    public const string InProgress = "in_progress";

    public const string Completed = "completed";

    public const string Failed = "failed";
}
