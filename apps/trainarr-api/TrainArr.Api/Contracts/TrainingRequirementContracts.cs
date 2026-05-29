namespace TrainArr.Api.Contracts;

public sealed record CreateTrainingRequirementRequest(
    string RequirementKey,
    string Label,
    string? Description,
    string RequirementSource,
    string? SourceKey,
    Guid? TrainingProgramId,
    Guid? TrainingDefinitionId,
    Guid? ApplicabilityProfileId,
    string RequirementLevel,
    int SortOrder);

public sealed record UpdateTrainingRequirementRequest(
    string Label,
    string? Description,
    string RequirementLevel,
    int SortOrder,
    string Status);

public sealed record TrainingRequirementResponse(
    Guid RequirementId,
    string RequirementKey,
    string Label,
    string? Description,
    string RequirementSource,
    string? SourceKey,
    Guid? TrainingProgramId,
    string? TrainingProgramName,
    Guid? TrainingDefinitionId,
    string? TrainingDefinitionName,
    Guid? ApplicabilityProfileId,
    string? ApplicabilityProfileKey,
    string? ApplicabilityProfileLabel,
    string RequirementLevel,
    int SortOrder,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrainingRequirementBuilderViewResponse(
    IReadOnlyList<TrainingApplicabilityProfileResponse> Profiles,
    IReadOnlyList<TrainingRequirementResponse> Requirements);

public sealed record SyncRequirementToMatrixRequest(Guid RequirementId);

public sealed record SyncRequirementToMatrixResponse(
    Guid RequirementId,
    Guid MatrixEntryId,
    string ApplicabilityKey);
