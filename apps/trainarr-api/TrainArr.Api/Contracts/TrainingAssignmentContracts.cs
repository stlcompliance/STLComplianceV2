namespace TrainArr.Api.Contracts;



public sealed record CreateTrainingAssignmentRequest(

    Guid StaffarrPersonId,

    Guid TrainingDefinitionId,

    Guid? StaffarrIncidentRemediationId,

    string AssignmentReason,

    DateTimeOffset? DueAt,

    Guid? AuthorizationQualificationCheckId = null);



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

    Guid? StaffarrAcknowledgementRequestId,

    string? StaffarrAcknowledgementStatus,

    DateTimeOffset? StaffarrAcknowledgementAt,

    bool StaffarrAcknowledgementRequired,

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

public static class TrainingAssignmentLaborTypes
{
    public const string Delivery = "delivery";
    public const string Preparation = "preparation";
    public const string Review = "review";
    public const string Administration = "administration";
    public const string Travel = "travel";
}

public sealed record CreateTrainingAssignmentLaborEntryRequest(
    string LaborTypeKey,
    decimal HoursWorked,
    decimal CostPerHour,
    string? Notes = null);

public sealed record TrainingAssignmentLaborEntryResponse(
    Guid LaborEntryId,
    Guid TrainingAssignmentId,
    string LaborTypeKey,
    decimal HoursWorked,
    decimal CostPerHour,
    decimal TotalCost,
    string? Notes,
    Guid? LoggedByUserId,
    DateTimeOffset LoggedAt,
    DateTimeOffset CreatedAt);


