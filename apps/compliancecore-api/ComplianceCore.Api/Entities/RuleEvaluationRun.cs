using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class RuleEvaluationRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RulePackId { get; set; }

    public Guid? ActorUserId { get; set; }

    public string Status { get; set; } = RuleEvaluationRunStatuses.Completed;

    public string OverallResult { get; set; } = RuleEvaluationResults.Fail;

    public string FactInputsJson { get; set; } = "{}";

    public string RuleResultsJson { get; set; } = "[]";

    public Guid? AppliedWaiverId { get; set; }

    public string? AppliedWaiverKey { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public RulePack? RulePack { get; set; }
}

public static class RuleEvaluationRunStatuses
{
    public const string Completed = "completed";

    public const string Failed = "failed";
}

public static class RuleEvaluationResults
{
    public const string Pass = "pass";

    public const string Fail = "fail";
}
