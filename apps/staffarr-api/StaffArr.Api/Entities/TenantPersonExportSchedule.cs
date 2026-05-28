using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class TenantPersonExportSchedule : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int IntervalHours { get; set; }

    public DateTimeOffset? LastDeliveredAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
