namespace StaffArr.Api.Contracts;

public sealed record PersonReadinessResponse(
    Guid PersonId,
    string ReadinessStatus,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<ReadinessRequirementStatusResponse> Requirements,
    IReadOnlyList<ReadinessBlockerResponse> Blockers);

public sealed record ReadinessRequirementStatusResponse(
    Guid CertificationDefinitionId,
    string CertificationKey,
    string CertificationName,
    string RequirementStatus,
    string? RecordEffectiveStatus,
    DateTimeOffset? ExpiresAt);

public sealed record ReadinessBlockerResponse(
    string CertificationKey,
    string CertificationName,
    string BlockerType,
    string Message);
