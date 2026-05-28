namespace ComplianceCore.Api.Contracts;

public sealed record LoadTestJourneySeedResponse(
    string RulePackKey,
    Guid RulePackId,
    bool RulePackCreated,
    bool RuleContentEnsured,
    bool DriverLicenseFactEnsured,
    int DispatchGatesCreated,
    IReadOnlyList<string> DispatchGateKeys);
