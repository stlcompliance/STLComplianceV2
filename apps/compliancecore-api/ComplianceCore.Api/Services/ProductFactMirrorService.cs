using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public sealed class ProductFactMirrorService(ComplianceCoreDbContext db)
{
    public async Task<ResolvedFactValue?> TryResolveAsync(
        Guid tenantId,
        FactDefinition definition,
        FactSource source,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(source.SourceType, FactSourceTypes.ProductMirror, StringComparison.Ordinal))
        {
            return null;
        }

        var productKey = string.IsNullOrWhiteSpace(source.ProductKey)
            ? "supplyarr"
            : source.ProductKey.Trim().ToLowerInvariant();
        var scopeKey = ProductFactMirrorRules.ResolveScopeKeyFromContext(context);

        var mirror = await db.ProductFactMirrors
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.SourceProduct == productKey
                && x.FactKey == definition.FactKey
                && x.ScopeKey == scopeKey)
            .OrderByDescending(x => x.PublishedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (mirror is null || !ProductFactMirrorRules.TryToJsonElement(mirror, out var value))
        {
            return null;
        }

        return new ResolvedFactValue(
            definition.FactKey,
            definition.ValueType,
            value,
            source.SourceType,
            source.SourceKey,
            FromContext: false);
    }

    public async Task<bool> HasResolvableMirrorAsync(
        Guid tenantId,
        FactDefinition definition,
        FactSource source,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(source.SourceType, FactSourceTypes.ProductMirror, StringComparison.Ordinal))
        {
            return false;
        }

        var productKey = string.IsNullOrWhiteSpace(source.ProductKey)
            ? "supplyarr"
            : source.ProductKey.Trim().ToLowerInvariant();

        return await db.ProductFactMirrors.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId
                && x.SourceProduct == productKey
                && x.FactKey == definition.FactKey,
            cancellationToken);
    }
}
