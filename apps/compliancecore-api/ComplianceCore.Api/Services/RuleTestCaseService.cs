using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RuleTestCaseService(
    ComplianceCoreDbContext db,
    RuleCatalogService ruleCatalogService)
{
    public async Task<IReadOnlyList<RuleTestCaseResponse>> ListAsync(
        Guid tenantId,
        Guid rulePackId,
        CancellationToken cancellationToken = default)
    {
        var pack = await LoadPackAsync(tenantId, rulePackId, cancellationToken);
        var cases = await db.RuleTestCases.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RulePackId == rulePackId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return cases.Select(testCase => MapResponse(pack, testCase)).ToList();
    }

    public async Task<RuleTestCaseResponse> GetAsync(
        Guid tenantId,
        Guid rulePackId,
        Guid testCaseId,
        CancellationToken cancellationToken = default)
    {
        var pack = await LoadPackAsync(tenantId, rulePackId, cancellationToken);
        var testCase = await LoadCaseAsync(tenantId, rulePackId, testCaseId, cancellationToken);
        return MapResponse(pack, testCase);
    }

    public async Task<RuleTestCaseResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid rulePackId,
        CreateRuleTestCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var pack = await LoadPackAsync(tenantId, rulePackId, cancellationToken);
        var content = RequireContent(pack);
        var ruleKey = NormalizeKey(request.RuleKey, "Rule key");
        if (!content.Rules.Any(x => string.Equals(x.RuleKey, ruleKey, StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException("rule_test_cases.rule_not_found", "The selected rule was not found in the rule pack.", 404);
        }

        var normalizedExpectedResult = NormalizeExpectedResult(request.ExpectedResult);
        var now = DateTimeOffset.UtcNow;
        var entity = new RuleTestCase
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RulePackId = rulePackId,
            RuleKey = ruleKey,
            TestKey = NormalizeKey(request.TestKey, "Test key"),
            Label = NormalizeLabel(request.Label, "Label"),
            Description = NormalizeDescription(request.Description),
            ExpectedResult = normalizedExpectedResult,
            FactsJson = JsonSerializer.Serialize(request.Facts, RuleEvaluationJson.Options),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.RuleTestCases.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(pack, entity);
    }

    public async Task<RuleTestCaseResponse> PatchAsync(
        Guid tenantId,
        Guid rulePackId,
        Guid testCaseId,
        PatchRuleTestCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var pack = await LoadPackAsync(tenantId, rulePackId, cancellationToken);
        var entity = await LoadCaseAsync(tenantId, rulePackId, testCaseId, cancellationToken);
        var content = RequireContent(pack);

        if (request.RuleKey is not null)
        {
            var normalizedRuleKey = NormalizeKey(request.RuleKey, "Rule key");
            if (!content.Rules.Any(x => string.Equals(x.RuleKey, normalizedRuleKey, StringComparison.OrdinalIgnoreCase)))
            {
                throw new StlApiException("rule_test_cases.rule_not_found", "The selected rule was not found in the rule pack.", 404);
            }

            entity.RuleKey = normalizedRuleKey;
        }

        if (request.TestKey is not null)
        {
            entity.TestKey = NormalizeKey(request.TestKey, "Test key");
        }

        if (request.Label is not null)
        {
            entity.Label = NormalizeLabel(request.Label, "Label");
        }

        if (request.Description is not null)
        {
            entity.Description = NormalizeDescription(request.Description);
        }

        if (request.Facts is not null)
        {
            entity.FactsJson = JsonSerializer.Serialize(request.Facts, RuleEvaluationJson.Options);
        }

        if (request.ExpectedResult is not null)
        {
            entity.ExpectedResult = NormalizeExpectedResult(request.ExpectedResult);
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(pack, entity);
    }

    public async Task DeleteAsync(
        Guid tenantId,
        Guid rulePackId,
        Guid testCaseId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadCaseAsync(tenantId, rulePackId, testCaseId, cancellationToken);
        db.RuleTestCases.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RuleTestCaseRunResponse> RunAsync(
        Guid tenantId,
        Guid rulePackId,
        Guid testCaseId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadCaseAsync(tenantId, rulePackId, testCaseId, cancellationToken);
        var ruleId = BuildRuleId(entity.RulePackId, entity.RuleKey);
        var facts = DeserializeFacts(entity.FactsJson);
        var result = await ruleCatalogService.TestAsync(
            tenantId,
            ruleId,
            new RuleCatalogTestRequest(facts),
            cancellationToken);

        var passed = string.Equals(result.Result, entity.ExpectedResult, StringComparison.OrdinalIgnoreCase);
        return new RuleTestCaseRunResponse(
            entity.Id,
            ruleId,
            entity.ExpectedResult,
            result.Result,
            passed,
            passed
                ? "The saved test case matched the expected result."
                : $"Expected {entity.ExpectedResult} but the rule returned {result.Result}.",
            result.Evaluation,
            DateTimeOffset.UtcNow);
    }

    private async Task<RulePack> LoadPackAsync(Guid tenantId, Guid rulePackId, CancellationToken cancellationToken)
    {
        return await db.RulePacks.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,
            cancellationToken)
            ?? throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
    }

    private async Task<RuleTestCase> LoadCaseAsync(Guid tenantId, Guid rulePackId, Guid testCaseId, CancellationToken cancellationToken)
    {
        return await db.RuleTestCases.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.RulePackId == rulePackId && x.Id == testCaseId,
            cancellationToken)
            ?? throw new StlApiException("rule_test_cases.not_found", "Rule test case was not found.", 404);
    }

    private static RulePackContentBody RequireContent(RulePack pack)
    {
        if (string.IsNullOrWhiteSpace(pack.RuleContentJson))
        {
            throw new StlApiException("rules.empty", "Rule pack has no rule content.", 409);
        }

        return RuleEvaluator.ParseContent(pack.RuleContentJson);
    }

    private static RuleTestCaseResponse MapResponse(RulePack pack, RuleTestCase testCase) =>
        new(
            testCase.Id,
            pack.Id,
            pack.PackKey,
            pack.VersionNumber,
            pack.Status,
            BuildRuleId(pack.Id, testCase.RuleKey),
            testCase.RuleKey,
            testCase.TestKey,
            testCase.Label,
            testCase.Description,
            testCase.ExpectedResult,
            DeserializeFacts(testCase.FactsJson),
            testCase.CreatedAt,
            testCase.UpdatedAt);

    private static IReadOnlyDictionary<string, bool> DeserializeFacts(string json)
        => JsonSerializer.Deserialize<Dictionary<string, bool>>(json, RuleEvaluationJson.Options) ?? [];

    private static string BuildRuleId(Guid rulePackId, string ruleKey) => $"{rulePackId:N}:{ruleKey}";

    private static string NormalizeKey(string value, string label)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        if (trimmed.Length < 2 || trimmed.Length > 64)
        {
            throw new StlApiException("rule_test_cases.validation", $"{label} must be between 2 and 64 characters.", 400);
        }

        return trimmed;
    }

    private static string NormalizeLabel(string value, string label)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 160)
        {
            throw new StlApiException("rule_test_cases.validation", $"{label} must be between 2 and 160 characters.", 400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length > 1024)
        {
            throw new StlApiException("rule_test_cases.validation", "Description must be 1024 characters or fewer.", 400);
        }

        return trimmed;
    }

    private static string NormalizeExpectedResult(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!string.Equals(normalized, RuleEvaluationResults.Pass, StringComparison.Ordinal)
            && !string.Equals(normalized, RuleEvaluationResults.Fail, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "rule_test_cases.validation",
                "Expected result must be either 'pass' or 'fail'.",
                400);
        }

        return normalized;
    }
}
