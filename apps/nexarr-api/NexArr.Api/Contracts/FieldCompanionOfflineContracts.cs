namespace NexArr.Api.Contracts;

public sealed record FieldCompanionOfflineActionItem(
    string IdempotencyKey,
    string ActionKind,
    string TaskKey,
    string ProductKey,
    DateTimeOffset ClientCreatedAt);

public sealed record SyncFieldCompanionOfflineActionsRequest(
    IReadOnlyList<FieldCompanionOfflineActionItem> Actions);

public sealed record FieldCompanionOfflineActionRejectedItem(
    string IdempotencyKey,
    string ReasonCode,
    string ReasonMessage);

public sealed record SyncFieldCompanionOfflineActionsResponse(
    int Accepted,
    int Duplicates,
    int Rejected,
    IReadOnlyList<FieldCompanionOfflineActionSyncedItem> Synced,
    IReadOnlyList<FieldCompanionOfflineActionRejectedItem> RejectedItems);

public sealed record FieldCompanionOfflineActionSyncedItem(
    string IdempotencyKey,
    string ActionKind,
    string TaskKey,
    string ProductKey,
    DateTimeOffset SyncedAt);

public sealed record FieldCompanionOfflineActionsListResponse(
    IReadOnlyList<FieldCompanionOfflineActionSyncedItem> Items);
