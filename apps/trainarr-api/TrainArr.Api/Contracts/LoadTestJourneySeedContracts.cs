namespace TrainArr.Api.Contracts;

public sealed record LoadTestJourneySeedResponse(
    Guid StaffarrPersonId,
    string QualificationKey,
    Guid TrainingDefinitionId,
    bool TrainingDefinitionCreated,
    Guid TrainingAssignmentId,
    bool TrainingAssignmentCreated,
    Guid QualificationIssueId,
    bool QualificationIssueCreated,
    bool QualificationGrantPublicationCreated);
