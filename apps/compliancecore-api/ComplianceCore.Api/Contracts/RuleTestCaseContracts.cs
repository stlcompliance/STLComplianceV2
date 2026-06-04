namespace ComplianceCore.Api.Contracts;

public sealed record RuleTestCaseResponse(
    Guid RuleTestCaseId,
    Guid RulePackId,
    string RulePackKey,
    int RulePackVersion,
    string RulePackStatus,
    string RuleId,
    string RuleKey,
    string TestKey,
    string Label,
    string Description,
    string ExpectedResult,
    IReadOnlyDictionary<string, bool> Facts,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRuleTestCaseRequest(
    string RuleKey,
    string TestKey,
    string Label,
    string Description,
    IReadOnlyDictionary<string, bool> Facts,
    string ExpectedResult = "pass");

public sealed record PatchRuleTestCaseRequest(
    string? RuleKey = null,
    string? TestKey = null,
    string? Label = null,
    string? Description = null,
    IReadOnlyDictionary<string, bool>? Facts = null,
    string? ExpectedResult = null);

public sealed record RuleTestCaseRunResponse(
    Guid RuleTestCaseId,
    string RuleId,
    string ExpectedResult,
    string ActualResult,
    bool Passed,
    string Message,
    RuleEvaluationItemResponse Evaluation,
    DateTimeOffset EvaluatedAt);
