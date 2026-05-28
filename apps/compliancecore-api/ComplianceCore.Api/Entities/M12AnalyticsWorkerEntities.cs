using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public static class M12AnalyticsBatchRunStatuses
{
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Failed = "failed";
}

public sealed class TenantM12AnalyticsWorkerSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public string DefaultScopeKey { get; set; } = "tenant";

    public int IntervalHours { get; set; } = 24;

    public bool RiskScoringEnabled { get; set; } = true;

    public bool MissingEvidenceEnabled { get; set; } = true;

    public bool ControlEffectivenessEnabled { get; set; } = true;

    public bool ReadinessForecastEnabled { get; set; } = true;

    public bool AuditDeliveryEnabled { get; set; }

    public DateTimeOffset? LastBatchRunAt { get; set; }

    public DateTimeOffset? LastRiskScoringRunAt { get; set; }

    public DateTimeOffset? LastMissingEvidenceRunAt { get; set; }

    public DateTimeOffset? LastControlEffectivenessRunAt { get; set; }

    public DateTimeOffset? LastReadinessForecastRunAt { get; set; }

    public DateTimeOffset? LastAuditDeliveryRunAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class M12AnalyticsBatchRun
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string Status { get; set; } = M12AnalyticsBatchRunStatuses.InProgress;

    public int IntervalHours { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public bool RiskScoringRan { get; set; }

    public bool MissingEvidenceRan { get; set; }

    public bool ControlEffectivenessRan { get; set; }

    public bool ReadinessForecastRan { get; set; }

    public bool AuditDeliveryQueued { get; set; }

    public Guid? RiskScoreRunId { get; set; }

    public Guid? MissingEvidenceWarningRunId { get; set; }

    public Guid? ControlEffectivenessRunId { get; set; }

    public Guid? ReadinessForecastRunId { get; set; }

    public Guid? AuditPackageJobId { get; set; }

    public string? ErrorMessage { get; set; }
}
