namespace TrainArr.Api.Contracts;

public sealed record UpsertEvidenceRetentionSettingsRequest(
    bool IsEnabled,
    int RetentionDaysAfterAssignmentClose);

public sealed record EvidenceRetentionSettingsResponse(
    bool IsEnabled,
    int RetentionDaysAfterAssignmentClose,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessEvidenceRetentionRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record PendingEvidenceRetentionItem(
    Guid EvidenceId,
    Guid TenantId,
    Guid TrainingAssignmentId,
    DateTimeOffset EvidenceCreatedAt,
    DateTimeOffset AssignmentClosedAt);

public sealed record PendingEvidenceRetentionResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingEvidenceRetentionItem> Items);

public sealed record EvidenceRetentionPurgeSkip(
    Guid EvidenceId,
    string Reason);

public sealed record ProcessEvidenceRetentionResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int PurgedCount,
    long BytesReclaimed,
    int SkippedCount,
    IReadOnlyList<Guid> PurgedEvidenceIds,
    IReadOnlyList<EvidenceRetentionPurgeSkip> Skipped);

public sealed record EvidenceRetentionRunItem(
    Guid RunId,
    string Outcome,
    int EvidencePurgedCount,
    long BytesReclaimed,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record EvidenceRetentionRunsResponse(
    IReadOnlyList<EvidenceRetentionRunItem> Items);
