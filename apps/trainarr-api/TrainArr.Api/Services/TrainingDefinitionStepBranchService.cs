using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingDefinitionStepBranchService(
    TrainArrDbContext db,
    TrainingDefinitionStepService stepService,
    ITrainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedBranchTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        TrainingStepBranchTypes.QuizFailedRemediation,
        TrainingStepBranchTypes.StepVisibility,
    };

    public IReadOnlyList<TrainingStepBranchCatalogItemResponse> ListCatalog() =>
    [
        new(
            TrainingStepBranchTypes.QuizFailedRemediation,
            "Quiz failed → remediation step",
            "When this quiz step fails, unlock the configured remediation step for the trainee.",
            """{"targetStepKey":"remediation-review"}"""),
        new(
            TrainingStepBranchTypes.StepVisibility,
            "Conditional step visibility",
            "Show this step only when another step reaches the required status.",
            """{"dependsOnStepKey":"intro-quiz","requiredStatus":"failed"}"""),
    ];

    public async Task<IReadOnlyList<TrainingDefinitionStepBranchResponse>> ListForStepAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        Guid stepId,
        CancellationToken cancellationToken = default)
    {
        await stepService.GetDefinitionStepAsync(tenantId, trainingDefinitionId, stepId, cancellationToken);

        var rows = await db.TrainingDefinitionStepBranches
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingDefinitionStepId == stepId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.BranchKey)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<TrainingDefinitionStepBranchResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid trainingDefinitionId,
        Guid stepId,
        CreateTrainingDefinitionStepBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        var step = await stepService.GetDefinitionStepAsync(tenantId, trainingDefinitionId, stepId, cancellationToken);

        var branchKey = NormalizeBranchKey(request.BranchKey);
        var branchType = NormalizeBranchType(request.BranchType);
        var label = NormalizeLabel(request.Label);
        var configJson = await NormalizeConfigJsonAsync(
            tenantId,
            step,
            branchType,
            request.ConfigJson,
            cancellationToken);

        var duplicate = await db.TrainingDefinitionStepBranches.AnyAsync(
            x => x.TenantId == tenantId
                && x.TrainingDefinitionStepId == stepId
                && x.BranchKey == branchKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "training_step_branches.duplicate",
                "A branch with this key already exists on the training step.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TrainingDefinitionStepBranch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingDefinitionStepId = stepId,
            BranchKey = branchKey,
            BranchType = branchType,
            Label = label,
            ConfigJson = configJson,
            SortOrder = request.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingDefinitionStepBranches.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_definition_step_branch.create",
            tenantId,
            actorUserId,
            "training_definition_step_branch",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task DeleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid trainingDefinitionId,
        Guid stepId,
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        await stepService.GetDefinitionStepAsync(tenantId, trainingDefinitionId, stepId, cancellationToken);
        var entity = await LoadBranchAsync(tenantId, stepId, branchId, cancellationToken);
        db.TrainingDefinitionStepBranches.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_definition_step_branch.delete",
            tenantId,
            actorUserId,
            "training_definition_step_branch",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<TrainingDefinitionStepBranch>>> LoadBranchesByStepIdAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        CancellationToken cancellationToken = default)
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

    private async Task<TrainingDefinitionStepBranch> LoadBranchAsync(
        Guid tenantId,
        Guid stepId,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var entity = await db.TrainingDefinitionStepBranches.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.TrainingDefinitionStepId == stepId
                && x.Id == branchId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException(
                "training_step_branches.not_found",
                "Step branch was not found.",
                404);
        }

        return entity;
    }

    private async Task<string> NormalizeConfigJsonAsync(
        Guid tenantId,
        TrainingDefinitionStep step,
        string branchType,
        string configJson,
        CancellationToken cancellationToken)
    {
        var trimmed = string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson.Trim();
        try
        {
            using var document = JsonDocument.Parse(trimmed);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("Config must be a JSON object.");
            }
        }
        catch (JsonException)
        {
            throw new StlApiException(
                "training_step_branches.validation",
                "Config JSON must be a valid JSON object.",
                400);
        }

        if (string.Equals(branchType, TrainingStepBranchTypes.QuizFailedRemediation, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(step.StepType, TrainingStepTypes.Quiz, StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "training_step_branches.validation",
                    "Quiz failed remediation branches can only be attached to quiz steps.",
                    400);
            }

            if (!TryParseRemediationConfig(trimmed, out var targetStepKey))
            {
                throw new StlApiException(
                    "training_step_branches.validation",
                    "Quiz failed remediation branches must include targetStepKey in config JSON.",
                    400);
            }

            await EnsureStepKeyExistsAsync(tenantId, step.TrainingDefinitionId, targetStepKey!, cancellationToken);
            if (string.Equals(targetStepKey, step.StepKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "training_step_branches.validation",
                    "Remediation target step must differ from the source quiz step.",
                    400);
            }
        }

        if (string.Equals(branchType, TrainingStepBranchTypes.StepVisibility, StringComparison.OrdinalIgnoreCase))
        {
            if (!TrainingStepBranchEvaluator.TryParseVisibilityConfig(trimmed, out var dependsOnStepKey, out _))
            {
                throw new StlApiException(
                    "training_step_branches.validation",
                    "Visibility branches must include dependsOnStepKey and optional requiredStatus in config JSON.",
                    400);
            }

            await EnsureStepKeyExistsAsync(tenantId, step.TrainingDefinitionId, dependsOnStepKey, cancellationToken);
            if (string.Equals(dependsOnStepKey, step.StepKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "training_step_branches.validation",
                    "Visibility dependency cannot reference the same step.",
                    400);
            }
        }

        return trimmed;
    }

    private static bool TryParseRemediationConfig(string configJson, out string? targetStepKey)
    {
        targetStepKey = null;
        try
        {
            using var document = JsonDocument.Parse(configJson);
            if (!document.RootElement.TryGetProperty("targetStepKey", out var keyElement)
                || keyElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            targetStepKey = keyElement.GetString()!.Trim().ToLowerInvariant();
            return targetStepKey.Length >= 2;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async Task EnsureStepKeyExistsAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        string stepKey,
        CancellationToken cancellationToken)
    {
        var exists = await db.TrainingDefinitionSteps.AnyAsync(
            x => x.TenantId == tenantId
                && x.TrainingDefinitionId == trainingDefinitionId
                && x.StepKey == stepKey,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException(
                "training_step_branches.validation",
                $"Referenced step key '{stepKey}' was not found on this training definition.",
                400);
        }
    }

    private static TrainingDefinitionStepBranchResponse Map(TrainingDefinitionStepBranch entity) =>
        new(
            entity.Id,
            entity.TrainingDefinitionStepId,
            entity.BranchKey,
            entity.BranchType,
            entity.Label,
            entity.ConfigJson,
            entity.SortOrder,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeBranchKey(string branchKey)
    {
        var normalized = branchKey.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "training_step_branches.validation",
                "Branch key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeBranchType(string branchType)
    {
        var normalized = branchType.Trim().ToLowerInvariant();
        if (!AllowedBranchTypes.Contains(normalized))
        {
            throw new StlApiException(
                "training_step_branches.validation",
                $"Branch type must be one of: {string.Join(", ", AllowedBranchTypes.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeLabel(string label)
    {
        var trimmed = label.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "training_step_branches.validation",
                "Branch label must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }
}
