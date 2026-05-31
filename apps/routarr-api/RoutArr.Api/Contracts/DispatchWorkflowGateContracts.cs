namespace RoutArr.Api.Contracts;

public sealed record DispatchWorkflowGateCheckRequest(
    Guid TripId,
    string? DriverPersonId = null,
    string? VehicleRefKey = null,
    string? AssignmentKind = null);

public sealed record DispatchWorkflowGateReasonSummary(
    string Code,
    string Message,
    string? RuleKey,
    string? FactKey);

public sealed record DispatchWorkflowGateResultSummary(
    string GateKey,
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    Guid? CheckResultId = null,
    string? GateLabel = null,
    Guid? RuleEvaluationRunId = null,
    IReadOnlyList<DispatchWorkflowGateReasonSummary>? Reasons = null,
    DateTimeOffset? CheckedAt = null,
    Guid? AppliedWaiverId = null,
    string? AppliedWaiverKey = null);

public sealed record DispatchWorkflowGateAuditSnapshotResponse(
    Guid AuditEventId,
    DateTimeOffset OccurredAt,
    string Action,
    string Result,
    string? ReasonCode);

public sealed record DispatchWorkflowGateCheckResponse(
    Guid TripId,
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    IReadOnlyList<DispatchWorkflowGateResultSummary> Gates,
    Guid? BatchId = null,
    DateTimeOffset? CheckedAt = null,
    IReadOnlyDictionary<string, string>? ContextSnapshot = null,
    DispatchWorkflowGateAuditSnapshotResponse? AuditSnapshot = null);

public sealed record DispatchAssignmentWorkflowGateSummary(
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    IReadOnlyList<DispatchWorkflowGateResultSummary> Gates,
    Guid? BatchId = null,
    DateTimeOffset? CheckedAt = null,
    IReadOnlyDictionary<string, string>? ContextSnapshot = null,
    DispatchWorkflowGateAuditSnapshotResponse? AuditSnapshot = null);

public static class DispatchWorkflowGateOutcomes
{
    public const string Allow = "allow";

    public const string Warn = "warn";

    public const string Block = "block";

    public const string Waived = "waived";
}
