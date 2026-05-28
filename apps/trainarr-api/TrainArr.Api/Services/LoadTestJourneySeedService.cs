using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Operations.LoadTesting;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class LoadTestJourneySeedService(
    TrainArrDbContext db,
    ITrainArrAuditService auditService)
{
    public async Task<LoadTestJourneySeedResponse> EnsureSeededAsync(
        Guid tenantId,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var subjectPersonId = StlTrainArrLoadTestJourneySeedCatalog.SubjectPersonId;
        var qualificationKey = StlTrainArrLoadTestJourneySeedCatalog.QualificationKey;

        var existingIssue = await db.QualificationIssues
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.StaffarrPersonId == subjectPersonId
                && x.QualificationKey == qualificationKey
                && x.Status == "issued")
            .OrderByDescending(x => x.IssuedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingIssue is not null)
        {
            var existingAssignment = await db.TrainingAssignments
                .AsNoTracking()
                .FirstAsync(x => x.Id == existingIssue.TrainingAssignmentId, cancellationToken);

            return new LoadTestJourneySeedResponse(
                subjectPersonId,
                qualificationKey,
                existingAssignment.TrainingDefinitionId,
                TrainingDefinitionCreated: false,
                existingAssignment.Id,
                TrainingAssignmentCreated: false,
                existingIssue.Id,
                QualificationIssueCreated: false,
                QualificationGrantPublicationCreated: false);
        }

        var now = DateTimeOffset.UtcNow;
        var (definition, definitionCreated) = await EnsureTrainingDefinitionAsync(tenantId, now, cancellationToken);
        var (assignment, assignmentCreated) = await EnsureTrainingAssignmentAsync(
            tenantId,
            subjectPersonId,
            definition.Id,
            actorUserId,
            now,
            cancellationToken);
        var (publication, publicationCreated) = await EnsureQualificationGrantPublicationAsync(
            tenantId,
            subjectPersonId,
            qualificationKey,
            now,
            cancellationToken);
        var (issue, issueCreated) = await EnsureQualificationIssueAsync(
            tenantId,
            subjectPersonId,
            qualificationKey,
            assignment.Id,
            publication.Id,
            now,
            cancellationToken);

        await auditService.WriteAsync(
            "load_test_journey.seed",
            tenantId,
            actorUserId,
            "qualification_issue",
            issue.Id.ToString(),
            "success",
            reasonCode: qualificationKey,
            cancellationToken: cancellationToken);

        return new LoadTestJourneySeedResponse(
            subjectPersonId,
            qualificationKey,
            definition.Id,
            definitionCreated,
            assignment.Id,
            assignmentCreated,
            issue.Id,
            issueCreated,
            publicationCreated);
    }

    private async Task<(TrainingDefinition Definition, bool Created)> EnsureTrainingDefinitionAsync(
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await db.TrainingDefinitions
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.DefinitionKey == StlTrainArrLoadTestJourneySeedCatalog.JourneyDefinitionKey,
                cancellationToken);

        if (existing is not null)
        {
            return (existing, false);
        }

        var definition = new TrainingDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DefinitionKey = StlTrainArrLoadTestJourneySeedCatalog.JourneyDefinitionKey,
            Name = StlTrainArrLoadTestJourneySeedCatalog.JourneyDefinitionName,
            Description = "Idempotent load-test journey training definition for k6 qualification checks.",
            QualificationKey = StlTrainArrLoadTestJourneySeedCatalog.QualificationKey,
            QualificationName = StlTrainArrLoadTestJourneySeedCatalog.JourneyQualificationName,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingDefinitions.Add(definition);
        await db.SaveChangesAsync(cancellationToken);
        return (definition, true);
    }

    private async Task<(TrainingAssignment Assignment, bool Created)> EnsureTrainingAssignmentAsync(
        Guid tenantId,
        Guid subjectPersonId,
        Guid trainingDefinitionId,
        Guid? actorUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await db.TrainingAssignments
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.StaffarrPersonId == subjectPersonId
                    && x.TrainingDefinitionId == trainingDefinitionId
                    && x.AssignmentReason == StlTrainArrLoadTestJourneySeedCatalog.JourneyAssignmentReason,
                cancellationToken);

        if (existing is not null)
        {
            return (existing, false);
        }

        var assignment = new TrainingAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StaffarrPersonId = subjectPersonId,
            TrainingDefinitionId = trainingDefinitionId,
            AssignmentReason = StlTrainArrLoadTestJourneySeedCatalog.JourneyAssignmentReason,
            Status = "completed",
            AssignedByUserId = actorUserId,
            CompletedAt = now,
            CompletedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingAssignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);
        return (assignment, true);
    }

    private async Task<(CertificationPublication Publication, bool Created)> EnsureQualificationGrantPublicationAsync(
        Guid tenantId,
        Guid subjectPersonId,
        string qualificationKey,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await db.CertificationPublications
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.StaffarrPersonId == subjectPersonId
                    && x.QualificationKey == qualificationKey
                    && x.PublicationType == "qualification_grant"
                    && x.Message == StlTrainArrLoadTestJourneySeedCatalog.JourneyGrantPublicationMessage,
                cancellationToken);

        if (existing is not null)
        {
            return (existing, false);
        }

        var publication = new CertificationPublication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StaffarrPersonId = subjectPersonId,
            QualificationKey = qualificationKey,
            QualificationName = StlTrainArrLoadTestJourneySeedCatalog.JourneyQualificationName,
            PublicationType = "qualification_grant",
            BlockerType = string.Empty,
            Message = StlTrainArrLoadTestJourneySeedCatalog.JourneyGrantPublicationMessage,
            Status = "published",
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.CertificationPublications.Add(publication);
        await db.SaveChangesAsync(cancellationToken);
        return (publication, true);
    }

    private async Task<(QualificationIssue Issue, bool Created)> EnsureQualificationIssueAsync(
        Guid tenantId,
        Guid subjectPersonId,
        string qualificationKey,
        Guid trainingAssignmentId,
        Guid grantPublicationId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await db.QualificationIssues
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.TrainingAssignmentId == trainingAssignmentId,
                cancellationToken);

        if (existing is not null)
        {
            return (existing, false);
        }

        var issue = new QualificationIssue
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingAssignmentId = trainingAssignmentId,
            StaffarrPersonId = subjectPersonId,
            QualificationKey = qualificationKey,
            QualificationName = StlTrainArrLoadTestJourneySeedCatalog.JourneyQualificationName,
            GrantPublicationId = grantPublicationId,
            Status = "issued",
            IssuedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.QualificationIssues.Add(issue);
        await db.SaveChangesAsync(cancellationToken);
        return (issue, true);
    }
}
