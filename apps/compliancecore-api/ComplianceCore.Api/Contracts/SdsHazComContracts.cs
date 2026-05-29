namespace ComplianceCore.Api.Contracts;

public sealed record SdsReferenceResponse(
    Guid SdsReferenceId,
    string SdsKey,
    Guid? MaterialKeyId,
    string? MaterialKey,
    string ProductName,
    string Manufacturer,
    string DocumentUrl,
    DateOnly? RevisionDate,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateSdsReferenceRequest(
    string SdsKey,
    Guid? MaterialKeyId,
    string ProductName,
    string Manufacturer,
    string DocumentUrl,
    DateOnly? RevisionDate,
    bool IsActive = true);

public sealed record UpdateSdsReferenceRequest(
    Guid? MaterialKeyId,
    string? ProductName,
    string? Manufacturer,
    string? DocumentUrl,
    DateOnly? RevisionDate,
    bool? IsActive);

public sealed record HazComReferenceResponse(
    Guid HazComReferenceId,
    string HazComKey,
    string Title,
    string Description,
    string? LinkedSdsKey,
    string LocationRef,
    string DocumentUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateHazComReferenceRequest(
    string HazComKey,
    string Title,
    string Description,
    string? LinkedSdsKey,
    string LocationRef,
    string DocumentUrl,
    bool IsActive = true);

public sealed record UpdateHazComReferenceRequest(
    string? Title,
    string? Description,
    string? LinkedSdsKey,
    string? LocationRef,
    string? DocumentUrl,
    bool? IsActive);

public sealed record RuleVersionResponse(
    Guid RulePackId,
    string PackKey,
    string ProgramKey,
    string ProgramLabel,
    int VersionNumber,
    string Status,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RuleVersionListResponse(IReadOnlyList<RuleVersionResponse> Items);

public sealed record RuleVersionRollbackResponse(
    RuleVersionResponse ArchivedVersion,
    RuleVersionResponse RestoredVersion);
