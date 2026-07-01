using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TenantApprovalReminderSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int PrReminderAfterHours { get; set; } = ApprovalReminderDefaults.PrReminderAfterHours;

    public int PoReminderAfterHours { get; set; } = ApprovalReminderDefaults.PoReminderAfterHours;

    public int ReminderCooldownHours { get; set; } = ApprovalReminderDefaults.ReminderCooldownHours;

    public int MaxRemindersPerSubject { get; set; } = ApprovalReminderDefaults.MaxRemindersPerSubject;

    public bool NotifyOnPrApprovalReminder { get; set; } = true;

    public bool NotifyOnPoApprovalReminder { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ApprovalReminderState : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SubjectType { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public string DocumentKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string DocumentStatus { get; set; } = string.Empty;

    public Guid? SupplierId { get; set; }

    public DateTimeOffset PendingSince { get; set; }

    public DateTimeOffset? LastReminderSentAt { get; set; }

    public int ReminderCount { get; set; }

    public string? LastReminderEventKind { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ApprovalReminderRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int RemindersSentCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class ApprovalReminderDefaults
{
    public const int PrReminderAfterHours = 24;

    public const int PoReminderAfterHours = 24;

    public const int ReminderCooldownHours = 24;

    public const int MaxRemindersPerSubject = 10;
}

public static class ApprovalReminderSubjectTypes
{
    public const string PurchaseRequest = "purchase_request";

    public const string PurchaseOrder = "purchase_order";
}
