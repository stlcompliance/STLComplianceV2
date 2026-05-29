namespace TrainArr.Api.Contracts;

public sealed record TrainingProgramVersionSummaryResponse(
    Guid ProgramVersionId,
    Guid ProgramId,
    int VersionNumber,
    string Status,
    string Name,
    int DefinitionCount,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt);

public sealed record TrainingProgramVersionDefinitionLinkResponse(
    Guid TrainingDefinitionId,
    string DefinitionKey,
    string Name,
    int SortOrder);

public sealed record TrainingProgramVersionDetailResponse(
    Guid ProgramVersionId,
    Guid ProgramId,
    string ProgramKey,
    int VersionNumber,
    string Status,
    string Name,
    string Description,
    IReadOnlyList<TrainingProgramVersionDefinitionLinkResponse> Definitions,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt);

public sealed record StartProgramRevisionRequest(
    Guid TrainingProgramId);
