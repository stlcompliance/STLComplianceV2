namespace TrainArr.Api.Contracts;

public sealed record ProcessQualificationExpirationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int BatchSize);

public sealed record ProcessQualificationExpirationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int ExpiredCount,
    int SkippedCount,
    IReadOnlyList<Guid> ExpiredQualificationIssueIds,
    IReadOnlyList<QualificationExpirationSkip> Skipped);

public sealed record QualificationExpirationSkip(
    Guid QualificationIssueId,
    string Reason);

public sealed record PendingQualificationExpirationItem(
    Guid QualificationIssueId,
    Guid TenantId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset EffectiveExpiresAt);

public sealed record PendingQualificationExpirationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingQualificationExpirationItem> Items);
