namespace TrainArr.Api.Entities;

public sealed class TrainingProgramDefinition
{
    public Guid TrainingProgramId { get; set; }

    public TrainingProgram TrainingProgram { get; set; } = null!;

    public Guid TrainingDefinitionId { get; set; }

    public TrainingDefinition TrainingDefinition { get; set; } = null!;

    public int SortOrder { get; set; }
}
