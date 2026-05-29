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

        var now = DateTimeOffset.UtcNow;
        foreach (var step in steps)
        {
            if (existingStepIds.Contains(step.Id))
            {
                continue;
            }

            db.TrainingAssignmentStepProgress.Add(new TrainingAssignmentStepProgress
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TrainingAssignmentId = assignment.Id,
                TrainingDefinitionStepId = step.Id,
                Status = "pending",
                CreatedAt = now,
                UpdatedAt = now,
            });
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
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignment.Id)
            .Include(x => x.TrainingDefinitionStep)
            .OrderBy(x => x.TrainingDefinitionStep.SortOrder)
            .ThenBy(x => x.TrainingDefinitionStep.Name)
            .ToListAsync(cancellationToken);

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
            entity.QuizScorePercent,
            entity.ResponseJson,
            entity.CompletedAt);

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
