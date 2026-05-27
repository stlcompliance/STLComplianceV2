namespace TrainArr.Api.Contracts;

public static class TrainingRulePackRequirementEntityTypes
{
    public const string TrainingDefinition = "training_definition";
    public const string TrainingProgram = "training_program";
}

public sealed record UpsertTrainingRulePackRequirementRequest(string RulePackKey);

public sealed record TrainingRulePackRequirementResponse(
    Guid RequirementId,
    string EntityType,
    Guid EntityId,
    string RulePackKey,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    TrainingRulePackMetadataResponse? Metadata = null);

public sealed record TrainingRulePackMetadataResponse(
    string Label,
    string Description,
    string RegulatoryProgramKey,
    string RegulatoryProgramLabel,
    int VersionNumber,
    string Status,
    bool IsActive);
