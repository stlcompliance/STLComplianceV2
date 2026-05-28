using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ReadinessForecastRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public Guid? ActorUserId { get; set; }

    public int PacksForecastCount { get; set; }

    public int ReadinessScore { get; set; }

    public string ReadinessLevel { get; set; } = ReadinessForecastLevels.Caution;

    public int LowestReadinessScore { get; set; }

    public int AverageReadinessScore { get; set; }

    public int HighestRiskScore { get; set; }

    public int MissingEvidenceWarningCount { get; set; }

    public int AverageEffectivenessScore { get; set; }

    public Guid RiskScoreRunId { get; set; }

    public Guid MissingEvidenceWarningRunId { get; set; }

    public Guid ControlEffectivenessRunId { get; set; }

    public DateTimeOffset ForecastedAt { get; set; }
}

public sealed class ReadinessForecast : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RunId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public Guid RulePackId { get; set; }

    public string PackKey { get; set; } = string.Empty;

    public int ReadinessScore { get; set; }

    public string ReadinessLevel { get; set; } = ReadinessForecastLevels.Caution;

    public int RiskScore { get; set; }

    public string RiskLevel { get; set; } = string.Empty;

    public int EffectivenessScore { get; set; }

    public string EffectivenessLevel { get; set; } = string.Empty;

    public int MissingEvidenceWarningCount { get; set; }

    public string HighestMissingEvidenceSeverity { get; set; } = MissingEvidenceWarningSeverities.Low;

    public string Summary { get; set; } = string.Empty;

    public DateTimeOffset ForecastedAt { get; set; }

    public ReadinessForecastRun? Run { get; set; }
}

public static class ReadinessForecastLevels
{
    public const string Ready = "ready";

    public const string Caution = "caution";

    public const string NotReady = "not_ready";

    public const string Unknown = "unknown";

    public static int Rank(string level) =>
        level switch
        {
            Ready => 4,
            Caution => 3,
            NotReady => 2,
            _ => 1,
        };

    public static string FromScore(int score) =>
        score switch
        {
            >= 75 => Ready,
            >= 50 => Caution,
            >= 25 => NotReady,
            _ => Unknown,
        };

    public static string Min(string left, string right) =>
        Rank(left) <= Rank(right) ? left : right;
}
