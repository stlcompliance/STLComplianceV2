using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class TenantMaintenanceNotificationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public string? NotificationWebhookUrl { get; set; }

    public bool NotifyOnWorkOrderCreated { get; set; } = true;

    public bool NotifyOnPmScheduleDue { get; set; } = true;

    public bool NotifyOnPmScheduleOverdue { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class MaintenanceNotificationDispatch : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public Guid AssetId { get; set; }

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string DispatchStatus { get; set; } = string.Empty;

    public string? WebhookHost { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? DispatchedAt { get; set; }
}

public static class MaintenanceNotificationEventKinds
{
    public const string WorkOrderCreated = "work_order_created";

    public const string PmScheduleDue = "pm_schedule_due";

    public const string PmScheduleOverdue = "pm_schedule_overdue";
}

public static class MaintenanceNotificationDispatchStatuses
{
    public const string Pending = "pending";

    public const string Sent = "sent";

    public const string Failed = "failed";

    public const string Skipped = "skipped";
}
