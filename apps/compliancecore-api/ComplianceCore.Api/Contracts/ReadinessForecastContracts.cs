namespace ComplianceCore.Api.Contracts;

public sealed record EvaluateReadinessForecastRequest(
    string? ScopeKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context);

public sealed record ReadinessForecastResponse(
    Guid ForecastId,
    Guid RunId,
    string ScopeKey,
    Guid RulePackId,
    string PackKey,
    int ReadinessScore,
    string ReadinessLevel,
    int RiskScore,
    string RiskLevel,
    int EffectivenessScore,
    string EffectivenessLevel,
    int MissingEvidenceWarningCount,
    string HighestMissingEvidenceSeverity,
    string Summary,
    DateTimeOffset ForecastedAt);

public sealed record EvaluateReadinessForecastResponse(
    Guid RunId,
    string ScopeKey,
    int PacksForecastCount,
    int ReadinessScore,
    string ReadinessLevel,
    int LowestReadinessScore,
    int AverageReadinessScore,
    int HighestRiskScore,
    int MissingEvidenceWarningCount,
    int AverageEffectivenessScore,
    Guid RiskScoreRunId,
    Guid MissingEvidenceWarningRunId,
    Guid ControlEffectivenessRunId,
    DateTimeOffset ForecastedAt,
    IReadOnlyList<ReadinessForecastResponse> Forecasts);

public sealed record ReadinessForecastSummaryResponse(
    int TotalForecasts,
    int ScopesTracked,
    int ReadyCount,
    int CautionCount,
    int NotReadyCount,
    int UnknownCount,
    int ReadinessScore,
    string ReadinessLevel,
    int LowestReadinessScore,
    int AverageReadinessScore,
    DateTimeOffset? LastForecastedAt,
    DateTimeOffset GeneratedAt);
