namespace MaintainArr.Api.Contracts;

public sealed record DefectEscalationSettingsResponse(
    bool IsEnabled,
    int LowThresholdHours,
    int MediumThresholdHours,
    int HighThresholdHours,
    int CriticalThresholdHours,
    bool AutoAcknowledgeOnEscalation,
    bool AutoCreateWorkOrderOnEscalation,
    bool BumpSeverityOnRepeatEscalation,
    bool NotifyOnEscalation,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertDefectEscalationSettingsRequest(
    bool IsEnabled,
    int LowThresholdHours,
    int MediumThresholdHours,
    int HighThresholdHours,
    int CriticalThresholdHours,
    bool AutoAcknowledgeOnEscalation,
    bool AutoCreateWorkOrderOnEscalation,
    bool BumpSeverityOnRepeatEscalation,
    bool NotifyOnEscalation);

public sealed record PendingDefectEscalationItem(
    Guid DefectId,
    Guid TenantId,
    Guid AssetId,
    string Title,
    string Severity,
    string Status,
    int EscalationCount,
    DateTimeOffset StagnationAnchorUtc,
    int ThresholdHours,
    double StagnationHours);

public sealed record PendingDefectEscalationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingDefectEscalationItem> Items);

public sealed record DefectEscalationRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record DefectEscalationRunsResponse(
    IReadOnlyList<DefectEscalationRunItem> Items);

public sealed record DefectEscalationEventItem(
    Guid EventId,
    Guid DefectId,
    string ActionKind,
    string? PreviousSeverity,
    string? NewSeverity,
    string? PreviousStatus,
    string? NewStatus,
    Guid? WorkOrderId,
    DateTimeOffset CreatedAt);

public sealed record DefectEscalationEventsResponse(
    IReadOnlyList<DefectEscalationEventItem> Items);

public sealed record ProcessDefectEscalationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record DefectEscalationSkip(
    Guid DefectId,
    string Reason);

public sealed record DefectEscalationResult(
    Guid DefectId,
    IReadOnlyList<string> ActionsTaken);

public sealed record ProcessDefectEscalationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount,
    IReadOnlyList<DefectEscalationResult> Escalated,
    IReadOnlyList<DefectEscalationSkip> Skipped);
