using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class RiskScoreRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public Guid? ActorUserId { get; set; }

    public int PacksEvaluatedCount { get; set; }

    public int HighestRiskScore { get; set; }

    public string HighestRiskLevel { get; set; } = RiskScoreLevels.Low;

    public DateTimeOffset EvaluatedAt { get; set; }
}

public sealed class RiskScore : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RunId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public Guid RulePackId { get; set; }

    public string PackKey { get; set; } = string.Empty;

    public int RiskScoreValue { get; set; }

    public string RiskLevel { get; set; } = RiskScoreLevels.Low;

    public string RuleOutcome { get; set; } = string.Empty;

    public string EvaluationResult { get; set; } = string.Empty;

    public int UnresolvedFactCount { get; set; }

    public int FailedRuleCount { get; set; }

    public int ResolvedFactCount { get; set; }

    public int MirrorFactCount { get; set; }

    public string Summary { get; set; } = string.Empty;

    public DateTimeOffset EvaluatedAt { get; set; }

    public RiskScoreRun? Run { get; set; }
}

public static class RiskScoreLevels
{
    public const string Low = "low";

    public const string Medium = "medium";

    public const string High = "high";

    public const string Critical = "critical";

    public static string FromScore(int score) =>
        score switch
        {
            <= 25 => Low,
            <= 50 => Medium,
            <= 75 => High,
            _ => Critical,
        };
}
