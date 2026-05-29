using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingMatrixEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ApplicabilityKey { get; set; } = string.Empty;

    public string ApplicabilityLabel { get; set; } = string.Empty;

    public Guid? TrainingProgramId { get; set; }

    public TrainingProgram? TrainingProgram { get; set; }

    public Guid? TrainingDefinitionId { get; set; }

    public TrainingDefinition? TrainingDefinition { get; set; }

    public string RequirementLevel { get; set; } = "required";

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
