namespace ComplianceCore.Api.Contracts;

public sealed record OperatorDashboardResponse(
    OperatorDashboardFindingsSummary Findings,
    OperatorDashboardRulePackSummary RulePacks,
    OperatorDashboardEvaluationsSummary Evaluations,
    OperatorDashboardWorkflowGateSummary WorkflowGates,
    OperatorDashboardAuditSummary AuditEvents,
    IReadOnlyList<OperatorDashboardRecentEvaluation> RecentEvaluations,
    DateTimeOffset GeneratedAt);

public sealed record OperatorDashboardFindingsSummary(
    int OpenCount,
    int OpenBlockSeverityCount,
    int OpenWarnSeverityCount,
    int AcknowledgedCount,
    int ResolvedCount,
    int TotalCount);

public sealed record OperatorDashboardRulePackSummary(
    int DraftCount,
    int ReviewCount,
    int PublishedCount,
    int ArchivedCount,
    int TotalCount);

public sealed record OperatorDashboardEvaluationsSummary(
    int TotalCount,
    int Last24HoursCount,
    int PassCount,
    int FailCount);

public sealed record OperatorDashboardWorkflowGateSummary(
    int DefinitionCount,
    int CheckResultsTotal,
    int CheckResultsLast24Hours,
    int BlockOutcomeCount,
    int WarnOutcomeCount,
    int AllowOutcomeCount);

public sealed record OperatorDashboardAuditSummary(
    int TotalCount,
    int Last24HoursCount,
    int SuccessCount,
    int FailureCount);

public sealed record OperatorDashboardRecentEvaluation(
    Guid EvaluationRunId,
    Guid RulePackId,
    string RulePackLabel,
    string PackKey,
    string OverallResult,
    DateTimeOffset CreatedAt);
