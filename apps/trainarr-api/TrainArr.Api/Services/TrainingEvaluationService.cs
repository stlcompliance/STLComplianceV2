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
