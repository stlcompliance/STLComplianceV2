namespace TrainArr.Api.Contracts;

public sealed record CreateTrainingDefinitionRequest(
    string DefinitionKey,
    string Name,
    string Description,
    string QualificationKey,
    string QualificationName);

public sealed record TrainingDefinitionResponse(
    Guid TrainingDefinitionId,
    string DefinitionKey,
    string Name,
    string Description,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset CreatedAt);
