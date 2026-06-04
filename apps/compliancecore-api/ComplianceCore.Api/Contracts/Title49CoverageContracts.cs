namespace ComplianceCore.Api.Contracts;

public sealed record Title49CoverageExplorerResponse(
    Guid TenantId,
    int TotalRulePacks,
    int OperationalRulePackCount,
    int ReferenceRulePackCount,
    int MetadataRulePackCount,
    int TotalCitations,
    int TotalFacts,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<Title49CoverageExplorerItemResponse> RulePacks);

public sealed record Title49CoverageExplorerItemResponse(
    Guid RulePackId,
    string PackKey,
    string ProgramKey,
    string PackLabel,
    string CoverageKind,
    bool HasContent,
    int CitationCount,
    int FactRequirementCount,
    DateTimeOffset UpdatedAt);

public sealed record Title49CitationCoverageReportResponse(
    Guid TenantId,
    int TotalCitations,
    int ActiveCitationCount,
    int OperationalCitationCount,
    int ReferenceCitationCount,
    int UnmappedCitationCount,
    int TotalRulePacks,
    int TotalFactRequirements,
    int TotalMappings,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<Title49CitationCoverageReportItemResponse> Citations);

public sealed record Title49CitationCoverageReportItemResponse(
    Guid CitationId,
    string CitationKey,
    string SourceReference,
    string ProgramKey,
    string CitationLabel,
    string CoverageMode,
    bool IsActive,
    bool HasRulePack,
    int RulePackCount,
    int FactRequirementCount,
    int MappingCount,
    DateTimeOffset UpdatedAt);
