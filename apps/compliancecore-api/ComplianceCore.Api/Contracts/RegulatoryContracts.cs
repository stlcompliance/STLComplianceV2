namespace ComplianceCore.Api.Contracts;



public sealed record GoverningBodyResponse(

    Guid GoverningBodyId,

    string BodyKey,

    string Label,

    string Description,

    bool IsActive,

    DateTimeOffset CreatedAt);



public sealed record CreateGoverningBodyRequest(

    string BodyKey,

    string Label,

    string Description);



public sealed record JurisdictionResponse(

    Guid JurisdictionId,

    Guid GoverningBodyId,

    string GoverningBodyKey,

    string GoverningBodyLabel,

    string JurisdictionKey,

    string Label,

    string Description,

    bool IsActive,

    DateTimeOffset CreatedAt);



public sealed record CreateJurisdictionRequest(

    Guid GoverningBodyId,

    string JurisdictionKey,

    string Label,

    string Description);



public sealed record RegulatoryProgramResponse(

    Guid RegulatoryProgramId,

    Guid JurisdictionId,

    string JurisdictionKey,

    string JurisdictionLabel,

    string ProgramKey,

    string Label,

    string Description,

    bool IsActive,

    DateTimeOffset CreatedAt);



public sealed record CreateRegulatoryProgramRequest(

    Guid JurisdictionId,

    string ProgramKey,

    string Label,

    string Description);



public sealed record RulePackResponse(

    Guid RulePackId,

    Guid RegulatoryProgramId,

    string RegulatoryProgramKey,

    string RegulatoryProgramLabel,

    string PackKey,

    string Label,

    string Description,

    int VersionNumber,

    string Status,

    bool IsActive,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record CreateRulePackRequest(

    Guid RegulatoryProgramId,

    string PackKey,

    string Label,

    string Description);



public sealed record UpdateRulePackStatusRequest(string Status);

public sealed record PatchRulePackRequest(
    string? Status = null,
    string? Label = null,
    string? Description = null);

public sealed record CloneRulePackRequest(
    string? PackKey = null,
    string? Label = null,
    string? Description = null,
    bool CopyContent = true);

public sealed record RulePackDiffResponse(
    Guid BaseRulePackId,
    Guid CompareRulePackId,
    int BaseVersionNumber,
    int CompareVersionNumber,
    string BaseStatus,
    string CompareStatus,
    bool MetadataChanged,
    bool ContentChanged,
    string? BaseContentHash,
    string? CompareContentHash);


