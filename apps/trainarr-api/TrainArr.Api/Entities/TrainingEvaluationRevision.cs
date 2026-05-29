using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingEvaluationRevision : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingAssignmentId { get; set; }

    public Guid TrainingEvaluationId { get; set; }

    public string Result { get; set; } = "pass";

    public decimal? Score { get; set; }

    public string? Notes { get; set; }

    public Guid EvaluatorUserId { get; set; }

    public DateTimeOffset EvaluatedAt { get; set; }

    public DateTimeOffset SupersededAt { get; set; }

    public Guid SupersededByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
