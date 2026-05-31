using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RuleCatalogService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public const string RuleChangedEventAction = "compliancecore.rule.changed";

    public async Task<IReadOnlyList<RuleCatalogItemResponse>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var packs = await db.RulePacks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && !string.IsNullOrWhiteSpace(x.RuleContentJson))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var items = new List<RuleCatalogItemResponse>();
        foreach (var pack in packs)
        {
            var content = RuleEvaluator.ParseContent(pack.RuleContentJson!);
            items.AddRange(content.Rules.Select(rule => MapRule(pack, rule)));
        }

        return items;
    }

    public async Task<RuleCatalogItemResponse> GetAsync(Guid tenantId, string ruleId, CancellationToken cancellationToken = default)
    {
        var (packId, ruleKey) = ParseRuleId(ruleId);
        var pack = await LoadPackAsync(tenantId, packId, cancellationToken);
        var content = RequireContent(pack);
        var rule = content.Rules.FirstOrDefault(x => string.Equals(x.RuleKey, ruleKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new StlApiException("rules.not_found", "Rule was not found.", 404);
        return MapRule(pack, rule);
    }

    public async Task<RuleCatalogItemResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateRuleCatalogRequest request,
        CancellationToken cancellationToken = default)
    {
        var pack = await LoadPackAsync(tenantId, request.RulePackId, cancellationToken);
        var content = RequireContent(pack);

        if (content.Rules.Any(x => string.Equals(x.RuleKey, request.RuleKey, StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException("rules.duplicate", "A rule with this key already exists in the rule pack.", 409);
        }

        var rule = new RuleDefinitionDto(
            NormalizeKey(request.RuleKey, "Rule key"),
            NormalizeLabel(request.Label, "Label"),
            NormalizeType(request.Type),
            NormalizeKey(request.FactKey, "Fact key"),
            request.ExpectedValue,
            request.NonWaivable,
            request.RemediationRequired,
            request.ReviewRequired);

        var updated = new RulePackContentBody(content.SchemaVersion, content.Logic, [.. content.Rules, rule]);
        pack.RuleContentJson = RuleEvaluator.SerializeContent(updated);
        pack.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "rule.create",
            tenantId,
            actorUserId,
            "rule",
            BuildRuleId(pack.Id, rule.RuleKey),
            "success",
            cancellationToken: cancellationToken);

        await auditService.WriteAsync(
            RuleChangedEventAction,
            tenantId,
            actorUserId,
            "rule",
            BuildRuleId(pack.Id, rule.RuleKey),
            "created",
            reasonCode: rule.RuleKey,
            cancellationToken: cancellationToken);

        return MapRule(pack, rule);
    }

    public async Task<RuleCatalogItemResponse> PatchAsync(
        Guid tenantId,
        Guid? actorUserId,
        string ruleId,
        PatchRuleCatalogRequest request,
        CancellationToken cancellationToken = default)
    {
        var (packId, ruleKey) = ParseRuleId(ruleId);
        var pack = await LoadPackAsync(tenantId, packId, cancellationToken);
        var content = RequireContent(pack);

        var rules = content.Rules.ToList();
        var index = rules.FindIndex(x => string.Equals(x.RuleKey, ruleKey, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            throw new StlApiException("rules.not_found", "Rule was not found.", 404);
        }

        var current = rules[index];
        var updatedRule = new RuleDefinitionDto(
            current.RuleKey,
            request.Label is null ? current.Label : NormalizeLabel(request.Label, "Label"),
            request.Type is null ? current.Type : NormalizeType(request.Type),
            request.FactKey is null ? current.FactKey : NormalizeKey(request.FactKey, "Fact key"),
            request.ExpectedValue ?? current.ExpectedValue,
            request.NonWaivable ?? current.NonWaivable,
            request.RemediationRequired ?? current.RemediationRequired,
            request.ReviewRequired ?? current.ReviewRequired);

        rules[index] = updatedRule;
        var updatedContent = new RulePackContentBody(content.SchemaVersion, content.Logic, rules);
        pack.RuleContentJson = RuleEvaluator.SerializeContent(updatedContent);
        pack.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "rule.update",
            tenantId,
            actorUserId,
            "rule",
            BuildRuleId(pack.Id, updatedRule.RuleKey),
            "success",
            cancellationToken: cancellationToken);

        await auditService.WriteAsync(
            RuleChangedEventAction,
            tenantId,
            actorUserId,
            "rule",
            BuildRuleId(pack.Id, updatedRule.RuleKey),
            "updated",
            reasonCode: updatedRule.RuleKey,
            cancellationToken: cancellationToken);

        return MapRule(pack, updatedRule);
    }

    public async Task<RuleCatalogValidateResponse> ValidateAsync(Guid tenantId, string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var (packId, ruleKey) = ParseRuleId(ruleId);
            var pack = await LoadPackAsync(tenantId, packId, cancellationToken);
            var content = RequireContent(pack);
            _ = content.Rules.FirstOrDefault(x => string.Equals(x.RuleKey, ruleKey, StringComparison.OrdinalIgnoreCase))
                ?? throw new StlApiException("rules.not_found", "Rule was not found.", 404);
            return new RuleCatalogValidateResponse(true, Array.Empty<string>());
        }
        catch (StlApiException ex) when (ex.StatusCode != 404)
        {
            return new RuleCatalogValidateResponse(false, [ex.Message]);
        }
    }

    public async Task<RuleCatalogTestResponse> TestAsync(
        Guid tenantId,
        string ruleId,
        RuleCatalogTestRequest request,
        CancellationToken cancellationToken = default)
    {
        var (packId, ruleKey) = ParseRuleId(ruleId);
        var pack = await LoadPackAsync(tenantId, packId, cancellationToken);
        var content = RequireContent(pack);
        var rule = content.Rules.FirstOrDefault(x => string.Equals(x.RuleKey, ruleKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new StlApiException("rules.not_found", "Rule was not found.", 404);
        var singleRuleBody = new RulePackContentBody(content.SchemaVersion, "all", [rule]);
        var (_, results) = RuleEvaluator.Evaluate(singleRuleBody, request.Facts);
        var evaluation = results.Single();
        return new RuleCatalogTestResponse(evaluation.Result, evaluation.Message, evaluation);
    }

    public async Task<RuleCatalogUsageResponse> GetUsageAsync(Guid tenantId, string ruleId, CancellationToken cancellationToken = default)
    {
        var (packId, ruleKey) = ParseRuleId(ruleId);
        await LoadPackAsync(tenantId, packId, cancellationToken);

        var evaluationRuns = await db.RuleEvaluationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RulePackId == packId)
            .ToListAsync(cancellationToken);
        var evaluationRunCount = evaluationRuns.Count(run =>
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<IReadOnlyList<RuleEvaluationItemResponse>>(run.RuleResultsJson, RuleEvaluationJson.Options)
                    ?? [];
                return parsed.Any(item => string.Equals(item.RuleKey, ruleKey, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        });

        var findingCount = await db.ComplianceFindings.AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.RulePackId == packId && x.RuleKey == ruleKey, cancellationToken);
        var waiverCount = await db.ComplianceWaivers.AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.RulePackId == packId && x.RuleKey == ruleKey, cancellationToken);

        return new RuleCatalogUsageResponse(evaluationRunCount, findingCount, waiverCount);
    }

    public async Task<RuleCatalogHistoryResponse> GetHistoryAsync(Guid tenantId, string ruleId, CancellationToken cancellationToken = default)
    {
        var (packId, ruleKey) = ParseRuleId(ruleId);
        var pack = await LoadPackAsync(tenantId, packId, cancellationToken);
        var history = await db.RulePacks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PackKey == pack.PackKey && x.IsActive)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);

        var items = new List<RuleCatalogHistoryItemResponse>();
        foreach (var version in history)
        {
            var exists = false;
            if (!string.IsNullOrWhiteSpace(version.RuleContentJson))
            {
                var content = RuleEvaluator.ParseContent(version.RuleContentJson);
                exists = content.Rules.Any(x => string.Equals(x.RuleKey, ruleKey, StringComparison.OrdinalIgnoreCase));
            }

            items.Add(new RuleCatalogHistoryItemResponse(
                version.Id,
                version.PackKey,
                version.VersionNumber,
                version.Status,
                version.UpdatedAt,
                exists));
        }

        return new RuleCatalogHistoryResponse(ruleKey, items);
    }

    private async Task<RulePack> LoadPackAsync(Guid tenantId, Guid rulePackId, CancellationToken cancellationToken)
    {
        return await db.RulePacks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,
            cancellationToken)
            ?? throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
    }

    private static RulePackContentBody RequireContent(RulePack pack)
    {
        if (string.IsNullOrWhiteSpace(pack.RuleContentJson))
        {
            throw new StlApiException("rules.empty", "Rule pack has no rule content.", 409);
        }

        return RuleEvaluator.ParseContent(pack.RuleContentJson);
    }

    private static RuleCatalogItemResponse MapRule(RulePack pack, RuleDefinitionDto rule) =>
        new(
            BuildRuleId(pack.Id, rule.RuleKey),
            pack.Id,
            pack.PackKey,
            pack.Label,
            pack.VersionNumber,
            pack.Status,
            rule.RuleKey,
            rule.Label,
            rule.Type,
            rule.FactKey,
            rule.ExpectedValue,
            rule.NonWaivable,
            rule.RemediationRequired,
            rule.ReviewRequired,
            pack.UpdatedAt);

    private static string BuildRuleId(Guid rulePackId, string ruleKey) => $"{rulePackId:N}:{ruleKey}";

    private static (Guid RulePackId, string RuleKey) ParseRuleId(string ruleId)
    {
        var parts = ruleId.Split(':', 2);
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var rulePackId) || string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new StlApiException("rules.invalid_id", "Rule id format is invalid.", 400);
        }

        return (rulePackId, parts[1]);
    }

    private static string NormalizeKey(string value, string label)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        if (trimmed.Length < 2 || trimmed.Length > 64)
        {
            throw new StlApiException("rules.validation", $"{label} must be between 2 and 64 characters.", 400);
        }

        return trimmed;
    }

    private static string NormalizeLabel(string value, string label)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 160)
        {
            throw new StlApiException("rules.validation", $"{label} must be between 2 and 160 characters.", 400);
        }

        return trimmed;
    }

    private static string NormalizeType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!string.Equals(normalized, "fact_boolean", StringComparison.Ordinal))
        {
            throw new StlApiException("rules.validation", "Only fact_boolean rule type is supported.", 400);
        }

        return normalized;
    }
}
