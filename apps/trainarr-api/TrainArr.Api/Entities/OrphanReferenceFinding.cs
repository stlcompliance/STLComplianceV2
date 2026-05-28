using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class OrphanReferenceFinding : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ReferenceKind { get; set; } = string.Empty;

    public string ReferenceKey { get; set; } = string.Empty;

    public string SampleSourceEntityType { get; set; } = string.Empty;

    public Guid SampleSourceEntityId { get; set; }

    public int AffectedSourceCount { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset FirstDetectedAt { get; set; }

    public DateTimeOffset LastDetectedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
