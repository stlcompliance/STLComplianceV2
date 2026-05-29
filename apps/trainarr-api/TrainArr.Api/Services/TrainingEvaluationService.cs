using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingEvaluationService(
    TrainArrDbContext db,
    TrainingAssignmentService assignmentService,
    ITrainArrAuditService audit)
{
    public async Task<TrainingEvaluationResponse?> GetForAssignmentAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var evaluation = await db.TrainingEvaluations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId,
                cancellationToken);
        return evaluation is null ? null : Map(evaluation);
    }

    public async Task<IReadOnlyList<TrainingEvaluationResponse>> ListForAssignmentAsync(
        Guid tenantId,
        Guid? trainingAssignmentId,
        CancellationToken cancellationToken = default)
    {
        var query = db.TrainingEvaluations.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (trainingAssignmentId is Guid assignmentId)
        {
            query = query.Where(x => x.TrainingAssignmentId == assignmentId);
        }

        return await query
            .OrderByDescending(x => x.EvaluatedAt)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingEvaluationHistoryResponse> GetHistoryForAssignmentAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var current = await db.TrainingEvaluations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId,
                cancellationToken);

        var revisions = await db.TrainingEvaluationRevisions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId)
            .OrderByDescending(x => x.SupersededAt)
            .ToListAsync(cancellationToken);

        var items = new List<TrainingEvaluationHistoryItemResponse>();
        if (current is not null)
        {
            items.Add(new TrainingEvaluationHistoryItemResponse(
                current.Id,
                current.TrainingAssignmentId,
                current.Result,
                current.Score,
                current.Notes,
                current.EvaluatorUserId,
                current.EvaluatedAt,
                IsCurrent: true,
                SupersededAt: null));
        }

        items.AddRange(revisions.Select(revision => new TrainingEvaluationHistoryItemResponse(
            revision.Id,
            revision.TrainingAssignmentId,
            revision.Result,
            revision.Score,
            revision.Notes,
            revision.EvaluatorUserId,
            revision.EvaluatedAt,
            IsCurrent: false,
            SupersededAt: revision.SupersededAt)));

        return new TrainingEvaluationHistoryResponse(assignmentId, items);
    }

    public async Task<TrainingEvaluationReviewTimelineResponse> ListReviewTimelineAsync(
        Guid tenantId,
        Guid? staffarrPersonId,
        string? result,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit, 1, 100);
        var query =
            from evaluation in db.TrainingEvaluations.AsNoTracking()
            join assignment in db.TrainingAssignments.AsNoTracking()
                on evaluation.TrainingAssignmentId equals assignment.Id
            join definition in db.TrainingDefinitions.AsNoTracking()
                on assignment.TrainingDefinitionId equals definition.Id
            where evaluation.TenantId == tenantId
            select new { evaluation, assignment, definition };

        if (staffarrPersonId is Guid personId)
        {
            query = query.Where(x => x.assignment.StaffarrPersonId == personId);
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var normalizedResult = result.Trim().ToLowerInvariant();
            query = query.Where(x => x.evaluation.Result == normalizedResult);
        }

        var rows = await query
            .OrderByDescending(x => x.evaluation.EvaluatedAt)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        var items = rows.Select(row => new TrainingEvaluationReviewItemResponse(
            row.evaluation.Id,
            row.evaluation.TrainingAssignmentId,
            row.assignment.StaffarrPersonId,
            row.definition.Name,
            row.definition.QualificationName,
            row.assignment.Status,
            row.evaluation.Result,
            row.evaluation.Score,
            row.evaluation.Notes,
            row.evaluation.EvaluatorUserId,
            row.evaluation.EvaluatedAt)).ToList();

        return new TrainingEvaluationReviewTimelineResponse(items);
    }

    public async Task<TrainingEvaluationResponse> SubmitAsync(
        Guid tenantId,
        Guid actorUserId,
        SubmitTrainingEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await assignmentService.LoadAssignmentEntityAsync(
            tenantId,
            request.TrainingAssignmentId,
            cancellationToken);
        EnsureAssignmentOpen(assignment);

        var result = NormalizeResult(request.Result);
        var now = DateTimeOffset.UtcNow;
        var evaluation = assignment.Evaluation;
        if (evaluation is null)
        {
            evaluation = new TrainingEvaluation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TrainingAssignmentId = assignment.Id,
                CreatedAt = now
            };
            db.TrainingEvaluations.Add(evaluation);
            assignment.Evaluation = evaluation;
        }
        else
        {
            db.TrainingEvaluationRevisions.Add(new TrainingEvaluationRevision
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TrainingAssignmentId = assignment.Id,
                TrainingEvaluationId = evaluation.Id,
                Result = evaluation.Result,
                Score = evaluation.Score,
                Notes = evaluation.Notes,
                EvaluatorUserId = evaluation.EvaluatorUserId,
                EvaluatedAt = evaluation.EvaluatedAt,
                SupersededAt = now,
                SupersededByUserId = actorUserId,
                CreatedAt = now
            });
        }

        evaluation.Result = result;
        evaluation.Score = request.Score;
        evaluation.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        evaluation.EvaluatorUserId = actorUserId;
        evaluation.EvaluatedAt = now;
        evaluation.UpdatedAt = now;

        if (string.Equals(assignment.Status, "assigned", StringComparison.OrdinalIgnoreCase))
        {
            assignment.Status = "in_progress";
            assignment.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_evaluation.submit",
            tenantId,
            actorUserId,
            "training_evaluation",
            evaluation.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(evaluation);
    }

    private static void EnsureAssignmentOpen(TrainingAssignment assignment)
    {
        if (!TrainingAssignmentService.ActiveAssignmentStatuses.Contains(assignment.Status))
        {
            throw new StlApiException(
                "evaluations.assignment_closed",
                "Evaluations can only be submitted for assigned or in-progress assignments.",
                409);
        }
    }

    private static string NormalizeResult(string result)
    {
        var normalized = result.Trim().ToLowerInvariant();
        if (!TrainingCompletionRequirements.AllowedEvaluationResults.Contains(normalized))
        {
            throw new StlApiException(
                "evaluations.validation",
                $"Evaluation result must be one of: {string.Join(", ", TrainingCompletionRequirements.AllowedEvaluationResults.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static TrainingEvaluationResponse Map(TrainingEvaluation entity) =>
        new(
            entity.Id,
            entity.TrainingAssignmentId,
            entity.Result,
            entity.Score,
            entity.Notes,
            entity.EvaluatorUserId,
            entity.EvaluatedAt);
}
