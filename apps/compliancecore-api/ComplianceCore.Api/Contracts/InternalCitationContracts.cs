namespace ComplianceCore.Api.Contracts;

public sealed record InternalCitationLookupRequest(
    Guid TenantId,
    IReadOnlyList<Guid> CitationIds);

public sealed record InternalCitationLookupItem(
    Guid CitationId,
    string CitationKey,
    int VersionNumber,
    string Label,
    string SourceReference,
    string Description,
    string RegulatoryProgramKey,
    string? RulePackKey,
    bool IsActive);
