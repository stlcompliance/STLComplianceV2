using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingEvaluation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingAssignmentId { get; set; }

    public TrainingAssignment TrainingAssignment { get; set; } = null!;

    public string Result { get; set; } = "pass";

    public decimal? Score { get; set; }

    public string? Notes { get; set; }

    public Guid EvaluatorUserId { get; set; }

    public DateTimeOffset EvaluatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
