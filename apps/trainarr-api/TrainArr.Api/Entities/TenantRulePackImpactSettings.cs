using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TenantRulePackImpactSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int StalenessHours { get; set; } = 24;

    public bool AutoUpdateRequirementBaselines { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
