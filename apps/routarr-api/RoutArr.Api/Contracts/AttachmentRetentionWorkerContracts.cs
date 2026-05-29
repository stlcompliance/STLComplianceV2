namespace RoutArr.Api.Contracts;

public sealed record UpsertAttachmentRetentionSettingsRequest(
    bool IsEnabled,
    int RetentionDaysAfterTripClose);

public sealed record AttachmentRetentionSettingsResponse(
    bool IsEnabled,
    int RetentionDaysAfterTripClose,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessAttachmentRetentionRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record PendingAttachmentRetentionItem(
    Guid AttachmentId,
    Guid TenantId,
    Guid TripId,
    DateTimeOffset AttachmentCreatedAt,
    DateTimeOffset TripClosedAt);

public sealed record PendingAttachmentRetentionResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingAttachmentRetentionItem> Items);

public sealed record AttachmentRetentionPurgeSkip(
    Guid AttachmentId,
    string Reason);

public sealed record ProcessAttachmentRetentionResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int PurgedCount,
    long BytesReclaimed,
    int SkippedCount,
    IReadOnlyList<Guid> PurgedAttachmentIds,
    IReadOnlyList<AttachmentRetentionPurgeSkip> Skipped);

public sealed record AttachmentRetentionRunItem(
    Guid RunId,
    string Outcome,
    int AttachmentsPurgedCount,
    long BytesReclaimed,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record AttachmentRetentionRunsResponse(
    IReadOnlyList<AttachmentRetentionRunItem> Items);
