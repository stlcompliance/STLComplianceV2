namespace ComplianceCore.Api.Contracts;

public sealed record EvaluateMissingEvidenceWarningsRequest(
    string? ScopeKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context);

public sealed record MissingEvidenceWarningResponse(
    Guid WarningId,
    Guid RunId,
    string ScopeKey,
    Guid RulePackId,
    string PackKey,
    string FactKey,
    Guid? FactDefinitionId,
    string WarningType,
    string Severity,
    string ReasonCode,
    bool HasMirrorAtScope,
    bool IsRequiredInRule,
    bool IsRequiredInCatalog,
    string Summary,
    DateTimeOffset EvaluatedAt);

public sealed record EvaluateMissingEvidenceWarningsResponse(
    Guid RunId,
    string ScopeKey,
    int PacksAnalyzedCount,
    int WarningsEmittedCount,
    string HighestSeverity,
    int MirrorFactCount,
    DateTimeOffset EvaluatedAt,
    IReadOnlyList<MissingEvidenceWarningResponse> Warnings);

public sealed record MissingEvidenceWarningSummaryResponse(
    int TotalWarnings,
    int ScopesTracked,
    int LowCount,
    int MediumCount,
    int HighCount,
    int CriticalCount,
    string HighestSeverity,
    DateTimeOffset? LastEvaluatedAt,
    DateTimeOffset GeneratedAt);
