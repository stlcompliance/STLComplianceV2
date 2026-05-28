using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TenantRecertificationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int LeadDays { get; set; } = 30;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
