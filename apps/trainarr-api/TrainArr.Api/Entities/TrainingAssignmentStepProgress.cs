using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingAssignmentStepProgress : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingAssignmentId { get; set; }

    public TrainingAssignment TrainingAssignment { get; set; } = null!;

    public Guid TrainingDefinitionStepId { get; set; }

    public TrainingDefinitionStep TrainingDefinitionStep { get; set; } = null!;

    public string Status { get; set; } = "pending";

    public int? QuizScorePercent { get; set; }

    public string? ResponseJson { get; set; }

    public Guid? CompletedByUserId { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
