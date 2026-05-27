using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ComplianceFinding : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RulePackId { get; set; }

    public Guid? RuleEvaluationRunId { get; set; }

    public string FindingKey { get; set; } = string.Empty;

    public string Severity { get; set; } = FindingSeverities.Block;

    public string Status { get; set; } = FindingStatuses.Open;

    public string? RuleKey { get; set; }

    public string? FactKey { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public RulePack? RulePack { get; set; }

    public RuleEvaluationRun? RuleEvaluationRun { get; set; }
}

public static class FindingSeverities
{
    public const string Warn = "warn";

    public const string Block = "block";
}

public static class FindingStatuses
{
    public const string Open = "open";

    public const string Acknowledged = "acknowledged";

    public const string Resolved = "resolved";
}
