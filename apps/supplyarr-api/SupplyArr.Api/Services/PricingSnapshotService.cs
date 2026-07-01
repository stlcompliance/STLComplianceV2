using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PricingSnapshotService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<PricingSnapshotResponse>> ListAsync(
        Guid tenantId,
        Guid? partSupplierLinkId = null,
        Guid? partId = null,
        Guid? supplierId = null,
        DateTimeOffset? asOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = BaseQuery(tenantId);

        if (partSupplierLinkId is not null)
        {
            query = query.Where(x => x.PartSupplierLinkId == partSupplierLinkId);
        }

        if (partId is not null)
        {
            query = query.Where(x => x.PartSupplierLink.PartId == partId);
        }

        if (supplierId is not null)
        {
            query = query.Where(x => x.PartSupplierLink.SupplierId == supplierId);
        }

        if (asOf is not null)
        {
            var point = asOf.Value;
            query = query.Where(x =>
                x.EffectiveFrom <= point
                && (x.EffectiveTo == null || x.EffectiveTo > point));
        }

        var items = await query
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<PricingSnapshotResponse> GetAsync(
        Guid tenantId,
        Guid pricingSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, pricingSnapshotId, cancellationToken);
        return Map(entity);
    }

    public async Task<PricingSnapshotResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePricingSnapshotRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = await LoadSupplierLinkAsync(tenantId, request.PartSupplierLinkId, cancellationToken);
        var snapshotKey = NormalizeSnapshotKey(request.SnapshotKey);
        await EnsureUniqueKeyAsync(tenantId, snapshotKey, cancellationToken);

        var unitPrice = NormalizeUnitPrice(request.UnitPrice);
        var currencyCode = NormalizeCurrencyCode(request.CurrencyCode);
        decimal? minimumOrderQuantity = request.MinimumOrderQuantity is null
            ? null
            : NormalizeMinimumOrderQuantity(request.MinimumOrderQuantity.Value);
        var effectiveFrom = request.EffectiveFrom ?? DateTimeOffset.UtcNow;
        var source = NormalizeSource(request.Source);
        var notes = NormalizeNotes(request.Notes ?? string.Empty);

        await CloseOpenSnapshotsAsync(
            tenantId,
            link.Id,
            effectiveFrom,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new PartSupplierPricingSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartSupplierLinkId = link.Id,
            SnapshotKey = snapshotKey,
            UnitPrice = unitPrice,
            CurrencyCode = currencyCode,
            MinimumOrderQuantity = minimumOrderQuantity,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            Source = source,
            Notes = notes,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PartSupplierPricingSnapshots.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pricing_snapshot.create",
            tenantId,
            actorUserId,
            "pricing_snapshot",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PricingSnapshotResponse> CreateWorkerCaptureAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partSupplierLinkId,
        decimal unitPrice,
        string currencyCode,
        decimal? minimumOrderQuantity,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken = default)
    {
        var link = await LoadSupplierLinkAsync(tenantId, partSupplierLinkId, cancellationToken);
        var normalizedUnitPrice = NormalizeUnitPrice(unitPrice);
        var normalizedCurrencyCode = NormalizeCurrencyCode(currencyCode);
        decimal? normalizedMinimumOrderQuantity = minimumOrderQuantity is null
            ? null
            : NormalizeMinimumOrderQuantity(minimumOrderQuantity.Value);
        var snapshotKey = PriceSnapshotCaptureRules.BuildWorkerSnapshotKey(partSupplierLinkId, effectiveFrom);

        await CloseOpenSnapshotsAsync(
            tenantId,
            link.Id,
            effectiveFrom,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new PartSupplierPricingSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartSupplierLinkId = link.Id,
            SnapshotKey = snapshotKey,
            UnitPrice = normalizedUnitPrice,
            CurrencyCode = normalizedCurrencyCode,
            MinimumOrderQuantity = normalizedMinimumOrderQuantity,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            Source = SnapshotSources.SupplierFeed,
            Notes = "Automated supplier catalog price capture.",
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PartSupplierPricingSnapshots.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pricing_snapshot.worker_capture",
            tenantId,
            actorUserId,
            "pricing_snapshot",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private IQueryable<PartSupplierPricingSnapshot> BaseQuery(Guid tenantId) =>
        db.PartSupplierPricingSnapshots
            .AsNoTracking()
            .Include(x => x.PartSupplierLink)
                .ThenInclude(x => x.Part)
            .Include(x => x.PartSupplierLink)
                .ThenInclude(x => x.Supplier)
                    .ThenInclude(x => x.ParentSupplier)
            .Where(x => x.TenantId == tenantId);

    private async Task<PartSupplierPricingSnapshot> LoadAsync(
        Guid tenantId,
        Guid pricingSnapshotId,
        CancellationToken cancellationToken)
    {
        var entity = await BaseQuery(tenantId)
            .FirstOrDefaultAsync(x => x.Id == pricingSnapshotId, cancellationToken);

        return entity
            ?? throw new StlApiException(
                "pricing_snapshot.not_found",
                "Pricing snapshot was not found.",
                404);
    }

    private async Task<PartSupplierLink> LoadSupplierLinkAsync(
        Guid tenantId,
        Guid partSupplierLinkId,
        CancellationToken cancellationToken)
    {
        var link = await db.PartSupplierLinks
            .Include(x => x.Part)
            .Include(x => x.Supplier)
                .ThenInclude(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == partSupplierLinkId,
                cancellationToken);

        return link
            ?? throw new StlApiException(
                "pricing_snapshot.supplier_link.not_found",
                "Part supplier link was not found.",
                404);
    }

    private async Task CloseOpenSnapshotsAsync(
        Guid tenantId,
        Guid partSupplierLinkId,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken)
    {
        var openSnapshots = await db.PartSupplierPricingSnapshots
            .Where(x =>
                x.TenantId == tenantId
                && x.PartSupplierLinkId == partSupplierLinkId
                && x.EffectiveTo == null
                && x.EffectiveFrom < effectiveFrom)
            .ToListAsync(cancellationToken);

        if (openSnapshots.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var snapshot in openSnapshots)
        {
            snapshot.EffectiveTo = effectiveFrom;
            snapshot.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueKeyAsync(
        Guid tenantId,
        string snapshotKey,
        CancellationToken cancellationToken)
    {
        var exists = await db.PartSupplierPricingSnapshots.AnyAsync(
            x => x.TenantId == tenantId && x.SnapshotKey == snapshotKey,
            cancellationToken);

        if (exists)
        {
            throw new StlApiException(
                "pricing_snapshot.key.duplicate",
                "A pricing snapshot with this key already exists.",
                409);
        }
    }

    private static PricingSnapshotResponse Map(PartSupplierPricingSnapshot entity)
    {
        var link = entity.PartSupplierLink;
        var part = link.Part;
        var supplier = link.Supplier;
        var now = DateTimeOffset.UtcNow;
        var serviceTypes = ParseServiceTypes(supplier.ServiceTypesJson);

        return new PricingSnapshotResponse(
            entity.Id,
            entity.SnapshotKey,
            entity.PartSupplierLinkId,
            part.Id,
            part.PartKey,
            part.DisplayName,
            supplier.Id,
            supplier.SupplierKey,
            supplier.DisplayName,
            supplier.ParentSupplierId,
            supplier.ParentSupplier?.DisplayName,
            supplier.UnitKind,
            serviceTypes,
            link.SupplierPartNumber,
            entity.UnitPrice,
            entity.CurrencyCode,
            entity.MinimumOrderQuantity,
            entity.EffectiveFrom,
            entity.EffectiveTo,
            entity.Source,
            entity.Notes,
            entity.EffectiveFrom <= now && (entity.EffectiveTo == null || entity.EffectiveTo > now),
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static IReadOnlyList<string> ParseServiceTypes(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string NormalizeSnapshotKey(string snapshotKey)
    {
        var normalized = snapshotKey.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "pricing_snapshot.key.required",
                "Snapshot key is required.",
                400);
        }

        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "pricing_snapshot.key.too_long",
                "Snapshot key cannot exceed 128 characters.",
                400);
        }

        return normalized;
    }

    private static decimal NormalizeUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
        {
            throw new StlApiException(
                "pricing_snapshot.unit_price.invalid",
                "Unit price must be greater than zero.",
                400);
        }

        return decimal.Round(unitPrice, 4, MidpointRounding.AwayFromZero);
    }

    private static decimal NormalizeMinimumOrderQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException(
                "pricing_snapshot.minimum_order_quantity.invalid",
                "Minimum order quantity must be greater than zero.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeCurrencyCode(string? currencyCode)
    {
        var normalized = string.IsNullOrWhiteSpace(currencyCode)
            ? "USD"
            : currencyCode.Trim().ToUpperInvariant();

        if (normalized.Length != 3)
        {
            throw new StlApiException(
                "pricing_snapshot.currency.invalid",
                "Currency code must be a 3-letter ISO code.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSource(string? source)
    {
        var normalized = string.IsNullOrWhiteSpace(source)
            ? SnapshotSources.Manual
            : source.Trim().ToLowerInvariant();

        return normalized switch
        {
            SnapshotSources.Manual => SnapshotSources.Manual,
            SnapshotSources.Quote => SnapshotSources.Quote,
            SnapshotSources.Contract => SnapshotSources.Contract,
            SnapshotSources.SupplierFeed => SnapshotSources.SupplierFeed,
            _ => throw new StlApiException(
                "pricing_snapshot.source.invalid",
                "Source must be manual, quote, contract, or supplier_feed.",
                400),
        };
    }

    private static string NormalizeNotes(string notes) =>
        notes.Length <= 1024 ? notes : notes[..1024];
}

