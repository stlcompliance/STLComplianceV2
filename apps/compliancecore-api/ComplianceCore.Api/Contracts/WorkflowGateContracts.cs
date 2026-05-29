namespace ComplianceCore.Api.Contracts;

public sealed record WorkflowGateDefinitionResponse(
    Guid WorkflowGateId,
    string GateKey,
    string Label,
    string Description,
    Guid RulePackId,
    string PackKey,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateWorkflowGateDefinitionRequest(
    string GateKey,
    string Label,
    string Description,
    Guid RulePackId);

public sealed record WorkflowGateCheckRequest(
    string GateKey,
    IReadOnlyDictionary<string, bool>? Facts,
    IReadOnlyDictionary<string, string>? Context,
    bool EmitFindings = false);

public sealed record InternalWorkflowGateCheckRequest(
    Guid TenantId,
    string GateKey,
    IReadOnlyDictionary<string, string>? Context,
    bool EmitFindings = false);

public sealed record WorkflowGateReasonResponse(
    string Code,
    string Message,
    string? RuleKey,
    string? FactKey);

public sealed record WorkflowGateCheckResponse(
    Guid CheckResultId,
    string GateKey,
    string GateLabel,
    Guid RulePackId,
    string PackKey,
    string Outcome,
    string ReasonCode,
    string Message,
    Guid? RuleEvaluationRunId,
    IReadOnlyList<WorkflowGateReasonResponse> Reasons,
    IReadOnlyList<ComplianceFindingResponse> FindingsEmitted,
    DateTimeOffset CheckedAt,
    Guid? AppliedWaiverId = null,
    string? AppliedWaiverKey = null);

public sealed record WorkflowGateBatchCheckItem(
    string GateKey,
    IReadOnlyDictionary<string, bool>? Facts = null,
    IReadOnlyDictionary<string, string>? Context = null);

public sealed record WorkflowGateBatchCheckRequest(
    IReadOnlyList<WorkflowGateBatchCheckItem> Items,
    IReadOnlyDictionary<string, bool>? Facts = null,
    IReadOnlyDictionary<string, string>? Context = null,
    bool EmitFindings = false);

public sealed record InternalWorkflowGateBatchCheckItem(
    string GateKey,
    IReadOnlyDictionary<string, string>? Context = null);

public sealed record InternalWorkflowGateBatchCheckRequest(
    Guid TenantId,
    IReadOnlyList<InternalWorkflowGateBatchCheckItem> Items,
    IReadOnlyDictionary<string, string>? Context = null,
    bool EmitFindings = false);

public sealed record WorkflowGateBatchCheckSummary(
    int Total,
    int AllowCount,
    int WarnCount,
    int BlockCount,
    int WaivedCount);

public sealed record WorkflowGateBatchCheckResponse(
    Guid BatchId,
    IReadOnlyList<WorkflowGateCheckResponse> Results,
    WorkflowGateBatchCheckSummary Summary);
