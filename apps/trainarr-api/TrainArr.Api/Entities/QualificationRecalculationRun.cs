using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class QualificationRecalculationRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid QualificationIssueId { get; set; }

    public string Outcome { get; set; } = string.Empty;

    public string? CheckOutcome { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
