namespace ComplianceCore.Api.Contracts;

public sealed record ComplianceFindingResponse(
    Guid FindingId,
    Guid RulePackId,
    string PackKey,
    Guid? RuleEvaluationRunId,
    string FindingKey,
    string Severity,
    string Status,
    string? RuleKey,
    string? FactKey,
    string Title,
    string Message,
    string ReasonCode,
    DateTimeOffset CreatedAt);

public sealed record CreateComplianceFindingRequest(
    Guid RulePackId,
    Guid? RuleEvaluationRunId,
    string FindingKey,
    string Severity,
    string? RuleKey,
    string? FactKey,
    string Title,
    string Message,
    string ReasonCode);

public sealed record UpdateComplianceFindingStatusRequest(string Status);
