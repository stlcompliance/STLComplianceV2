using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class RecertificationAssignmentRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid QualificationIssueId { get; set; }

    public Guid? TrainingAssignmentId { get; set; }

    public string Outcome { get; set; } = "assigned";

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
