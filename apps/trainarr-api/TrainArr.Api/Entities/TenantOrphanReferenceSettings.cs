using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TenantOrphanReferenceSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int ScanStalenessHours { get; set; } = 24;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
