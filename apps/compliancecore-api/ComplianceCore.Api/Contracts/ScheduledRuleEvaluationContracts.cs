namespace ComplianceCore.Api.Contracts;

public sealed record PendingScheduledRuleEvaluationItem(
    Guid TenantId,
    Guid RulePackId,
    string PackKey,
    string Label,
    int VersionNumber,
    DateTimeOffset? LastScheduledEvaluationAt);

public sealed record PendingScheduledRuleEvaluationsResponse(
    DateTimeOffset AsOfUtc,
    int IntervalHours,
    int BatchSize,
    IReadOnlyList<PendingScheduledRuleEvaluationItem> Items);

public sealed record ProcessScheduledRuleEvaluationsRequest(
    Guid? TenantId = null,
    DateTimeOffset? AsOfUtc = null,
    int? BatchSize = null,
    int? IntervalHours = null,
    bool EmitFindings = true);

public sealed record ScheduledRuleEvaluationSkip(
    Guid RulePackId,
    string PackKey,
    string Reason);

public sealed record ProcessScheduledRuleEvaluationsResponse(
    Guid ScheduledRunId,
    DateTimeOffset AsOfUtc,
    int IntervalHours,
    int BatchSize,
    int PacksDueCount,
    int PacksProcessedCount,
    int EvaluatedCount,
    int SkippedCount,
    int AllowCount,
    int WarnCount,
    int BlockCount,
    IReadOnlyList<Guid> EvaluationRunIds,
    IReadOnlyList<ScheduledRuleEvaluationSkip> Skipped);
