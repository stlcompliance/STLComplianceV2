namespace ComplianceCore.Api.Contracts;

public sealed record RuleCatalogItemResponse(
    string RuleId,
    Guid RulePackId,
    string RulePackKey,
    string RulePackLabel,
    int RulePackVersion,
    string RulePackStatus,
    string RuleKey,
    string Label,
    string Type,
    string FactKey,
    bool ExpectedValue,
    bool NonWaivable,
    bool RemediationRequired,
    bool ReviewRequired,
    DateTimeOffset UpdatedAt);

public sealed record CreateRuleCatalogRequest(
    Guid RulePackId,
    string RuleKey,
    string Label,
    string Type,
    string FactKey,
    bool ExpectedValue,
    bool NonWaivable = false,
    bool RemediationRequired = false,
    bool ReviewRequired = false);

public sealed record PatchRuleCatalogRequest(
    string? Label,
    string? Type,
    string? FactKey,
    bool? ExpectedValue,
    bool? NonWaivable,
    bool? RemediationRequired = null,
    bool? ReviewRequired = null);

public sealed record RuleCatalogValidateResponse(
    bool IsValid,
    IReadOnlyList<string> Errors);

public sealed record RuleCatalogTestRequest(
    IReadOnlyDictionary<string, bool> Facts);

public sealed record RuleCatalogTestResponse(
    string Result,
    string Message,
    RuleEvaluationItemResponse Evaluation);

public sealed record RuleCatalogUsageResponse(
    int EvaluationRunCount,
    int FindingCount,
    int WaiverCount);

public sealed record RuleCatalogHistoryItemResponse(
    Guid RulePackId,
    string RulePackKey,
    int RulePackVersion,
    string RulePackStatus,
    DateTimeOffset UpdatedAt,
    bool ExistsInVersion);

public sealed record RuleCatalogHistoryResponse(
    string RuleKey,
    IReadOnlyList<RuleCatalogHistoryItemResponse> History);
