namespace NexArr.Api.Contracts;

public sealed record CompanionOfflineActionItem(
    string IdempotencyKey,
    string ActionKind,
    string TaskKey,
    string ProductKey,
    DateTimeOffset ClientCreatedAt);

public sealed record SyncCompanionOfflineActionsRequest(
    IReadOnlyList<CompanionOfflineActionItem> Actions);

public sealed record CompanionOfflineActionRejectedItem(
    string IdempotencyKey,
    string ReasonCode,
    string ReasonMessage);

public sealed record SyncCompanionOfflineActionsResponse(
    int Accepted,
    int Duplicates,
    int Rejected,
    IReadOnlyList<CompanionOfflineActionSyncedItem> Synced,
    IReadOnlyList<CompanionOfflineActionRejectedItem> RejectedItems);

public sealed record CompanionOfflineActionSyncedItem(
    string IdempotencyKey,
    string ActionKind,
    string TaskKey,
    string ProductKey,
    DateTimeOffset SyncedAt);

public sealed record CompanionOfflineActionsListResponse(
    IReadOnlyList<CompanionOfflineActionSyncedItem> Items);
