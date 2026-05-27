namespace ComplianceCore.Api.Contracts;

public sealed record InternalRulePackLookupRequest(
    Guid TenantId,
    IReadOnlyList<string> RulePackKeys);

public sealed record InternalRulePackLookupItem(
    string RulePackKey,
    string Label,
    string Description,
    string RegulatoryProgramKey,
    string RegulatoryProgramLabel,
    int VersionNumber,
    string Status,
    bool IsActive);
