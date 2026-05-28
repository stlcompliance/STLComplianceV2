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

    public int ExpiringLeadDays { get; set; } = 30;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
