using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class QualificationCheckRecord : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid StaffarrPersonId { get; set; }

    public string QualificationKey { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? RulePackKey { get; set; }

    public Guid? TrainingDefinitionId { get; set; }

    public Guid? TrainingProgramId { get; set; }

    public Guid? ActorUserId { get; set; }

    public Guid? BatchId { get; set; }

    public DateTimeOffset CheckedAt { get; set; }
}
