using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class WorkflowGateCheckResult : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkflowGateDefinitionId { get; set; }

    public Guid? RuleEvaluationRunId { get; set; }

    public string GateKey { get; set; } = string.Empty;

    public string Outcome { get; set; } = "block";

    public string ReasonCode { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string ReasonsJson { get; set; } = "[]";

    public string ContextJson { get; set; } = "{}";

    public Guid? AppliedWaiverId { get; set; }

    public string? AppliedWaiverKey { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public WorkflowGateDefinition? WorkflowGateDefinition { get; set; }

    public RuleEvaluationRun? RuleEvaluationRun { get; set; }
}
