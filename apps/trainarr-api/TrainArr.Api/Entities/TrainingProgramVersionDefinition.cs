namespace TrainArr.Api.Entities;

public sealed class TrainingProgramVersionDefinition
{
    public Guid TrainingProgramVersionId { get; set; }

    public TrainingProgramVersion TrainingProgramVersion { get; set; } = null!;

    public Guid TrainingDefinitionId { get; set; }

    public TrainingDefinition TrainingDefinition { get; set; } = null!;

    public int SortOrder { get; set; }
}
