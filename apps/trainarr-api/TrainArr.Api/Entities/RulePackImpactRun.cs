using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class RulePackImpactRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string RulePackKey { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public bool RequiresAttention { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
