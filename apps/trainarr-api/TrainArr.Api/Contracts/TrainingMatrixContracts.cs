namespace TrainArr.Api.Contracts;

public sealed record CreateTrainingMatrixEntryRequest(
    string ApplicabilityKey,
    string ApplicabilityLabel,
    Guid? TrainingProgramId,
    Guid? TrainingDefinitionId,
    string RequirementLevel,
    int SortOrder);

public sealed record UpdateTrainingMatrixEntryRequest(
    string ApplicabilityLabel,
    string RequirementLevel,
    int SortOrder);

public sealed record TrainingMatrixEntryResponse(
    Guid MatrixEntryId,
    string ApplicabilityKey,
    string ApplicabilityLabel,
    Guid? TrainingProgramId,
    string? TrainingProgramName,
    Guid? TrainingDefinitionId,
    string? TrainingDefinitionName,
    string RequirementLevel,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrainingMatrixViewResponse(
    IReadOnlyList<string> ApplicabilityKeys,
    IReadOnlyList<TrainingMatrixEntryResponse> Entries);
