using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class OrphanReferenceRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Outcome { get; set; } = string.Empty;

    public int ReferencesCheckedCount { get; set; }

    public int FindingsDetectedCount { get; set; }

    public int FindingsResolvedCount { get; set; }

    public int SkippedCount { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
