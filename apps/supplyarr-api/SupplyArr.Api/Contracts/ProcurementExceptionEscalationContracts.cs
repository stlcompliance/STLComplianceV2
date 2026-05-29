namespace SupplyArr.Api.Contracts;

public sealed record ProcurementExceptionEscalationSettingsResponse(
    bool IsEnabled,
    int EscalationCooldownHours,
    int MaxEscalationsPerException,
    bool NotifyOnProcurementExceptionSlaEscalation,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertProcurementExceptionEscalationSettingsRequest(
    bool IsEnabled,
    int EscalationCooldownHours,
    int MaxEscalationsPerException,
    bool NotifyOnProcurementExceptionSlaEscalation);

public sealed record PendingProcurementExceptionEscalationItem(
    Guid ProcurementExceptionId,
    string ExceptionKey,
    string SubjectType,
    Guid SubjectId,
    string SubjectKey,
    string Title,
    string Status,
    DateTimeOffset? SlaDueAt,
    int EscalationCount,
    DateTimeOffset? LastEscalatedAt,
    double HoursOverdue,
    double HoursUntilNextEscalation);

public sealed record PendingProcurementExceptionEscalationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingProcurementExceptionEscalationItem> Items);

public sealed record ProcurementExceptionEscalationRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record ProcurementExceptionEscalationRunsResponse(
    IReadOnlyList<ProcurementExceptionEscalationRunItem> Items);

public sealed record ProcurementExceptionEscalationEventItem(
    Guid EventId,
    Guid ProcurementExceptionId,
    string ExceptionKey,
    int EscalationLevel,
    string ActionKind,
    Guid? NotificationDispatchId,
    DateTimeOffset CreatedAt);

public sealed record ProcurementExceptionEscalationEventsResponse(
    IReadOnlyList<ProcurementExceptionEscalationEventItem> Items);

public sealed record ProcessProcurementExceptionEscalationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcurementExceptionEscalationResult(
    Guid ProcurementExceptionId,
    string ExceptionKey,
    int EscalationCount,
    Guid? NotificationDispatchId);

public sealed record ProcurementExceptionEscalationSkip(
    Guid ProcurementExceptionId,
    string Reason);

public sealed record ProcessProcurementExceptionEscalationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount,
    IReadOnlyList<ProcurementExceptionEscalationResult> Escalated,
    IReadOnlyList<ProcurementExceptionEscalationSkip> Skipped);
