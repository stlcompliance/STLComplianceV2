using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TenantTrainingNotificationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public string? NotificationWebhookUrl { get; set; }

    public bool NotifyOnAssignmentCreated { get; set; } = true;

    public bool NotifyOnQualificationExpiring { get; set; } = true;

    public bool NotifyOnQualificationExpired { get; set; } = true;

    public bool NotifyOnAssignmentCompleted { get; set; } = true;

    public bool NotifyOnQualificationIssued { get; set; } = true;

    public bool NotifyOnQualificationSuspended { get; set; } = true;

    public bool NotifyOnQualificationRevoked { get; set; } = true;

    public int ExpiringLeadDays { get; set; } = 30;

    public int MaxAttempts { get; set; } = 10;

    public int RetryIntervalMinutes { get; set; } = 5;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
