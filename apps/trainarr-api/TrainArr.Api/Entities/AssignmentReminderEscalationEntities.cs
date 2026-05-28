using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TenantAssignmentDueReminderSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int DueSoonLeadDays { get; set; } = AssignmentReminderEscalationDefaults.DueSoonLeadDays;

    public int ReminderCooldownHours { get; set; } = AssignmentReminderEscalationDefaults.ReminderCooldownHours;

    public int MaxRemindersPerAssignment { get; set; } = AssignmentReminderEscalationDefaults.MaxRemindersPerAssignment;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssignmentDueReminderRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int RemindersSentCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class TenantAssignmentEscalationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int OverdueEscalationAfterHours { get; set; } = AssignmentReminderEscalationDefaults.OverdueEscalationAfterHours;

    public int EscalationCooldownHours { get; set; } = AssignmentReminderEscalationDefaults.EscalationCooldownHours;

    public int MaxEscalationsPerAssignment { get; set; } = AssignmentReminderEscalationDefaults.MaxEscalationsPerAssignment;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssignmentEscalationEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingAssignmentId { get; set; }

    public Guid StaffarrPersonId { get; set; }

    public DateTimeOffset? DueAt { get; set; }

    public int EscalationCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AssignmentEscalationRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int EscalatedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class AssignmentReminderEscalationDefaults
{
    public const int DueSoonLeadDays = 7;

    public const int ReminderCooldownHours = 24;

    public const int MaxRemindersPerAssignment = 5;

    public const int OverdueEscalationAfterHours = 24;

    public const int EscalationCooldownHours = 48;

    public const int MaxEscalationsPerAssignment = 10;
}
