using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class TenantPersonExportSchedule : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int IntervalHours { get; set; }

    public DateTimeOffset? LastDeliveredAt { get; set; }

    public string? NotificationWebhookUrl { get; set; }

    public bool NotifyOnSuccess { get; set; } = true;

    public bool NotifyOnFailure { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
