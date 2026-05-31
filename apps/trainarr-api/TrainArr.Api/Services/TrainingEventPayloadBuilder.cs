using TrainArr.Api.Contracts;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public static class TrainingEventPayloadBuilder
{
    public static TrainingDomainEventPayload ForAssignmentCreated(TrainingAssignment assignment) =>
        new(
            assignment.StaffarrPersonId,
            "training_assignment",
            assignment.Id,
            $"Training assignment created: {assignment.TrainingDefinition.Name} (due {assignment.DueAt:u}).",
            DateTimeOffset.UtcNow,
            assignment.TrainingDefinition.QualificationKey,
            assignment.TrainingDefinition.QualificationName,
            assignment.TrainingDefinition.Name,
            assignment.Id);

    public static TrainingDomainEventPayload ForAssignmentCompleted(TrainingAssignment assignment) =>
        new(
            assignment.StaffarrPersonId,
            "training_assignment",
            assignment.Id,
            $"Training assignment completed: {assignment.TrainingDefinition.Name}.",
            assignment.CompletedAt ?? DateTimeOffset.UtcNow,
            assignment.TrainingDefinition.QualificationKey,
            assignment.TrainingDefinition.QualificationName,
            assignment.TrainingDefinition.Name,
            assignment.Id);

    public static TrainingDomainEventPayload ForQualificationIssued(QualificationIssue issue, TrainingAssignment assignment) =>
        new(
            issue.StaffarrPersonId,
            "qualification_issue",
            issue.Id,
            $"Qualification issued: {issue.QualificationName}.",
            issue.IssuedAt,
            issue.QualificationKey,
            issue.QualificationName,
            assignment.TrainingDefinition.Name,
            assignment.Id,
            issue.Id);

    public static TrainingDomainEventPayload ForQualificationLifecycle(
        QualificationIssue issue,
        string action,
        DateTimeOffset occurredAt) =>
        new(
            issue.StaffarrPersonId,
            "qualification_issue",
            issue.Id,
            $"Qualification {action}: {issue.QualificationName}.",
            occurredAt,
            issue.QualificationKey,
            issue.QualificationName,
            QualificationIssueId: issue.Id);

    public static TrainingDomainEventPayload ForRemediationRequired(StaffarrIncidentRemediation remediation) =>
        new(
            remediation.StaffarrPersonId,
            "incident_remediation",
            remediation.Id,
            $"Remediation required after {remediation.SourceProduct} incident: {remediation.Title}.",
            remediation.OccurredAt);
}
