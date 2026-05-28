namespace StaffArr.Api.Contracts;

public sealed record PersonnelHistorySummaryResponse(
    Guid PersonId,
    int EventCount,
    int IncidentCount,
    int CertificationCount,
    int PermissionCount,
    int ReadinessCount,
    int TrainingBlockerCount,
    int PersonnelNoteCount,
    int PersonnelDocumentCount,
    DateTimeOffset? LastEventAt,
    DateTimeOffset ComputedAt,
    bool IsMaterialized);

public sealed record PendingPersonnelHistoryItem(
    Guid PersonId,
    string DisplayName,
    DateTimeOffset? LastComputedAt);

public sealed record PendingPersonnelHistoryResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingPersonnelHistoryItem> Items);

public sealed record ProcessPersonnelHistoryRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record PersonnelHistoryRefreshSkip(
    Guid PersonId,
    string Reason);

public sealed record ProcessPersonnelHistoryResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    IReadOnlyList<PersonnelHistorySummaryResponse> RefreshedRollups,
    IReadOnlyList<PersonnelHistoryRefreshSkip> Skipped);
