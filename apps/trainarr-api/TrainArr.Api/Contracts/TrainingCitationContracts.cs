namespace TrainArr.Api.Contracts;

public static class TrainingCitationEntityTypes
{
    public const string TrainingDefinition = "training_definition";
    public const string TrainingProgram = "training_program";
    public const string TrainingAssignment = "training_assignment";
}

public sealed record AttachTrainingCitationRequest(
    Guid ComplianceCoreCitationId,
    string CitationKey,
    int? CitationVersion = null);

public sealed record TrainingCitationAttachmentResponse(
    Guid AttachmentId,
    string EntityType,
    Guid EntityId,
    Guid ComplianceCoreCitationId,
    string CitationKey,
    int CitationVersion,
    DateTimeOffset CreatedAt,
    TrainingCitationMetadataResponse? Metadata = null);

public sealed record TrainingCitationMetadataResponse(
    string Label,
    string SourceReference,
    string Description,
    string? RegulatoryProgramKey,
    string? RulePackKey,
    bool IsActive);
