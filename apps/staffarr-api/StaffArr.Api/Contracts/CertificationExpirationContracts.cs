namespace StaffArr.Api.Contracts;

public sealed record ProcessCertificationExpirationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int BatchSize);

public sealed record ProcessCertificationExpirationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int ExpiredCount,
    int SkippedCount,
    IReadOnlyList<Guid> ExpiredPersonCertificationIds,
    IReadOnlyList<CertificationExpirationSkip> Skipped);

public sealed record CertificationExpirationSkip(
    Guid PersonCertificationId,
    string Reason);

public sealed record PendingCertificationExpirationItem(
    Guid PersonCertificationId,
    Guid TenantId,
    Guid PersonId,
    Guid CertificationDefinitionId,
    string SourceType,
    string Status,
    DateTimeOffset ExpiresAt);

public sealed record PendingCertificationExpirationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingCertificationExpirationItem> Items);
