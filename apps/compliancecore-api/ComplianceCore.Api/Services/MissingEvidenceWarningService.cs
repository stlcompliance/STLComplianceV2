using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class MissingEvidenceWarningService(
    ComplianceCoreDbContext db,
    FactResolveService factResolveService,
    IComplianceCoreAuditService auditService)
{
    public async Task<EvaluateMissingEvidenceWarningsResponse> EvaluateAsync(
        Guid tenantId,
        Guid? actorUserId,
        EvaluateMissingEvidenceWarningsRequest request,
        CancellationToken cancellationToken = default)
    {
        var scopeKey = ResolveScopeKey(request);
        var context = request.Context ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var packs = await LoadPacksToAnalyzeAsync(tenantId, request.RulePackKey, cancellationToken);

        if (packs.Count == 0)
        {
            throw new StlApiException(
                "missing_evidence_warnings.no_packs",
                "No published rule packs with content are available to analyze.",
                404);
        }

        var mirrorFactKeys = await db.ProductFactMirrors
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ScopeKey == scopeKey)
            .Select(x => x.FactKey)
            .ToListAsync(cancellationToken);
        var mirrorFactKeySet = mirrorFactKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var now = DateTimeOffset.UtcNow;
        var run = new MissingEvidenceWarningRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ScopeKey = scopeKey,
            ActorUserId = actorUserId,
            EvaluatedAt = now,
        };

        var warnings = new List<MissingEvidenceWarning>();
        var highestSeverity = MissingEvidenceWarningSeverities.Low;

        foreach (var pack in packs)
        {
            var analysis = await AnalyzePackAsync(
                tenantId,
                pack,
                scopeKey,
                context,
                mirrorFactKeySet,
                cancellationToken);

            foreach (var item in analysis)
            {
                highestSeverity = MissingEvidenceWarningSeverities.Max(highestSeverity, item.Severity);
                warnings.Add(new MissingEvidenceWarning
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RunId = run.Id,
                    ScopeKey = scopeKey,
                    RulePackId = pack.Id,
                    PackKey = pack.PackKey,
                    FactKey = item.FactKey,
                    FactDefinitionId = item.FactDefinitionId,
                    WarningType = item.WarningType,
                    Severity = item.Severity,
                    ReasonCode = item.ReasonCode,
                    HasMirrorAtScope = item.HasMirrorAtScope,
                    IsRequiredInRule = item.IsRequiredInRule,
                    IsRequiredInCatalog = item.IsRequiredInCatalog,
                    Summary = item.Summary,
                    EvaluatedAt = now,
                });
            }
        }

        run.PacksAnalyzedCount = packs.Count;
        run.WarningsEmittedCount = warnings.Count;
        run.HighestSeverity = highestSeverity;

        db.MissingEvidenceWarningRuns.Add(run);
        db.MissingEvidenceWarnings.AddRange(warnings);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "missing_evidence_warnings.evaluate",
            tenantId,
            actorUserId,
            "missing_evidence_warning_run",
            run.Id.ToString(),
            "success",
            reasonCode: $"{scopeKey}:{warnings.Count}:{highestSeverity}",
            cancellationToken: cancellationToken);

        return new EvaluateMissingEvidenceWarningsResponse(
            run.Id,
            scopeKey,
            run.PacksAnalyzedCount,
            run.WarningsEmittedCount,
            run.HighestSeverity,
            mirrorFactKeys.Count,
            run.EvaluatedAt,
            warnings.Select(MapResponse).ToList());
    }

    public async Task<IReadOnlyList<MissingEvidenceWarningResponse>> ListWarningsAsync(
        Guid tenantId,
        string? scopeKey,
        string? rulePackKey,
        string? severity,
        Guid? runId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit ?? 50, 1, MissingEvidenceWarningRules.MaxListLimit);
        var query = db.MissingEvidenceWarnings.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(scopeKey))
        {
            query = query.Where(x => x.ScopeKey == ProductFactMirrorRules.NormalizeScopeKey(scopeKey));
        }

        if (!string.IsNullOrWhiteSpace(rulePackKey))
        {
            var normalizedPack = rulePackKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.PackKey == normalizedPack);
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            var normalizedSeverity = severity.Trim().ToLowerInvariant();
            query = query.Where(x => x.Severity == normalizedSeverity);
        }

        if (runId.HasValue)
        {
            query = query.Where(x => x.RunId == runId.Value);
        }
        else
        {
            var latestRunId = await db.MissingEvidenceWarningRuns
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.EvaluatedAt)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestRunId is null)
            {
                return [];
            }

            query = query.Where(x => x.RunId == latestRunId.Value);
        }

        var rows = await query
            .OrderBy(x => x.PackKey)
            .ThenBy(x => x.FactKey)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        return rows
            .OrderByDescending(x => MissingEvidenceWarningSeverities.Rank(x.Severity))
            .ThenBy(x => x.PackKey)
            .ThenBy(x => x.FactKey)
            .Select(MapResponse)
            .ToList();
    }

    public async Task<MissingEvidenceWarningSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var latestRunId = await db.MissingEvidenceWarningRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.EvaluatedAt)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestRunId is null)
        {
            return new MissingEvidenceWarningSummaryResponse(
                0,
                0,
                0,
                0,
                0,
                0,
                MissingEvidenceWarningSeverities.Low,
                null,
                DateTimeOffset.UtcNow);
        }

        var warnings = await db.MissingEvidenceWarnings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RunId == latestRunId.Value)
            .ToListAsync(cancellationToken);

        var lastEvaluated = await db.MissingEvidenceWarningRuns
            .AsNoTracking()
            .Where(x => x.Id == latestRunId.Value)
            .Select(x => x.EvaluatedAt)
            .FirstAsync(cancellationToken);

        var highest = warnings.Count == 0
            ? MissingEvidenceWarningSeverities.Low
            : warnings
                .Select(x => x.Severity)
                .Aggregate(MissingEvidenceWarningSeverities.Low, MissingEvidenceWarningSeverities.Max);

        return new MissingEvidenceWarningSummaryResponse(
            warnings.Count,
            warnings.Select(x => x.ScopeKey).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Low),
            warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Medium),
            warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.High),
            warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Critical),
            highest,
            lastEvaluated,
            DateTimeOffset.UtcNow);
    }

    private async Task<List<AnalyzedWarning>> AnalyzePackAsync(
        Guid tenantId,
        RulePack pack,
        string scopeKey,
        IReadOnlyDictionary<string, string> context,
        HashSet<string> mirrorFactKeySet,
        CancellationToken cancellationToken)
    {
        var ruleFactKeys = ExtractRuleFactKeys(pack.RuleContentJson);
        var catalogRequirements = await db.FactRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.IsActive
                && x.RulePackId == pack.Id)
            .Join(
                db.FactDefinitions.AsNoTracking(),
                requirement => requirement.FactDefinitionId,
                definition => definition.Id,
                (requirement, definition) => new CatalogFactRequirement(
                    definition.FactKey,
                    definition.Id,
                    requirement.IsRequired))
            .ToListAsync(cancellationToken);

        var catalogByKey = catalogRequirements
            .GroupBy(x => x.FactKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var allFactKeys = ruleFactKeys
            .Concat(catalogByKey.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (allFactKeys.Count == 0)
        {
            return [];
        }

        var resolveResponse = await factResolveService.ResolveAsync(
            new InternalResolveFactsRequest(tenantId, allFactKeys, context),
            sourceProductKey: "missing_evidence_warnings",
            cancellationToken);

        var unresolvedSet = resolveResponse.UnresolvedFactKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var definitionKeys = await db.FactDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.IsActive
                && allFactKeys.Contains(x.FactKey))
            .Select(x => x.FactKey)
            .ToListAsync(cancellationToken);
        var definitionKeySet = definitionKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var results = new List<AnalyzedWarning>();

        foreach (var factKey in allFactKeys)
        {
            var isInRule = ruleFactKeys.Contains(factKey, StringComparer.OrdinalIgnoreCase);
            catalogByKey.TryGetValue(factKey, out var catalog);
            var isInCatalog = catalog is not null;
            var isRequiredInCatalog = catalog?.IsRequired ?? false;
            var hasMirrorAtScope = mirrorFactKeySet.Contains(factKey);
            var isUnresolved = unresolvedSet.Contains(factKey);
            var hasDefinition = definitionKeySet.Contains(factKey);

            if (!MissingEvidenceWarningRules.ShouldEmitWarning(
                    isInRule,
                    isInCatalog,
                    isRequiredInCatalog,
                    hasMirrorAtScope,
                    isUnresolved,
                    hasDefinition))
            {
                continue;
            }

            var severity = MissingEvidenceWarningRules.DetermineSeverity(
                isInRule,
                isRequiredInCatalog,
                hasMirrorAtScope,
                isUnresolved,
                hasDefinition);
            var reasonCode = MissingEvidenceWarningRules.DetermineReasonCode(
                hasDefinition,
                hasMirrorAtScope,
                isUnresolved);
            var warningType = MissingEvidenceWarningRules.DetermineWarningType(isInRule, isInCatalog);

            results.Add(new AnalyzedWarning(
                factKey,
                catalog?.FactDefinitionId,
                warningType,
                severity,
                reasonCode,
                hasMirrorAtScope,
                isInRule,
                isRequiredInCatalog,
                MissingEvidenceWarningRules.BuildSummary(pack.PackKey, factKey, severity, reasonCode, hasMirrorAtScope)));
        }

        return results;
    }

    private static List<string> ExtractRuleFactKeys(string? ruleContentJson)
    {
        if (string.IsNullOrWhiteSpace(ruleContentJson))
        {
            return [];
        }

        var content = RuleEvaluator.ParseContent(ruleContentJson);
        return content.Rules
            .Select(rule => rule.FactKey.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<List<RulePack>> LoadPacksToAnalyzeAsync(
        Guid tenantId,
        string? rulePackKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(rulePackKey))
        {
            var normalized = rulePackKey.Trim().ToLowerInvariant();
            var pack = await db.RulePacks
                .Where(x => x.TenantId == tenantId
                    && x.PackKey == normalized
                    && x.IsActive
                    && x.Status == RulePackStatuses.Published
                    && x.RuleContentJson != null
                    && x.RuleContentJson != "")
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            return pack is null ? [] : [pack];
        }

        var packs = await db.RulePacks
            .Where(x => x.TenantId == tenantId
                && x.IsActive
                && x.Status == RulePackStatuses.Published
                && x.RuleContentJson != null
                && x.RuleContentJson != "")
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);

        return packs
            .GroupBy(x => x.PackKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(MissingEvidenceWarningRules.MaxPacksPerEvaluate)
            .ToList();
    }

    private static string ResolveScopeKey(EvaluateMissingEvidenceWarningsRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ScopeKey))
        {
            return ProductFactMirrorRules.NormalizeScopeKey(request.ScopeKey);
        }

        return ProductFactMirrorRules.ResolveScopeKeyFromContext(request.Context);
    }

    private static MissingEvidenceWarningResponse MapResponse(MissingEvidenceWarning entity) =>
        new(
            entity.Id,
            entity.RunId,
            entity.ScopeKey,
            entity.RulePackId,
            entity.PackKey,
            entity.FactKey,
            entity.FactDefinitionId,
            entity.WarningType,
            entity.Severity,
            entity.ReasonCode,
            entity.HasMirrorAtScope,
            entity.IsRequiredInRule,
            entity.IsRequiredInCatalog,
            entity.Summary,
            entity.EvaluatedAt);

    private sealed record CatalogFactRequirement(string FactKey, Guid FactDefinitionId, bool IsRequired);

    private sealed record AnalyzedWarning(
        string FactKey,
        Guid? FactDefinitionId,
        string WarningType,
        string Severity,
        string ReasonCode,
        bool HasMirrorAtScope,
        bool IsRequiredInRule,
        bool IsRequiredInCatalog,
        string Summary);
}
