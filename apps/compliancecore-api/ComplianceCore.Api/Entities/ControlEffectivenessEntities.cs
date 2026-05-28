using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ControlEffectivenessRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public Guid? ActorUserId { get; set; }

    public int PacksEvaluatedCount { get; set; }

    public int LowestEffectivenessScore { get; set; }

    public string LowestEffectivenessLevel { get; set; } = ControlEffectivenessLevels.Unknown;

    public int AverageEffectivenessScore { get; set; }

    public DateTimeOffset EvaluatedAt { get; set; }
}

public sealed class ControlEffectivenessRecord : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RunId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public Guid RulePackId { get; set; }

    public string PackKey { get; set; } = string.Empty;

    public int EffectivenessScore { get; set; }

    public string EffectivenessLevel { get; set; } = ControlEffectivenessLevels.Unknown;

    public string ControlStatus { get; set; } = ControlEffectivenessStatuses.Degraded;

    public string RuleOutcome { get; set; } = string.Empty;

    public string EvaluationResult { get; set; } = string.Empty;

    public int TotalRuleCount { get; set; }

    public int PassedRuleCount { get; set; }

    public int FailedRuleCount { get; set; }

    public int UnresolvedFactCount { get; set; }

    public int ResolvedFactCount { get; set; }

    public string Summary { get; set; } = string.Empty;

    public DateTimeOffset EvaluatedAt { get; set; }

    public ControlEffectivenessRun? Run { get; set; }
}

public static class ControlEffectivenessLevels
{
    public const string Effective = "effective";

    public const string PartiallyEffective = "partially_effective";

    public const string Ineffective = "ineffective";

    public const string Unknown = "unknown";

    public static int Rank(string level) =>
        level switch
        {
            Effective => 4,
            PartiallyEffective => 3,
            Ineffective => 2,
            _ => 1,
        };

    public static string FromScore(int score) =>
        score switch
        {
            >= 80 => Effective,
            >= 55 => PartiallyEffective,
            >= 30 => Ineffective,
            _ => Unknown,
        };

    public static string Min(string left, string right) =>
        Rank(left) <= Rank(right) ? left : right;
}

public static class ControlEffectivenessStatuses
{
    public const string Operating = "operating";

    public const string Degraded = "degraded";

    public const string Failing = "failing";

    public const string Unknown = "unknown";

    public static string FromLevel(string level) =>
        level switch
        {
            ControlEffectivenessLevels.Effective => Operating,
            ControlEffectivenessLevels.PartiallyEffective => Degraded,
            ControlEffectivenessLevels.Ineffective => Failing,
            _ => Unknown,
        };
}
