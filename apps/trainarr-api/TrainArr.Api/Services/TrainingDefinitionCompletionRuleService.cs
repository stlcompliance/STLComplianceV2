using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingDefinitionCompletionRuleService(
    TrainArrDbContext db,
    TrainingDefinitionService definitionService,
    ITrainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedRuleTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        TrainingCompletionRuleTypes.AllStepsRequired,
        TrainingCompletionRuleTypes.RequiredSignoff,
        TrainingCompletionRuleTypes.RequiredEvaluatorPass,
        TrainingCompletionRuleTypes.MinimumEvaluationScore,
    };

    public IReadOnlyList<TrainingCompletionRuleCatalogItemResponse> ListCatalog() =>
    [
        new(
            TrainingCompletionRuleTypes.AllStepsRequired,
            "All steps required",
            "Every configured training step must be completed before the assignment can finish.",
            "{}"),
        new(
            TrainingCompletionRuleTypes.RequiredSignoff,
            "Required signoff",
            "A specific signer role must submit a signoff.",
            """{"signoffRole":"trainer"}"""),
        new(
            TrainingCompletionRuleTypes.RequiredEvaluatorPass,
            "Passing evaluation",
            "An evaluator must record a passing evaluation result.",
            "{}"),
        new(
            TrainingCompletionRuleTypes.MinimumEvaluationScore,
            "Minimum evaluation score",
            "The current evaluation score must meet or exceed the configured threshold.",
            """{"minimumScorePercent":80}"""),
    ];

    public async Task<IReadOnlyList<TrainingDefinitionCompletionRuleResponse>> ListForDefinitionAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        CancellationToken cancellationToken = default)
    {
        await definitionService.GetActiveDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken);

        var rows = await db.TrainingDefinitionCompletionRules
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingDefinitionId == trainingDefinitionId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.RuleKey)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<TrainingDefinitionCompletionRuleResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid trainingDefinitionId,
        CreateTrainingDefinitionCompletionRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        await definitionService.GetActiveDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken);

        var ruleKey = NormalizeRuleKey(request.RuleKey);
        var ruleType = NormalizeRuleType(request.RuleType);
        var label = NormalizeLabel(request.Label);
        var configJson = NormalizeConfigJson(ruleType, request.ConfigJson);

        var duplicate = await db.TrainingDefinitionCompletionRules.AnyAsync(
            x => x.TenantId == tenantId
                && x.TrainingDefinitionId == trainingDefinitionId
                && x.RuleKey == ruleKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "training_completion_rules.duplicate",
                "A completion rule with this key already exists for the training definition.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TrainingDefinitionCompletionRule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingDefinitionId = trainingDefinitionId,
            RuleKey = ruleKey,
            RuleType = ruleType,
            Label = label,
            ConfigJson = configJson,
            SortOrder = request.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingDefinitionCompletionRules.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_completion_rule.create",
            tenantId,
            actorUserId,
            "training_definition_completion_rule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<TrainingDefinitionCompletionRuleResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid trainingDefinitionId,
        Guid completionRuleId,
        UpdateTrainingDefinitionCompletionRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadRuleAsync(tenantId, trainingDefinitionId, completionRuleId, cancellationToken);
        var ruleType = NormalizeRuleType(request.RuleType);

        entity.RuleType = ruleType;
        entity.Label = NormalizeLabel(request.Label);
        entity.ConfigJson = NormalizeConfigJson(ruleType, request.ConfigJson);
        entity.SortOrder = request.SortOrder;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_completion_rule.update",
            tenantId,
            actorUserId,
            "training_definition_completion_rule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task DeleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid trainingDefinitionId,
        Guid completionRuleId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadRuleAsync(tenantId, trainingDefinitionId, completionRuleId, cancellationToken);
        db.TrainingDefinitionCompletionRules.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_completion_rule.delete",
            tenantId,
            actorUserId,
            "training_definition_completion_rule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private async Task<TrainingDefinitionCompletionRule> LoadRuleAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        Guid completionRuleId,
        CancellationToken cancellationToken)
    {
        await definitionService.GetActiveDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken);

        var entity = await db.TrainingDefinitionCompletionRules.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.TrainingDefinitionId == trainingDefinitionId
                && x.Id == completionRuleId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException(
                "training_completion_rules.not_found",
                "Completion rule was not found.",
                404);
        }

        return entity;
    }

    private static TrainingDefinitionCompletionRuleResponse Map(TrainingDefinitionCompletionRule entity) =>
        new(
            entity.Id,
            entity.TrainingDefinitionId,
            entity.RuleKey,
            entity.RuleType,
            entity.Label,
            entity.ConfigJson,
            entity.SortOrder,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeRuleKey(string ruleKey)
    {
        var normalized = ruleKey.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "training_completion_rules.validation",
                "Rule key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRuleType(string ruleType)
    {
        var normalized = ruleType.Trim().ToLowerInvariant();
        if (!AllowedRuleTypes.Contains(normalized))
        {
            throw new StlApiException(
                "training_completion_rules.validation",
                $"Rule type must be one of: {string.Join(", ", AllowedRuleTypes.OrderBy(x => x))}.",
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
                "training_completion_rules.validation",
                "Rule label must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeConfigJson(string ruleType, string configJson)
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
                "training_completion_rules.validation",
                "Config JSON must be a valid JSON object.",
                400);
        }

        if (string.Equals(ruleType, TrainingCompletionRuleTypes.RequiredSignoff, StringComparison.OrdinalIgnoreCase))
        {
            using var document = JsonDocument.Parse(trimmed);
            if (!document.RootElement.TryGetProperty("signoffRole", out var roleElement)
                || roleElement.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(roleElement.GetString()))
            {
                throw new StlApiException(
                    "training_completion_rules.validation",
                    "Required signoff rules must include a signoffRole string in config JSON.",
                    400);
            }

            var role = roleElement.GetString()!.Trim().ToLowerInvariant();
            if (!TrainingCompletionRequirements.AllowedSignoffRoles.Contains(role))
            {
                throw new StlApiException(
                    "training_completion_rules.validation",
                    $"Signoff role must be one of: {string.Join(", ", TrainingCompletionRequirements.AllowedSignoffRoles.OrderBy(x => x))}.",
                    400);
            }
        }

        if (string.Equals(ruleType, TrainingCompletionRuleTypes.MinimumEvaluationScore, StringComparison.OrdinalIgnoreCase))
        {
            using var document = JsonDocument.Parse(trimmed);
            if (!document.RootElement.TryGetProperty("minimumScorePercent", out var scoreElement)
                || !scoreElement.TryGetDecimal(out var score)
                || score is < 0 or > 100)
            {
                throw new StlApiException(
                    "training_completion_rules.validation",
                    "Minimum evaluation score rules must include minimumScorePercent between 0 and 100.",
                    400);
            }
        }

        return trimmed;
    }
}
