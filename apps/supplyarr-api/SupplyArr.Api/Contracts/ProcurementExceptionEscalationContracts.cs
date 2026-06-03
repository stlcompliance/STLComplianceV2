namespace SupplyArr.Api.Contracts;

public sealed record ProcurementExceptionEscalationSettingsResponse(
    bool IsEnabled,
    int EscalationCooldownHours,
    int MaxEscalationsPerException,
    bool NotifyOnProcurementExceptionSlaEscalation,
    bool AutoCloseCompletedExceptionsEnabled,
    int AutoCloseCompletedExceptionsAfterHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertProcurementExceptionEscalationSettingsRequest(
    bool IsEnabled,
    int EscalationCooldownHours,
    int MaxEscalationsPerException,
    bool NotifyOnProcurementExceptionSlaEscalation,
    bool AutoCloseCompletedExceptionsEnabled,
    int AutoCloseCompletedExceptionsAfterHours);

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

public sealed record PendingProcurementExceptionAutoCloseItem(
    Guid ProcurementExceptionId,
    string ExceptionKey,
    string SubjectType,
    Guid SubjectId,
    string SubjectKey,
    string Title,
    string Status,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? WaivedAt,
    DateTimeOffset? CompletedAt,
    double HoursCompleted,
    double HoursUntilAutoClose);

public sealed record PendingProcurementExceptionAutoClosesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingProcurementExceptionAutoCloseItem> Items);

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

public sealed record ProcessProcurementExceptionAutoClosesRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcurementExceptionAutoCloseResult(
    Guid ProcurementExceptionId,
    string ExceptionKey,
    string Status,
    DateTimeOffset ClosedAt);

public sealed record ProcurementExceptionAutoCloseSkip(
    Guid ProcurementExceptionId,
    string Reason);

public sealed record ProcessProcurementExceptionAutoClosesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int ClosedCount,
    int SkippedCount,
    IReadOnlyList<ProcurementExceptionAutoCloseResult> Closed,
    IReadOnlyList<ProcurementExceptionAutoCloseSkip> Skipped);
