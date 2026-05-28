namespace TrainArr.Api.Contracts;



public sealed record CreateTrainingProgramRequest(

    string ProgramKey,

    string Name,

    string Description,

    IReadOnlyList<Guid> TrainingDefinitionIds);



public sealed record UpdateTrainingProgramRequest(

    string Name,

    string Description,

    string Status,

    IReadOnlyList<Guid> TrainingDefinitionIds);



public sealed record TrainingProgramSummaryResponse(

    Guid ProgramId,

    string ProgramKey,

    string Name,

    string Status,

    int DefinitionCount,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record TrainingProgramDefinitionLinkResponse(

    Guid TrainingDefinitionId,

    string DefinitionKey,

    string Name,

    int SortOrder);



public sealed record TrainingProgramDetailResponse(

    Guid ProgramId,

    string ProgramKey,

    string Name,

    string Description,

    string Status,

    IReadOnlyList<TrainingProgramDefinitionLinkResponse> Definitions,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);


