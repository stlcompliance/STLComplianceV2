using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class EvidenceRetentionRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Outcome { get; set; } = string.Empty;

    public int EvidencePurgedCount { get; set; }

    public long BytesReclaimed { get; set; }

    public int SkippedCount { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
