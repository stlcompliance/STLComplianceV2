using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class FactResolveService(
    ComplianceCoreDbContext db,
    ProductFactMirrorService productFactMirrorService,
    FactSourceSyncCacheService factSourceSyncCacheService,
    IComplianceCoreAuditService auditService)
{
    public const string ResolveActionScope = "compliancecore.facts.resolve";

    public const string ValidateActionScope = "compliancecore.facts.validate";

    public async Task<InternalResolveFactsResponse> ResolveAsync(
        InternalResolveFactsRequest request,
        string? sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var factKeys = NormalizeFactKeys(request.FactKeys);
        if (factKeys.Count == 0)
        {
            throw new StlApiException("facts.resolve.validation", "At least one fact key is required.", 400);
        }

        var definitions = await LoadDefinitionsAsync(request.TenantId, factKeys, cancellationToken);
        var sourcesByDefinition = await LoadSourcesAsync(request.TenantId, definitions.Values.Select(x => x.Id).ToList(), cancellationToken);

        var resolved = new List<ResolvedFactValue>();
        var unresolved = new List<string>();

        foreach (var factKey in factKeys)
        {
            if (!definitions.TryGetValue(factKey, out var definition))
            {
                unresolved.Add(factKey);
                continue;
            }

            var sources = sourcesByDefinition.GetValueOrDefault(definition.Id) ?? [];
            var match = await TryResolveDefinitionAsync(
                request.TenantId,
                definition,
                sources,
                request.Context,
                cancellationToken);
            if (match is null)
            {
                unresolved.Add(factKey);
                continue;
            }

            resolved.Add(match);
        }

        await auditService.WriteAsync(
            "facts.resolve",
            request.TenantId,
            actorUserId: null,
            "internal_resolve",
            string.Join(',', factKeys),
            unresolved.Count == 0 ? "success" : "partial",
            reasonCode: sourceProductKey,
            cancellationToken: cancellationToken);

        return new InternalResolveFactsResponse(request.TenantId, resolved, unresolved);
    }

    public async Task<InternalValidateFactsResponse> ValidateAsync(
        InternalValidateFactsRequest request,
        string? sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var factKeys = NormalizeFactKeys(request.FactKeys);
        if (factKeys.Count == 0)
        {
            throw new StlApiException("facts.validate.validation", "At least one fact key is required.", 400);
        }

        var definitions = await LoadDefinitionsAsync(request.TenantId, factKeys, cancellationToken);
        var sourcesByDefinition = await LoadSourcesAsync(request.TenantId, definitions.Values.Select(x => x.Id).ToList(), cancellationToken);

        var results = new List<FactValidationItem>();

        foreach (var factKey in factKeys)
        {
            if (!definitions.TryGetValue(factKey, out var definition))
            {
                results.Add(new FactValidationItem(factKey, false, "Fact key is not registered in the catalog."));
                continue;
            }

            var sources = sourcesByDefinition.GetValueOrDefault(definition.Id) ?? [];
            if (sources.Count == 0)
            {
                results.Add(new FactValidationItem(factKey, false, "No fact sources are registered for this fact."));
                continue;
            }

            var resolvableSource = await FindResolvableSourceAsync(
                request.TenantId,
                definition,
                sources,
                cancellationToken);
            if (resolvableSource is not null)
            {
                results.Add(new FactValidationItem(factKey, true, null));
                continue;
            }

            var best = sources.First();
            results.Add(new FactValidationItem(
                factKey,
                false,
                FactResolver.DescribeValidationGap(definition, best)));
        }

        var isValid = results.All(x => x.CanResolve);

        await auditService.WriteAsync(
            "facts.validate",
            request.TenantId,
            actorUserId: null,
            "internal_validate",
            string.Join(',', factKeys),
            isValid ? "success" : "failed",
            reasonCode: sourceProductKey,
            cancellationToken: cancellationToken);

        return new InternalValidateFactsResponse(request.TenantId, isValid, results);
    }

    private async Task<ResolvedFactValue?> TryResolveDefinitionAsync(
        Guid tenantId,
        FactDefinition definition,
        IReadOnlyList<FactSource> sources,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken)
    {
        foreach (var source in sources.OrderBy(x => x.Priority))
        {
            if (string.Equals(source.SourceType, FactSourceTypes.ProductMirror, StringComparison.Ordinal))
            {
                var mirrorResolved = await productFactMirrorService.TryResolveAsync(
                    tenantId,
                    definition,
                    source,
                    context,
                    cancellationToken);
                if (mirrorResolved is not null)
                {
                    return mirrorResolved;
                }

                continue;
            }

            var resolved = FactResolver.TryResolveFromSource(definition, source, context);
            if (resolved is not null)
            {
                return resolved;
            }

            if (string.Equals(source.SourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal)
                || string.Equals(source.SourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
            {
                var cached = await factSourceSyncCacheService.TryResolveCachedAsync(
                    tenantId,
                    definition,
                    source,
                    context,
                    cancellationToken);
                if (cached is not null)
                {
                    return cached;
                }
            }
        }

        return null;
    }

    private async Task<FactSource?> FindResolvableSourceAsync(
        Guid tenantId,
        FactDefinition definition,
        IReadOnlyList<FactSource> sources,
        CancellationToken cancellationToken)
    {
        foreach (var source in sources.OrderBy(x => x.Priority))
        {
            if (string.Equals(source.SourceType, FactSourceTypes.ProductMirror, StringComparison.Ordinal))
            {
                if (await productFactMirrorService.HasResolvableMirrorAsync(
                        tenantId,
                        definition,
                        source,
                        cancellationToken))
                {
                    return source;
                }

                continue;
            }

            if (FactResolver.CanSourceResolve(definition, source, context: null))
            {
                return source;
            }

            if ((string.Equals(source.SourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal)
                    || string.Equals(source.SourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
                && await factSourceSyncCacheService.HasCachedValueAsync(
                    tenantId,
                    definition,
                    source,
                    cancellationToken))
            {
                return source;
            }
        }

        return null;
    }

    private async Task<Dictionary<string, FactDefinition>> LoadDefinitionsAsync(
        Guid tenantId,
        IReadOnlyList<string> factKeys,
        CancellationToken cancellationToken)
    {
        var definitions = await db.FactDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && factKeys.Contains(x.FactKey))
            .ToListAsync(cancellationToken);

        return definitions.ToDictionary(x => x.FactKey, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<Guid, List<FactSource>>> LoadSourcesAsync(
        Guid tenantId,
        IReadOnlyList<Guid> definitionIds,
        CancellationToken cancellationToken)
    {
        if (definitionIds.Count == 0)
        {
            return new Dictionary<Guid, List<FactSource>>();
        }

        var sources = await db.FactSources
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && definitionIds.Contains(x.FactDefinitionId))
            .OrderBy(x => x.Priority)
            .ToListAsync(cancellationToken);

        return sources
            .GroupBy(x => x.FactDefinitionId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    private static IReadOnlyList<string> NormalizeFactKeys(IReadOnlyList<string> factKeys)
    {
        if (factKeys is null || factKeys.Count == 0)
        {
            return Array.Empty<string>();
        }

        return factKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
