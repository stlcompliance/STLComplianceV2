namespace ComplianceCore.Api.Contracts;

public sealed record RegulatoryMappingResponse(
    Guid RegulatoryMappingId,
    string MappingKey,
    string Label,
    string Description,
    string TargetKind,
    Guid RegulatoryProgramId,
    string RegulatoryProgramKey,
    string RegulatoryProgramLabel,
    Guid? RulePackId,
    string? RulePackKey,
    string? RulePackLabel,
    Guid? CitationId,
    string? CitationKey,
    Guid? FactDefinitionId,
    string? FactKey,
    Guid? ComplianceKeyId,
    string? ComplianceKey,
    Guid? MaterialKeyId,
    string? MaterialKey,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRegulatoryMappingRequest(
    string MappingKey,
    string Label,
    string Description,
    string TargetKind,
    Guid RegulatoryProgramId,
    Guid? RulePackId,
    Guid? CitationId,
    Guid? FactDefinitionId,
    Guid? ComplianceKeyId,
    Guid? MaterialKeyId);

public sealed record DerivedFactPreviewRequest(
    Guid? RegulatoryProgramId,
    Guid? RulePackId,
    Guid? CitationId,
    Guid? ComplianceKeyId,
    Guid? MaterialKeyId,
    int? Limit);

public sealed record DerivedFactPreviewResponse(
    DateTimeOffset GeneratedAt,
    int ReturnedCount,
    IReadOnlyList<RegulatoryMappingResponse> Items);
