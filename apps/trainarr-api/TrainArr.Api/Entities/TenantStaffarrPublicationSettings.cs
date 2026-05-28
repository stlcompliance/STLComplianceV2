using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TenantStaffarrPublicationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int MaxAttempts { get; set; } = 10;

    public int RetryIntervalMinutes { get; set; } = 5;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
