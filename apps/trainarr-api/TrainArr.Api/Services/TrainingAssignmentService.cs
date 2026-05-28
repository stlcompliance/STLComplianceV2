using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingAssignmentService(
    TrainArrDbContext db,
    TrainingDefinitionService definitionService,
    CertificationPublicationService publicationService,
    QualificationIssueService qualificationIssueService,
    TrainingNotificationEnqueueService notificationEnqueueService,
    ITrainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedAssignmentReasons = new(StringComparer.OrdinalIgnoreCase)
    {
        "manual",
        "incident_remediation"
    };

    public static readonly HashSet<string> ActiveAssignmentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "assigned",
        "in_progress"
    };

    public async Task<IReadOnlyList<TrainingAssignmentSummaryResponse>> ListAsync(
        Guid tenantId,
        Guid? staffarrPersonId,
        Guid? staffarrIncidentRemediationId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.TrainingAssignments
            .AsNoTracking()
            .Include(x => x.TrainingDefinition)
            .Where(x => x.TenantId == tenantId);

        if (staffarrPersonId is Guid personId)
        {
            query = query.Where(x => x.StaffarrPersonId == personId);
        }

        if (staffarrIncidentRemediationId is Guid remediationId)
        {
            query = query.Where(x => x.StaffarrIncidentRemediationId == remediationId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapSummary(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingAssignmentDetailResponse> GetAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await LoadAssignmentAsync(tenantId, assignmentId, cancellationToken);
        return MapDetail(assignment);
    }

    public async Task<TrainingAssignmentDetailResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateTrainingAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignmentReason = NormalizeAssignmentReason(request.AssignmentReason);
        var definition = await definitionService.GetActiveDefinitionAsync(
            tenantId,
            request.TrainingDefinitionId,
            cancellationToken);

        StaffarrIncidentRemediation? remediation = null;
        if (request.StaffarrIncidentRemediationId is Guid remediationId)
        {
            remediation = await db.StaffarrIncidentRemediations.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == remediationId,
                cancellationToken);
            if (remediation is null)
            {
                throw new StlApiException(
                    "assignments.remediation_not_found",
                    "StaffArr incident remediation was not found.",
                    404);
            }

            if (remediation.StaffarrPersonId != request.StaffarrPersonId)
            {
                throw new StlApiException(
                    "assignments.person_mismatch",
                    "Assignment person must match the remediation subject.",
                    400);
            }

            assignmentReason = "incident_remediation";

            var existingForRemediation = await db.TrainingAssignments.AnyAsync(
                x => x.TenantId == tenantId
                    && x.StaffarrIncidentRemediationId == remediationId
                    && ActiveAssignmentStatuses.Contains(x.Status),
                cancellationToken);
            if (existingForRemediation)
            {
                throw new StlApiException(
                    "assignments.remediation_already_assigned",
                    "An active assignment already exists for this remediation.",
                    409);
            }
        }
        else if (string.Equals(assignmentReason, "incident_remediation", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "assignments.remediation_required",
                "Incident remediation id is required for incident_remediation assignments.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        var assignment = new TrainingAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StaffarrPersonId = request.StaffarrPersonId,
            TrainingDefinitionId = definition.Id,
            StaffarrIncidentRemediationId = remediation?.Id,
            AssignmentReason = assignmentReason,
            Status = "assigned",
            DueAt = request.DueAt,
            AssignedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.TrainingAssignments.Add(assignment);

        var blockerMessage =
            $"Complete required training: {definition.Name} before returning to duty.";
        var publication = await publicationService.PublishTrainingBlockerAsync(
            new CreateCertificationPublicationRequest(
                tenantId,
                request.StaffarrPersonId,
                definition.QualificationKey,
                definition.QualificationName,
                "missing_assignment",
                blockerMessage,
                request.DueAt),
            cancellationToken);

        assignment.BlockerPublicationId = publication.PublicationId;

        if (remediation is not null)
        {
            remediation.Status = "assignment_created";
            remediation.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_assignment.create",
            tenantId,
            actorUserId,
            "training_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await notificationEnqueueService.TryEnqueueAsync(
            tenantId,
            TrainingNotificationEventKinds.AssignmentCreated,
            assignment.StaffarrPersonId,
            "training_assignment",
            assignment.Id,
            cancellationToken);

        var loaded = await LoadAssignmentAsync(tenantId, assignment.Id, cancellationToken);
        return MapDetail(loaded);
    }

    public async Task<CompleteTrainingAssignmentResponse> CompleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await LoadAssignmentAsync(tenantId, assignmentId, cancellationToken);
        if (!ActiveAssignmentStatuses.Contains(assignment.Status))
        {
            throw new StlApiException(
                "assignments.invalid_status",
                "Only assigned or in-progress assignments can be completed.",
                409);
        }

        if (!TrainingCompletionRequirements.AreMet(assignment))
        {
            var missing = TrainingCompletionRequirements.MissingRequirements(assignment);
            throw new StlApiException(
                "assignments.completion_requirements",
                $"Assignment cannot be completed until requirements are met: {string.Join(", ", missing)}.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        assignment.Status = "completed";
        assignment.CompletedAt = now;
        assignment.CompletedByUserId = actorUserId;
        assignment.UpdatedAt = now;

        if (assignment.BlockerPublicationId is Guid publicationId)
        {
            await publicationService.ClearTrainingBlockerAsync(
                tenantId,
                assignment.StaffarrPersonId,
                publicationId,
                cancellationToken);
        }

        var qualificationIssue = await qualificationIssueService.IssueOnAssignmentCompletionAsync(
            assignment,
            actorUserId,
            cancellationToken);

        if (assignment.StaffarrIncidentRemediation is not null)
        {
            assignment.StaffarrIncidentRemediation.Status = "completed";
            assignment.StaffarrIncidentRemediation.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_assignment.complete",
            tenantId,
            actorUserId,
            "training_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new CompleteTrainingAssignmentResponse(
            assignment.Id,
            assignment.Status,
            assignment.CompletedAt!.Value,
            assignment.BlockerPublicationId,
            qualificationIssue);
    }

    public async Task<TrainingAssignment> LoadAssignmentEntityAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken = default) =>
        await LoadAssignmentAsync(tenantId, assignmentId, cancellationToken);

    private async Task<TrainingAssignment> LoadAssignmentAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var assignment = await db.TrainingAssignments
            .Include(x => x.TrainingDefinition)
            .Include(x => x.StaffarrIncidentRemediation)
            .Include(x => x.EvidenceRecords)
            .Include(x => x.Evaluation)
            .Include(x => x.Signoffs)
            .Include(x => x.QualificationIssue)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assignmentId, cancellationToken);
        if (assignment is null)
        {
            throw new StlApiException("assignments.not_found", "Training assignment was not found.", 404);
        }

        return assignment;
    }

    private static TrainingAssignmentSummaryResponse MapSummary(TrainingAssignment entity) =>
        new(
            entity.Id,
            entity.StaffarrPersonId,
            entity.TrainingDefinitionId,
            entity.TrainingDefinition.Name,
            entity.TrainingDefinition.QualificationKey,
            entity.StaffarrIncidentRemediationId,
            entity.AssignmentReason,
            entity.Status,
            entity.DueAt,
            entity.CreatedAt);

    private static TrainingAssignmentDetailResponse MapDetail(TrainingAssignment entity) =>
        new(
            entity.Id,
            entity.StaffarrPersonId,
            entity.TrainingDefinitionId,
            entity.TrainingDefinition.Name,
            entity.TrainingDefinition.DefinitionKey,
            entity.TrainingDefinition.QualificationKey,
            entity.TrainingDefinition.QualificationName,
            entity.StaffarrIncidentRemediationId,
            entity.AssignmentReason,
            entity.Status,
            entity.DueAt,
            entity.AssignedByUserId,
            entity.BlockerPublicationId,
            entity.CompletedAt,
            entity.CompletedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.EvidenceRecords.Count,
            MapEvaluation(entity.Evaluation),
            entity.Signoffs
                .OrderBy(x => x.SignoffRole)
                .Select(MapSignoff)
                .ToList(),
            TrainingCompletionRequirements.AreMet(entity),
            MapQualificationIssue(entity.QualificationIssue));

    private static QualificationIssueResponse? MapQualificationIssue(QualificationIssue? entity) =>
        entity is null
            ? null
            : new QualificationIssueResponse(
                entity.Id,
                entity.TrainingAssignmentId,
                entity.StaffarrPersonId,
                entity.QualificationKey,
                entity.QualificationName,
                entity.GrantPublicationId,
                entity.Status,
                entity.IssuedAt,
                entity.ExpiresAt,
                entity.StatusChangedAt,
                entity.LifecycleReason,
                entity.LifecyclePublicationId);

    private static TrainingEvaluationResponse? MapEvaluation(TrainingEvaluation? entity) =>
        entity is null
            ? null
            : new TrainingEvaluationResponse(
                entity.Id,
                entity.TrainingAssignmentId,
                entity.Result,
                entity.Score,
                entity.Notes,
                entity.EvaluatorUserId,
                entity.EvaluatedAt);

    private static TrainingSignoffResponse MapSignoff(TrainingSignoff entity) =>
        new(
            entity.Id,
            entity.TrainingAssignmentId,
            entity.SignoffRole,
            entity.SignedByUserId,
            entity.Notes,
            entity.SignedAt);

    private static string NormalizeAssignmentReason(string assignmentReason)
    {
        var normalized = assignmentReason.Trim().ToLowerInvariant();
        if (!AllowedAssignmentReasons.Contains(normalized))
        {
            throw new StlApiException(
                "assignments.validation",
                $"Assignment reason must be one of: {string.Join(", ", AllowedAssignmentReasons.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }
}
