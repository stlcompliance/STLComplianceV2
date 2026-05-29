using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public static class RuleEvaluator
{
    public const int CurrentSchemaVersion = 1;

    public static RulePackContentBody ParseContent(string ruleContentJson)
    {
        RulePackContentBody? content;
        try
        {
            content = JsonSerializer.Deserialize<RulePackContentBody>(
                ruleContentJson,
                RuleEvaluationJson.Options);
        }
        catch (JsonException ex)
        {
            throw new StlApiException(
                "rule_content.invalid_json",
                $"Rule content JSON is invalid: {ex.Message}",
                400);
        }

        if (content is null)
        {
            throw new StlApiException("rule_content.invalid_json", "Rule content JSON is empty.", 400);
        }

        ValidateContent(content);
        return content;
    }

    public static string SerializeContent(RulePackContentBody content)
    {
        ValidateContent(content);
        return JsonSerializer.Serialize(content, RuleEvaluationJson.Options);
    }

    public static (string OverallResult, IReadOnlyList<RuleEvaluationItemResponse> RuleResults) Evaluate(
        RulePackContentBody content,
        IReadOnlyDictionary<string, bool> facts)
    {
        var results = content.Rules
            .Select(rule => EvaluateRule(rule, facts))
            .ToList();

        var overallResult = string.Equals(content.Logic, "any", StringComparison.OrdinalIgnoreCase)
            ? results.Any(x => string.Equals(x.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
                ? RuleEvaluationResults.Pass
                : RuleEvaluationResults.Fail
            : results.All(x => string.Equals(x.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
                ? RuleEvaluationResults.Pass
                : RuleEvaluationResults.Fail;

        return (overallResult, results);
    }

    private static RuleEvaluationItemResponse EvaluateRule(
        RuleDefinitionDto rule,
        IReadOnlyDictionary<string, bool> facts)
    {
        if (!string.Equals(rule.Type, "fact_boolean", StringComparison.OrdinalIgnoreCase))
        {
            return new RuleEvaluationItemResponse(
                rule.RuleKey,
                rule.Label,
                RuleEvaluationResults.Fail,
                $"Unsupported rule type '{rule.Type}'.",
                rule.NonWaivable);
        }

        if (!facts.TryGetValue(rule.FactKey, out var actualValue))
        {
            return new RuleEvaluationItemResponse(
                rule.RuleKey,
                rule.Label,
                RuleEvaluationResults.Fail,
                $"Fact '{rule.FactKey}' was not provided.",
                rule.NonWaivable);
        }

        var passed = actualValue == rule.ExpectedValue;
        return new RuleEvaluationItemResponse(
            rule.RuleKey,
            rule.Label,
            passed ? RuleEvaluationResults.Pass : RuleEvaluationResults.Fail,
            passed
                ? $"Fact '{rule.FactKey}' matched expected value {rule.ExpectedValue.ToString().ToLowerInvariant()}."
                : $"Fact '{rule.FactKey}' was {actualValue.ToString().ToLowerInvariant()} but expected {rule.ExpectedValue.ToString().ToLowerInvariant()}.",
            rule.NonWaivable);
    }

    private static void ValidateContent(RulePackContentBody content)
    {
        if (content.SchemaVersion != CurrentSchemaVersion)
        {
            throw new StlApiException(
                "rule_content.unsupported_schema",
                $"Rule content schema version {content.SchemaVersion} is not supported.",
                400);
        }

        var logic = content.Logic.Trim().ToLowerInvariant();
        if (logic is not ("all" or "any"))
        {
            throw new StlApiException(
                "rule_content.validation",
                "Rule content logic must be 'all' or 'any'.",
                400);
        }

        if (content.Rules.Count == 0)
        {
            throw new StlApiException(
                "rule_content.validation",
                "Rule content must include at least one rule.",
                400);
        }

        var duplicateKeys = content.Rules
            .GroupBy(x => x.RuleKey.Trim().ToLowerInvariant())
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateKeys.Count > 0)
        {
            throw new StlApiException(
                "rule_content.validation",
                "Rule keys must be unique within rule content.",
                400);
        }

        foreach (var rule in content.Rules)
        {
            if (string.IsNullOrWhiteSpace(rule.RuleKey))
            {
                throw new StlApiException("rule_content.validation", "Each rule requires a rule key.", 400);
            }

            if (string.IsNullOrWhiteSpace(rule.FactKey))
            {
                throw new StlApiException("rule_content.validation", "Each rule requires a fact key.", 400);
            }

            if (!string.Equals(rule.Type, "fact_boolean", StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "rule_content.validation",
                    $"Unsupported rule type '{rule.Type}'. Only fact_boolean is supported.",
                    400);
            }
        }
    }
}

internal static class RuleEvaluationJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}
