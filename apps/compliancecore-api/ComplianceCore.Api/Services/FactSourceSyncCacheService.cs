using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public sealed class FactSourceSyncCacheService(ComplianceCoreDbContext db)
{
    public async Task<ResolvedFactValue?> TryResolveCachedAsync(
        Guid tenantId,
        FactDefinition definition,
        FactSource source,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(source.SourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal)
            && !string.Equals(source.SourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
        {
            return null;
        }

        var config = FactSourceApiSyncConfigParser.Parse(source.ConfigJson, "tenant");
        var scopeKey = ResolveScopeKey(config.ScopeKey, context);
        var productKey = string.IsNullOrWhiteSpace(source.ProductKey)
            ? "compliancecore"
            : source.ProductKey.Trim().ToLowerInvariant();
        var idempotencyKey = FactSourceSyncRules.BuildIdempotencyKey(source.Id);

        var mirror = await db.ProductFactMirrors
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.SourceProduct == productKey
                && x.FactKey == definition.FactKey
                && x.ScopeKey == scopeKey
                && x.IdempotencyKey == idempotencyKey)
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

    public async Task<bool> HasCachedValueAsync(
        Guid tenantId,
        FactDefinition definition,
        FactSource source,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(source.SourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal)
            && !string.Equals(source.SourceType, FactSourceTypes.ReportGenerated, StringComparison.Ordinal))
        {
            return false;
        }

        var config = FactSourceApiSyncConfigParser.Parse(source.ConfigJson, "tenant");
        var productKey = string.IsNullOrWhiteSpace(source.ProductKey)
            ? "compliancecore"
            : source.ProductKey.Trim().ToLowerInvariant();
        var idempotencyKey = FactSourceSyncRules.BuildIdempotencyKey(source.Id);

        return await db.ProductFactMirrors.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId
                && x.SourceProduct == productKey
                && x.FactKey == definition.FactKey
                && x.ScopeKey == config.ScopeKey
                && x.IdempotencyKey == idempotencyKey,
            cancellationToken);
    }

    public async Task<Guid> UpsertMirrorAsync(
        Guid tenantId,
        FactDefinition definition,
        FactSource source,
        FactSourceApiSyncConfig config,
        ProductFactApiFetchResult fetch,
        DateTimeOffset publishedAt,
        CancellationToken cancellationToken)
    {
        var productKey = string.IsNullOrWhiteSpace(source.ProductKey)
            ? "compliancecore"
            : source.ProductKey.Trim().ToLowerInvariant();
        var idempotencyKey = FactSourceSyncRules.BuildIdempotencyKey(source.Id);
        var now = DateTimeOffset.UtcNow;

        var mirror = await db.ProductFactMirrors
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.SourceProduct == productKey
                    && x.FactKey == definition.FactKey
                    && x.ScopeKey == config.ScopeKey
                    && x.IdempotencyKey == idempotencyKey,
                cancellationToken);

        if (mirror is null)
        {
            mirror = new ProductFactMirror
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SourceProduct = productKey,
                FactKey = definition.FactKey,
                ScopeKey = config.ScopeKey,
                CreatedAt = now,
            };
            db.ProductFactMirrors.Add(mirror);
        }

        mirror.ValueType = definition.ValueType;
        mirror.StringValue = fetch.StringValue;
        mirror.BooleanValue = fetch.BooleanValue;
        mirror.NumberValue = fetch.NumberValue;
        mirror.DateValue = ParseDate(fetch.DateValue);
        mirror.SourceEntityType = FactSourceSyncRules.SyncSourceEntityType;
        mirror.SourceEntityId = source.Id;
        mirror.SourceEventKind = FactSourceSyncRules.SyncSourceEventKind;
        mirror.SourcePublicationId = Guid.NewGuid();
        mirror.IdempotencyKey = idempotencyKey;
        mirror.PublishedAt = publishedAt;
        mirror.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        return mirror.Id;
    }

    private static string ResolveScopeKey(string configScopeKey, IReadOnlyDictionary<string, string>? context)
    {
        if (context is not null && context.TryGetValue("scope_key", out var explicitScope) && !string.IsNullOrWhiteSpace(explicitScope))
        {
            return ProductFactMirrorRules.NormalizeScopeKey(explicitScope);
        }

        return ProductFactMirrorRules.NormalizeScopeKey(configScopeKey);
    }

    private static DateOnly? ParseDate(string? raw) =>
        string.IsNullOrWhiteSpace(raw) || !DateOnly.TryParse(raw, out var date) ? null : date;
}
