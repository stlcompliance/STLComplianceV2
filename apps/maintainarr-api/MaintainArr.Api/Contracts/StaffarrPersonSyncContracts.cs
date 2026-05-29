namespace MaintainArr.Api.Contracts;

public sealed record IngestStaffarrPersonSyncRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    string DisplayName,
    string EmploymentStatus,
    string? PrimarySite,
    string EventType,
    DateTimeOffset OccurredAt,
    string? CorrelationId);

public sealed record IngestStaffarrPersonSyncResponse(
    string PersonId,
    string DisplayName,
    bool IdempotentReplay);

public sealed record PendingTechnicianRefRefreshItem(
    string PersonId,
    DateTimeOffset LastSeenAt);

public sealed record PendingTechnicianRefRefreshResponse(
    int PendingCount,
    IReadOnlyList<PendingTechnicianRefRefreshItem> Items);

public sealed record ProcessTechnicianRefRefreshRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    TimeSpan? StaleAfter);

public sealed record ProcessTechnicianRefRefreshResponse(
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    int FailedCount);
