using STLCompliance.Shared.Data;



namespace TrainArr.Api.Entities;



public sealed class TrainingAssignment : IHasTenant

{

    public Guid Id { get; set; }



    public Guid TenantId { get; set; }



    public Guid StaffarrPersonId { get; set; }



    public Guid TrainingDefinitionId { get; set; }



    public TrainingDefinition TrainingDefinition { get; set; } = null!;



    public Guid? StaffarrIncidentRemediationId { get; set; }



    public StaffarrIncidentRemediation? StaffarrIncidentRemediation { get; set; }



    public string AssignmentReason { get; set; } = "manual";



    public string Status { get; set; } = "assigned";



    public DateTimeOffset? DueAt { get; set; }



    public Guid? AssignedByUserId { get; set; }



    public Guid? BlockerPublicationId { get; set; }



    public DateTimeOffset? CompletedAt { get; set; }



    public Guid? CompletedByUserId { get; set; }



    public DateTimeOffset CreatedAt { get; set; }



    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<TrainingEvidence> EvidenceRecords { get; set; } = [];

    public TrainingEvaluation? Evaluation { get; set; }

    public ICollection<TrainingSignoff> Signoffs { get; set; } = [];

    public QualificationIssue? QualificationIssue { get; set; }

}


