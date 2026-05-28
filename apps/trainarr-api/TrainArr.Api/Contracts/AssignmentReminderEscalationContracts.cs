namespace TrainArr.Api.Contracts;

public sealed record AssignmentDueReminderSettingsResponse(
    bool IsEnabled,
    int DueSoonLeadDays,
    int ReminderCooldownHours,
    int MaxRemindersPerAssignment,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertAssignmentDueReminderSettingsRequest(
    bool IsEnabled,
    int DueSoonLeadDays,
    int ReminderCooldownHours,
    int MaxRemindersPerAssignment);

public sealed record PendingAssignmentDueReminderItem(
    Guid TrainingAssignmentId,
    Guid StaffarrPersonId,
    DateTimeOffset DueAt,
    int DueReminderCount,
    DateTimeOffset? LastDueReminderSentAt,
    double HoursUntilDue,
    double? HoursUntilNextReminder);

public sealed record PendingAssignmentDueRemindersResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingAssignmentDueReminderItem> Items);

public sealed record AssignmentDueReminderResult(
    Guid TrainingAssignmentId,
    int DueReminderCount,
    Guid? NotificationDispatchId);

public sealed record AssignmentDueReminderSkip(
    Guid TrainingAssignmentId,
    string Reason);

public sealed record ProcessAssignmentDueRemindersRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcessAssignmentDueRemindersResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int RemindersSentCount,
    int SkippedCount,
    IReadOnlyList<AssignmentDueReminderResult> RemindersSent,
    IReadOnlyList<AssignmentDueReminderSkip> Skipped);

public sealed record AssignmentDueReminderRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int RemindersSentCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record AssignmentDueReminderRunsResponse(
    IReadOnlyList<AssignmentDueReminderRunItem> Items);

public sealed record AssignmentEscalationSettingsResponse(
    bool IsEnabled,
    int OverdueEscalationAfterHours,
    int EscalationCooldownHours,
    int MaxEscalationsPerAssignment,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertAssignmentEscalationSettingsRequest(
    bool IsEnabled,
    int OverdueEscalationAfterHours,
    int EscalationCooldownHours,
    int MaxEscalationsPerAssignment);

public sealed record PendingAssignmentEscalationItem(
    Guid TrainingAssignmentId,
    Guid StaffarrPersonId,
    DateTimeOffset DueAt,
    int EscalationCount,
    DateTimeOffset? LastEscalatedAt,
    double HoursOverdue,
    double? HoursUntilNextEscalation);

public sealed record PendingAssignmentEscalationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingAssignmentEscalationItem> Items);

public sealed record AssignmentEscalationResult(
    Guid TrainingAssignmentId,
    int EscalationCount,
    Guid? NotificationDispatchId);

public sealed record AssignmentEscalationSkip(
    Guid TrainingAssignmentId,
    string Reason);

public sealed record ProcessAssignmentEscalationsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcessAssignmentEscalationsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount,
    IReadOnlyList<AssignmentEscalationResult> Escalated,
    IReadOnlyList<AssignmentEscalationSkip> Skipped);

public sealed record AssignmentEscalationRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int EscalatedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record AssignmentEscalationRunsResponse(
    IReadOnlyList<AssignmentEscalationRunItem> Items);

public sealed record AssignmentEscalationEventItem(
    Guid EventId,
    Guid TrainingAssignmentId,
    Guid StaffarrPersonId,
    DateTimeOffset? DueAt,
    int EscalationCount,
    DateTimeOffset CreatedAt);

public sealed record AssignmentEscalationEventsResponse(
    IReadOnlyList<AssignmentEscalationEventItem> Items);
