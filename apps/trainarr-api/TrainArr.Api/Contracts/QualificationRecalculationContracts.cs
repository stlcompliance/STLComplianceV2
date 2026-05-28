namespace TrainArr.Api.Contracts;

public sealed record UpsertQualificationRecalculationSettingsRequest(
    bool IsEnabled,
    int StalenessHours,
    bool AutoSuspendOnBlock);

public sealed record QualificationRecalculationSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    bool AutoSuspendOnBlock,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessQualificationRecalculationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record PendingQualificationRecalculationItem(
    Guid QualificationIssueId,
    Guid TenantId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset? LastComputedAt);

public sealed record PendingQualificationRecalculationsResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingQualificationRecalculationItem> Items);

public sealed record QualificationRecalculationSkip(
    Guid QualificationIssueId,
    string Reason);

public sealed record ProcessQualificationRecalculationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int RecalculatedCount,
    int SuspendedCount,
    int SkippedCount,
    IReadOnlyList<Guid> RecalculatedIssueIds,
    IReadOnlyList<Guid> SuspendedIssueIds,
    IReadOnlyList<QualificationRecalculationSkip> Skipped);

public sealed record QualificationRecalculationStateItem(
    Guid QualificationIssueId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string Outcome,
    string ReasonCode,
    string? RulePackKey,
    string? PreviousOutcome,
    DateTimeOffset ComputedAt);

public sealed record QualificationRecalculationStatesResponse(
    IReadOnlyList<QualificationRecalculationStateItem> Items);

public sealed record QualificationRecalculationRunItem(
    Guid RunId,
    Guid QualificationIssueId,
    string Outcome,
    string? CheckOutcome,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record QualificationRecalculationRunsResponse(
    IReadOnlyList<QualificationRecalculationRunItem> Items);
