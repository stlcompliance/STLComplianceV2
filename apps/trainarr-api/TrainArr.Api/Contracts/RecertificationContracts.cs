namespace TrainArr.Api.Contracts;

public sealed record UpsertRecertificationSettingsRequest(
    bool IsEnabled,
    int LeadDays);

public sealed record RecertificationSettingsResponse(
    bool IsEnabled,
    int LeadDays,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessRecertificationAssignmentsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int BatchSize);

public sealed record ProcessRecertificationAssignmentsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int AssignedCount,
    int SkippedCount,
    IReadOnlyList<Guid> CreatedAssignmentIds,
    IReadOnlyList<RecertificationAssignmentSkip> Skipped);

public sealed record RecertificationAssignmentSkip(
    Guid QualificationIssueId,
    string Reason);

public sealed record PendingRecertificationCandidate(
    Guid QualificationIssueId,
    Guid TenantId,
    Guid StaffarrPersonId,
    Guid TrainingDefinitionId,
    string TrainingDefinitionName,
    string QualificationKey,
    string QualificationName,
    DateTimeOffset EffectiveExpiresAt);

public sealed record PendingRecertificationCandidatesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingRecertificationCandidate> Items);

public sealed record RecertificationAssignmentRunItem(
    Guid RunId,
    Guid QualificationIssueId,
    Guid? TrainingAssignmentId,
    string Outcome,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record RecertificationAssignmentRunsResponse(
    IReadOnlyList<RecertificationAssignmentRunItem> Items);
