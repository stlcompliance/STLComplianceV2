namespace TrainArr.Api.Contracts;



public sealed record CreateTrainingAssignmentRequest(

    Guid StaffarrPersonId,

    Guid TrainingDefinitionId,

    Guid? StaffarrIncidentRemediationId,

    string AssignmentReason,

    DateTimeOffset? DueAt);



public sealed record TrainingAssignmentSummaryResponse(

    Guid AssignmentId,

    Guid StaffarrPersonId,

    Guid TrainingDefinitionId,

    string TrainingDefinitionName,

    string QualificationKey,

    Guid? StaffarrIncidentRemediationId,

    Guid? SourceQualificationIssueId,

    string AssignmentReason,

    string Status,

    DateTimeOffset? DueAt,

    DateTimeOffset CreatedAt);



public sealed record TrainingAssignmentDetailResponse(

    Guid AssignmentId,

    Guid StaffarrPersonId,

    Guid TrainingDefinitionId,

    string TrainingDefinitionName,

    string TrainingDefinitionKey,

    string QualificationKey,

    string QualificationName,

    Guid? StaffarrIncidentRemediationId,

    Guid? SourceQualificationIssueId,

    string AssignmentReason,

    string Status,

    DateTimeOffset? DueAt,

    Guid? AssignedByUserId,

    Guid? BlockerPublicationId,

    DateTimeOffset? CompletedAt,

    Guid? CompletedByUserId,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt,

    int EvidenceCount,

    TrainingEvaluationResponse? Evaluation,

    IReadOnlyList<TrainingSignoffResponse> Signoffs,

    bool CompletionRequirementsMet,

    QualificationIssueResponse? QualificationIssue);



public sealed record CompleteTrainingAssignmentResponse(

    Guid AssignmentId,

    string Status,

    DateTimeOffset CompletedAt,

    Guid? BlockerPublicationId,

    QualificationIssueResponse QualificationIssue);


