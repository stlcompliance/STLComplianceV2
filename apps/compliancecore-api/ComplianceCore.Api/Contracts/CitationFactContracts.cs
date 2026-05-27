namespace ComplianceCore.Api.Contracts;

public sealed record RegulatoryCitationResponse(
    Guid CitationId,
    Guid RegulatoryProgramId,
    string RegulatoryProgramKey,
    string RegulatoryProgramLabel,
    Guid? RulePackId,
    string? RulePackKey,
    string? RulePackLabel,
    string CitationKey,
    string Label,
    string SourceReference,
    string Description,
    int VersionNumber,
    Guid? SupersedesCitationId,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRegulatoryCitationRequest(
    Guid RegulatoryProgramId,
    Guid? RulePackId,
    string CitationKey,
    string Label,
    string SourceReference,
    string Description,
    Guid? SupersedesCitationId);

public sealed record FactDefinitionResponse(
    Guid FactDefinitionId,
    string FactKey,
    string Label,
    string Description,
    string ValueType,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateFactDefinitionRequest(
    string FactKey,
    string Label,
    string Description,
    string ValueType);

public sealed record FactRequirementResponse(
    Guid FactRequirementId,
    Guid FactDefinitionId,
    string FactKey,
    string FactLabel,
    Guid? RulePackId,
    string? RulePackKey,
    Guid? CitationId,
    string? CitationKey,
    string RequirementKey,
    string Label,
    string Description,
    bool IsRequired,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateFactRequirementRequest(
    Guid FactDefinitionId,
    Guid? RulePackId,
    Guid? CitationId,
    string RequirementKey,
    string Label,
    string Description,
    bool IsRequired);
