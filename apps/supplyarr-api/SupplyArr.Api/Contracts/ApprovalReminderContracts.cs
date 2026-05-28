namespace SupplyArr.Api.Contracts;

public sealed record ApprovalReminderSettingsResponse(
    bool IsEnabled,
    int PrReminderAfterHours,
    int PoReminderAfterHours,
    int ReminderCooldownHours,
    int MaxRemindersPerSubject,
    bool NotifyOnPrApprovalReminder,
    bool NotifyOnPoApprovalReminder,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertApprovalReminderSettingsRequest(
    bool IsEnabled,
    int PrReminderAfterHours,
    int PoReminderAfterHours,
    int ReminderCooldownHours,
    int MaxRemindersPerSubject,
    bool NotifyOnPrApprovalReminder,
    bool NotifyOnPoApprovalReminder);

public sealed record PendingApprovalReminderItem(
    string SubjectType,
    Guid SubjectId,
    string DocumentKey,
    string Title,
    string DocumentStatus,
    DateTimeOffset PendingSince,
    DateTimeOffset? LastReminderSentAt,
    int ReminderCount,
    double HoursPending,
    double HoursUntilNextReminder);

public sealed record PendingApprovalRemindersResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingApprovalReminderItem> Items);

public sealed record ApprovalReminderRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int RemindersSentCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record ApprovalReminderRunsResponse(
    IReadOnlyList<ApprovalReminderRunItem> Items);

public sealed record ProcessApprovalRemindersRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ApprovalReminderResult(
    string SubjectType,
    Guid SubjectId,
    string DocumentKey,
    int ReminderCount,
    Guid? NotificationDispatchId);

public sealed record ApprovalReminderSkip(
    string SubjectType,
    Guid SubjectId,
    string Reason);

public sealed record ProcessApprovalRemindersResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int RemindersSentCount,
    int SkippedCount,
    IReadOnlyList<ApprovalReminderResult> RemindersSent,
    IReadOnlyList<ApprovalReminderSkip> Skipped);

public sealed record ApprovalReminderSummaryResponse(
    Guid ReminderStateId,
    string SubjectType,
    Guid SubjectId,
    string DocumentKey,
    string Title,
    string DocumentStatus,
    Guid? VendorPartyId,
    DateTimeOffset PendingSince,
    DateTimeOffset? LastReminderSentAt,
    int ReminderCount,
    double HoursPending,
    bool IsOverdue);

public sealed record ApprovalRemindersDashboardResponse(
    int OverdueCount,
    int PendingCount,
    IReadOnlyList<ApprovalReminderSummaryResponse> Items);
