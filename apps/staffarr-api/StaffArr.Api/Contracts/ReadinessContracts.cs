namespace StaffArr.Api.Contracts;

public sealed record PersonReadinessResponse(
    Guid PersonId,
    string ReadinessStatus,
    string ReadinessBasis,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<ReadinessRequirementStatusResponse> Requirements,
    IReadOnlyList<ReadinessBlockerResponse> Blockers,
    ReadinessOverrideSummaryResponse? ActiveOverride,
    ReadinessAuditSnapshotResponse? AuditSnapshot = null);

public sealed record ReadinessAuditSnapshotResponse(
    Guid AuditEventId,
    string SnapshotKind,
    DateTimeOffset CapturedAt);

public sealed record ReadinessRequirementStatusResponse(
    Guid CertificationDefinitionId,
    string CertificationKey,
    string CertificationName,
    string RequirementStatus,
    string? RecordEffectiveStatus,
    DateTimeOffset? ExpiresAt);

public sealed record ReadinessBlockerResponse(
    string BlockerSource,
    string BlockerType,
    string Message,
    string? CertificationKey,
    string? CertificationName,
    string? QualificationKey,
    string? QualificationName);
