using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainingCompletionEvaluationService(TrainArrDbContext db)
{
    public async Task<CompletionEvaluationResult> EvaluateAsync(
        TrainingAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        var rules = await db.TrainingDefinitionCompletionRules
            .AsNoTracking()
            .Where(x => x.TenantId == assignment.TenantId && x.TrainingDefinitionId == assignment.TrainingDefinitionId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        IReadOnlyList<TrainingAssignmentStepProgress>? stepProgress = null;
        if (rules.Any(x =>
                string.Equals(x.RuleType, Contracts.TrainingCompletionRuleTypes.AllStepsRequired, StringComparison.OrdinalIgnoreCase)))
        {
            stepProgress = await db.TrainingAssignmentStepProgress
                .AsNoTracking()
                .Where(x => x.TenantId == assignment.TenantId && x.TrainingAssignmentId == assignment.Id)
                .ToListAsync(cancellationToken);
        }

        return TrainingCompletionRuleEvaluator.Evaluate(assignment, rules, stepProgress);
    }
}
