using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingDefinitionStepService(
    TrainArrDbContext db,
    TrainingDefinitionService definitionService,
    ITrainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedStepTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        TrainingStepTypes.Content,
        TrainingStepTypes.Quiz,
        TrainingStepTypes.Practical,
    };

    public async Task<IReadOnlyList<TrainingDefinitionStepResponse>> ListForDefinitionAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        CancellationToken cancellationToken = default)
    {
        await definitionService.GetActiveDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken);
        return await db.TrainingDefinitionSteps
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingDefinitionId == trainingDefinitionId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => MapDefinitionStep(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingDefinitionStepResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid trainingDefinitionId,
        CreateTrainingDefinitionStepRequest request,
        CancellationToken cancellationToken = default)
    {
        await definitionService.GetActiveDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken);
        var stepKey = NormalizeStepKey(request.StepKey);
        var stepType = NormalizeStepType(request.StepType);
        ValidateConfigJson(stepType, request.ConfigJson);

        var duplicate = await db.TrainingDefinitionSteps.AnyAsync(
            x => x.TenantId == tenantId
                && x.TrainingDefinitionId == trainingDefinitionId
                && x.StepKey == stepKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "training_steps.duplicate",
                "A step with this key already exists on the training definition.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TrainingDefinitionStep
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingDefinitionId = trainingDefinitionId,
            StepKey = stepKey,
            Name = NormalizeName(request.Name),
            Description = NormalizeDescription(request.Description),
            StepType = stepType,
            ConfigJson = request.ConfigJson.Trim(),
            SortOrder = request.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingDefinitionSteps.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_definition_step.create",
            tenantId,
            actorUserId,
            "training_definition_step",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDefinitionStep(entity);
    }

    public async Task<TrainingDefinitionStepResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid trainingDefinitionId,
        Guid stepId,
        UpdateTrainingDefinitionStepRequest request,
        CancellationToken cancellationToken = default)
    {
        await definitionService.GetActiveDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken);
        var entity = await LoadDefinitionStepAsync(tenantId, trainingDefinitionId, stepId, cancellationToken);
        var stepType = NormalizeStepType(request.StepType);
        ValidateConfigJson(stepType, request.ConfigJson);

        entity.Name = NormalizeName(request.Name);
        entity.Description = NormalizeDescription(request.Description);
        entity.StepType = stepType;
        entity.ConfigJson = request.ConfigJson.Trim();
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_definition_step.update",
            tenantId,
            actorUserId,
            "training_definition_step",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDefinitionStep(entity);
    }

    public async Task DeleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid trainingDefinitionId,
        Guid stepId,
        CancellationToken cancellationToken = default)
    {
        await definitionService.GetActiveDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken);
        var entity = await LoadDefinitionStepAsync(tenantId, trainingDefinitionId, stepId, cancellationToken);
        db.TrainingDefinitionSteps.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_definition_step.delete",
            tenantId,
            actorUserId,
            "training_definition_step",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    public async Task EnsureAssignmentProgressAsync(
        Guid tenantId,
        TrainingAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        var steps = await db.TrainingDefinitionSteps
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingDefinitionId == assignment.TrainingDefinitionId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        if (steps.Count == 0)
        {
            return;
        }

        var existingStepIds = await db.TrainingAssignmentStepProgress
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignment.Id)
            .Select(x => x.TrainingDefinitionStepId)
            .ToListAsync(cancellationToken);

        var branchesByStepId = await LoadBranchesByStepIdAsync(tenantId, assignment.TrainingDefinitionId, cancellationToken);
        var remediationTargetKeys = BuildRemediationTargetKeys(steps, branchesByStepId);
        var progressByStepKey = await BuildProgressByStepKeyAsync(
            tenantId,
            assignment.Id,
            steps,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var step in steps)
        {
            if (existingStepIds.Contains(step.Id))
            {
                continue;
            }

            branchesByStepId.TryGetValue(step.Id, out var branches);
            var initialStatus = ResolveInitialStepStatus(
                step,
                branches ?? [],
                progressByStepKey,
                remediationTargetKeys);

            db.TrainingAssignmentStepProgress.Add(new TrainingAssignmentStepProgress
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TrainingAssignmentId = assignment.Id,
                TrainingDefinitionStepId = step.Id,
                Status = initialStatus,
                CreatedAt = now,
                UpdatedAt = now,
            });

            progressByStepKey[step.StepKey] = new TrainingAssignmentStepProgress
            {
                Status = initialStatus,
                TrainingDefinitionStep = step,
            };
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<TrainingAssignmentStepProgressResponse>> ListAssignmentProgressAsync(
        Guid tenantId,
        TrainingAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        await EnsureAssignmentProgressAsync(tenantId, assignment, cancellationToken);

        var rows = await db.TrainingAssignmentStepProgress
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignment.Id)
            .Include(x => x.TrainingDefinitionStep)
            .OrderBy(x => x.TrainingDefinitionStep.SortOrder)
            .ThenBy(x => x.TrainingDefinitionStep.Name)
            .ToListAsync(cancellationToken);

        await ApplyVisibilityStatusesAsync(tenantId, assignment, rows, cancellationToken);

        return rows.Select(MapProgress).ToList();
    }

    public async Task<TrainingAssignmentStepProgressResponse> SubmitAssignmentStepAsync(
        Guid tenantId,
        Guid actorUserId,
        TrainingAssignment assignment,
        Guid stepId,
        SubmitTrainingAssignmentStepRequest request,
        bool isEvaluator,
        CancellationToken cancellationToken = default)
    {
        await EnsureAssignmentProgressAsync(tenantId, assignment, cancellationToken);

        var progress = await db.TrainingAssignmentStepProgress
            .Include(x => x.TrainingDefinitionStep)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.TrainingAssignmentId == assignment.Id
                    && x.TrainingDefinitionStepId == stepId,
                cancellationToken);
        if (progress is null)
        {
            throw new StlApiException(
                "training_steps.not_found",
                "Training step progress was not found for this assignment.",
                404);
        }

        if (string.Equals(progress.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "training_steps.already_completed",
                "This training step is already completed.",
                409);
        }

        if (string.Equals(progress.Status, "hidden", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "training_steps.not_visible",
                "This training step is not available until its visibility conditions are met.",
                403);
        }

        var step = progress.TrainingDefinitionStep;
        var now = DateTimeOffset.UtcNow;
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        switch (step.StepType)
        {
            case TrainingStepTypes.Content:
                progress.Status = "completed";
                progress.ResponseJson = notes is null ? null : JsonSerializer.Serialize(new { notes });
                break;

            case TrainingStepTypes.Quiz:
                var score = ScoreQuiz(step.ConfigJson, request.SelectedOptionIndexes);
                progress.QuizScorePercent = score.ScorePercent;
                progress.Status = score.Passed ? "completed" : "failed";
                progress.ResponseJson = JsonSerializer.Serialize(new
                {
                    selectedOptionIndexes = request.SelectedOptionIndexes,
                    scorePercent = score.ScorePercent,
                    passed = score.Passed,
                    notes,
                });
                break;

            case TrainingStepTypes.Practical:
                if (!isEvaluator)
                {
                    throw new StlApiException(
                        "training_steps.evaluator_required",
                        "Practical steps require an evaluator submission.",
                        403);
                }

                var practicalResult = NormalizePracticalResult(request.PracticalResult);
                progress.Status = string.Equals(practicalResult, "pass", StringComparison.OrdinalIgnoreCase)
                    ? "completed"
                    : "failed";
                progress.ResponseJson = JsonSerializer.Serialize(new
                {
                    practicalResult,
                    notes,
                });
                break;

            default:
                throw new StlApiException(
                    "training_steps.unsupported_type",
                    "Unsupported training step type.",
                    400);
        }

        progress.CompletedByUserId = actorUserId;
        progress.CompletedAt = now;
        progress.UpdatedAt = now;

        if (string.Equals(progress.Status, "completed", StringComparison.OrdinalIgnoreCase)
            && string.Equals(assignment.Status, "assigned", StringComparison.OrdinalIgnoreCase))
        {
            assignment.Status = "in_progress";
            assignment.UpdatedAt = now;
        }

        if (string.Equals(progress.Status, "failed", StringComparison.OrdinalIgnoreCase)
            && string.Equals(step.StepType, TrainingStepTypes.Quiz, StringComparison.OrdinalIgnoreCase))
        {
            var allProgress = await db.TrainingAssignmentStepProgress
                .Include(x => x.TrainingDefinitionStep)
                .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignment.Id)
                .ToListAsync(cancellationToken);
            await ApplyQuizFailedRemediationAsync(tenantId, assignment, step, allProgress, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_assignment_step.submit",
            tenantId,
            actorUserId,
            "training_assignment_step",
            progress.Id.ToString(),
            progress.Status,
            cancellationToken: cancellationToken);

        return MapProgress(progress);
    }

    public async Task<TrainingDefinitionStep> GetDefinitionStepAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        Guid stepId,
        CancellationToken cancellationToken = default) =>
        await LoadDefinitionStepAsync(tenantId, trainingDefinitionId, stepId, cancellationToken);

    private async Task<TrainingDefinitionStep> LoadDefinitionStepAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        Guid stepId,
        CancellationToken cancellationToken)
    {
        var entity = await db.TrainingDefinitionSteps.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.TrainingDefinitionId == trainingDefinitionId
                && x.Id == stepId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException(
                "training_steps.not_found",
                "Training definition step was not found.",
                404);
        }

        return entity;
    }

    private static TrainingDefinitionStepResponse MapDefinitionStep(TrainingDefinitionStep entity) =>
        new(
            entity.Id,
            entity.TrainingDefinitionId,
            entity.StepKey,
            entity.Name,
            entity.Description,
            entity.StepType,
            entity.ConfigJson,
            entity.SortOrder,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static TrainingAssignmentStepProgressResponse MapProgress(TrainingAssignmentStepProgress entity) =>
        new(
            entity.Id,
            entity.TrainingAssignmentId,
            entity.TrainingDefinitionStepId,
            entity.TrainingDefinitionStep.StepKey,
            entity.TrainingDefinitionStep.Name,
            entity.TrainingDefinitionStep.Description,
            entity.TrainingDefinitionStep.StepType,
            entity.TrainingDefinitionStep.ConfigJson,
            entity.TrainingDefinitionStep.SortOrder,
            entity.Status,
            !string.Equals(entity.Status, "hidden", StringComparison.OrdinalIgnoreCase),
            entity.QuizScorePercent,
            entity.ResponseJson,
            entity.CompletedAt);

    private async Task ApplyVisibilityStatusesAsync(
        Guid tenantId,
        TrainingAssignment assignment,
        IReadOnlyList<TrainingAssignmentStepProgress> rows,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            return;
        }

        var branchesByStepId = await LoadBranchesByStepIdAsync(tenantId, assignment.TrainingDefinitionId, cancellationToken);
        var remediationTargetKeys = BuildRemediationTargetKeys(
            rows.Select(x => x.TrainingDefinitionStep).DistinctBy(x => x.Id).ToList(),
            branchesByStepId);
        var progressByStepKey = rows.ToDictionary(
            x => x.TrainingDefinitionStep.StepKey,
            x => x,
            StringComparer.OrdinalIgnoreCase);

        var changed = false;
        foreach (var row in rows)
        {
            if (remediationTargetKeys.Contains(row.TrainingDefinitionStep.StepKey))
            {
                continue;
            }

            branchesByStepId.TryGetValue(row.TrainingDefinitionStepId, out var branches);
            var shouldBeVisible = TrainingStepBranchEvaluator.IsStepVisible(
                row.TrainingDefinitionStep,
                branches ?? [],
                progressByStepKey);

            if (shouldBeVisible
                && string.Equals(row.Status, "hidden", StringComparison.OrdinalIgnoreCase))
            {
                row.Status = "pending";
                row.UpdatedAt = DateTimeOffset.UtcNow;
                changed = true;
            }
            else if (!shouldBeVisible
                && string.Equals(row.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                row.Status = "hidden";
                row.UpdatedAt = DateTimeOffset.UtcNow;
                changed = true;
            }
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ApplyQuizFailedRemediationAsync(
        Guid tenantId,
        TrainingAssignment assignment,
        TrainingDefinitionStep failedStep,
        IReadOnlyList<TrainingAssignmentStepProgress>? rows,
        CancellationToken cancellationToken)
    {
        var branchesByStepId = await LoadBranchesByStepIdAsync(tenantId, assignment.TrainingDefinitionId, cancellationToken);
        branchesByStepId.TryGetValue(failedStep.Id, out var branches);
        var targetKeys = TrainingStepBranchEvaluator.GetQuizFailedRemediationTargets(failedStep, branches ?? []);
        if (targetKeys.Count == 0)
        {
            return;
        }

        var progressRows = rows ?? await db.TrainingAssignmentStepProgress
            .Include(x => x.TrainingDefinitionStep)
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignment.Id)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var activated = false;
        foreach (var progress in progressRows)
        {
            if (!targetKeys.Contains(progress.TrainingDefinitionStep.StepKey, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(progress.Status, "hidden", StringComparison.OrdinalIgnoreCase)
                || string.Equals(progress.Status, "skipped", StringComparison.OrdinalIgnoreCase))
            {
                progress.Status = "pending";
                progress.UpdatedAt = now;
                activated = true;
            }
        }

        if (activated
            && !string.Equals(assignment.Status, "remediation_required", StringComparison.OrdinalIgnoreCase))
        {
            assignment.Status = "remediation_required";
            assignment.UpdatedAt = now;
        }
    }

    private static string ResolveInitialStepStatus(
        TrainingDefinitionStep step,
        IReadOnlyList<TrainingDefinitionStepBranch> branches,
        IReadOnlyDictionary<string, TrainingAssignmentStepProgress> progressByStepKey,
        IReadOnlySet<string> remediationTargetKeys)
    {
        if (remediationTargetKeys.Contains(step.StepKey))
        {
            return "hidden";
        }

        return TrainingStepBranchEvaluator.IsStepVisible(step, branches, progressByStepKey)
            ? "pending"
            : "hidden";
    }

    private static HashSet<string> BuildRemediationTargetKeys(
        IReadOnlyList<TrainingDefinitionStep> steps,
        IReadOnlyDictionary<Guid, IReadOnlyList<TrainingDefinitionStepBranch>> branchesByStepId)
    {
        var stepById = steps.ToDictionary(x => x.Id);
        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (stepId, branches) in branchesByStepId)
        {
            if (!stepById.TryGetValue(stepId, out var sourceStep))
            {
                continue;
            }

            foreach (var targetKey in TrainingStepBranchEvaluator.GetQuizFailedRemediationTargets(sourceStep, branches))
            {
                targets.Add(targetKey);
            }
        }

        return targets;
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<TrainingDefinitionStepBranch>>> LoadBranchesByStepIdAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        CancellationToken cancellationToken)
    {
        var stepIds = await db.TrainingDefinitionSteps
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingDefinitionId == trainingDefinitionId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (stepIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<TrainingDefinitionStepBranch>>();
        }

        var branches = await db.TrainingDefinitionStepBranches
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && stepIds.Contains(x.TrainingDefinitionStepId))
            .ToListAsync(cancellationToken);

        return branches
            .GroupBy(x => x.TrainingDefinitionStepId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<TrainingDefinitionStepBranch>)x.OrderBy(b => b.SortOrder).ToList());
    }

    private async Task<Dictionary<string, TrainingAssignmentStepProgress>> BuildProgressByStepKeyAsync(
        Guid tenantId,
        Guid assignmentId,
        IReadOnlyList<TrainingDefinitionStep> steps,
        CancellationToken cancellationToken)
    {
        var existing = await db.TrainingAssignmentStepProgress
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId)
            .Include(x => x.TrainingDefinitionStep)
            .ToListAsync(cancellationToken);

        var map = existing.ToDictionary(
            x => x.TrainingDefinitionStep.StepKey,
            x => x,
            StringComparer.OrdinalIgnoreCase);

        foreach (var step in steps)
        {
            if (!map.ContainsKey(step.StepKey))
            {
                map[step.StepKey] = new TrainingAssignmentStepProgress
                {
                    Status = "pending",
                    TrainingDefinitionStep = step,
                };
            }
        }

        return map;
    }

    private static (int ScorePercent, bool Passed) ScoreQuiz(
        string configJson,
        IReadOnlyList<int>? selectedOptionIndexes)
    {
        if (selectedOptionIndexes is null || selectedOptionIndexes.Count == 0)
        {
            throw new StlApiException(
                "training_steps.quiz_answers_required",
                "Quiz submission requires selected answers.",
                400);
        }

        using var document = JsonDocument.Parse(configJson);
        var root = document.RootElement;
        var passingScore = root.TryGetProperty("passingScorePercent", out var passingElement)
            && passingElement.TryGetInt32(out var parsedPassing)
            ? parsedPassing
            : 80;

        if (!root.TryGetProperty("questions", out var questionsElement) || questionsElement.ValueKind != JsonValueKind.Array)
        {
            throw new StlApiException(
                "training_steps.invalid_quiz_config",
                "Quiz step configuration is invalid.",
                400);
        }

        var questions = questionsElement.EnumerateArray().ToList();
        if (questions.Count == 0)
        {
            throw new StlApiException(
                "training_steps.invalid_quiz_config",
                "Quiz step must include at least one question.",
                400);
        }

        if (selectedOptionIndexes.Count != questions.Count)
        {
            throw new StlApiException(
                "training_steps.quiz_answer_count_mismatch",
                "Quiz submission must include one answer per question.",
                400);
        }

        var correctCount = 0;
        for (var index = 0; index < questions.Count; index++)
        {
            var question = questions[index];
            if (!question.TryGetProperty("correctOptionIndex", out var correctElement)
                || !correctElement.TryGetInt32(out var correctIndex))
            {
                throw new StlApiException(
                    "training_steps.invalid_quiz_config",
                    "Quiz question is missing correctOptionIndex.",
                    400);
            }

            if (selectedOptionIndexes[index] == correctIndex)
            {
                correctCount++;
            }
        }

        var scorePercent = (int)Math.Round(correctCount * 100.0 / questions.Count);
        return (scorePercent, scorePercent >= passingScore);
    }

    private static string NormalizeStepKey(string stepKey)
    {
        var normalized = stepKey.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "training_steps.validation",
                "Step key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();
        if (normalized.Length < 2 || normalized.Length > 128)
        {
            throw new StlApiException(
                "training_steps.validation",
                "Step name must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDescription(string description)
    {
        var normalized = description.Trim();
        if (normalized.Length < 2 || normalized.Length > 1024)
        {
            throw new StlApiException(
                "training_steps.validation",
                "Step description must be between 2 and 1024 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeStepType(string stepType)
    {
        var normalized = stepType.Trim().ToLowerInvariant();
        if (!AllowedStepTypes.Contains(normalized))
        {
            throw new StlApiException(
                "training_steps.validation",
                "Step type must be content, quiz, or practical.",
                400);
        }

        return normalized;
    }

    private static string NormalizePracticalResult(string? practicalResult)
    {
        var normalized = (practicalResult ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized is not ("pass" or "fail"))
        {
            throw new StlApiException(
                "training_steps.validation",
                "Practical result must be pass or fail.",
                400);
        }

        return normalized;
    }

    private static void ValidateConfigJson(string stepType, string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            throw new StlApiException(
                "training_steps.validation",
                "Step configuration is required.",
                400);
        }

        try
        {
            using var document = JsonDocument.Parse(configJson);
            if (stepType == TrainingStepTypes.Quiz)
            {
                var root = document.RootElement;
                if (!root.TryGetProperty("questions", out var questions)
                    || questions.ValueKind != JsonValueKind.Array
                    || questions.GetArrayLength() == 0)
                {
                    throw new StlApiException(
                        "training_steps.validation",
                        "Quiz steps require a questions array in configJson.",
                        400);
                }
            }
        }
        catch (JsonException)
        {
            throw new StlApiException(
                "training_steps.validation",
                "Step configuration must be valid JSON.",
                400);
        }
    }
}
