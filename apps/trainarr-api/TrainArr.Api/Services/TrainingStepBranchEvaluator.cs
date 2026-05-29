using System.Text.Json;
using TrainArr.Api.Contracts;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public static class TrainingStepBranchEvaluator
{
    public static bool IsStepVisible(
        TrainingDefinitionStep step,
        IReadOnlyList<TrainingDefinitionStepBranch> branchesOnStep,
        IReadOnlyDictionary<string, TrainingAssignmentStepProgress> progressByStepKey)
    {
        var visibilityBranches = branchesOnStep
            .Where(x => string.Equals(x.BranchType, TrainingStepBranchTypes.StepVisibility, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (visibilityBranches.Count == 0)
        {
            return true;
        }

        foreach (var branch in visibilityBranches)
        {
            if (!TryParseVisibilityConfig(branch.ConfigJson, out var dependsOnStepKey, out var requiredStatus))
            {
                continue;
            }

            if (!progressByStepKey.TryGetValue(dependsOnStepKey, out var dependency))
            {
                return false;
            }

            if (!string.Equals(dependency.Status, requiredStatus, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public static IReadOnlyList<string> GetQuizFailedRemediationTargets(
        TrainingDefinitionStep step,
        IReadOnlyList<TrainingDefinitionStepBranch> branchesOnStep)
    {
        if (!string.Equals(step.StepType, TrainingStepTypes.Quiz, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        return branchesOnStep
            .Where(x => string.Equals(x.BranchType, TrainingStepBranchTypes.QuizFailedRemediation, StringComparison.OrdinalIgnoreCase))
            .Select(x => TryParseRemediationTargetKey(x.ConfigJson, out var target) ? target : null)
            .Where(x => x is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool TryParseVisibilityConfig(
        string configJson,
        out string dependsOnStepKey,
        out string requiredStatus)
    {
        dependsOnStepKey = string.Empty;
        requiredStatus = string.Empty;

        try
        {
            using var document = JsonDocument.Parse(configJson);
            var root = document.RootElement;
            if (!root.TryGetProperty("dependsOnStepKey", out var keyElement)
                || keyElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            dependsOnStepKey = keyElement.GetString()!.Trim().ToLowerInvariant();
            if (dependsOnStepKey.Length < 2)
            {
                return false;
            }

            requiredStatus = root.TryGetProperty("requiredStatus", out var statusElement)
                && statusElement.ValueKind == JsonValueKind.String
                ? statusElement.GetString()!.Trim().ToLowerInvariant()
                : "completed";

            return requiredStatus is "pending" or "completed" or "failed" or "hidden";
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryParseRemediationTargetKey(string configJson, out string targetStepKey)
    {
        targetStepKey = string.Empty;

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
}
