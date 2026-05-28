using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class AvailabilitySnapshotService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<AvailabilitySnapshotResponse>> ListAsync(
        Guid tenantId,
        Guid? partVendorLinkId = null,
        Guid? partId = null,
        Guid? vendorPartyId = null,
        DateTimeOffset? asOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = BaseQuery(tenantId);

        if (partVendorLinkId is not null)
        {
            query = query.Where(x => x.PartVendorLinkId == partVendorLinkId);
        }

        if (partId is not null)
        {
            query = query.Where(x => x.PartVendorLink.PartId == partId);
        }

        if (vendorPartyId is not null)
        {
            query = query.Where(x => x.PartVendorLink.ExternalPartyId == vendorPartyId);
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

    public async Task<AvailabilitySnapshotResponse> GetAsync(
        Guid tenantId,
        Guid availabilitySnapshotId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, availabilitySnapshotId, cancellationToken);
        return Map(entity);
    }

    public async Task<AvailabilitySnapshotResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateAvailabilitySnapshotRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = await LoadVendorLinkAsync(tenantId, request.PartVendorLinkId, cancellationToken);
        var snapshotKey = NormalizeSnapshotKey(request.SnapshotKey);
        await EnsureUniqueKeyAsync(tenantId, snapshotKey, cancellationToken);

        var quantityAvailable = NormalizeQuantityAvailable(request.QuantityAvailable);
        var availabilityStatus = NormalizeAvailabilityStatus(request.AvailabilityStatus);
        var effectiveFrom = request.EffectiveFrom ?? DateTimeOffset.UtcNow;
        var source = NormalizeSource(request.Source);
        var notes = NormalizeNotes(request.Notes ?? string.Empty);

        await CloseOpenSnapshotsAsync(
            tenantId,
            link.Id,
            effectiveFrom,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new PartVendorAvailabilitySnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartVendorLinkId = link.Id,
            SnapshotKey = snapshotKey,
            QuantityAvailable = quantityAvailable,
            AvailabilityStatus = availabilityStatus,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            Source = source,
            Notes = notes,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PartVendorAvailabilitySnapshots.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "availability_snapshot.create",
            tenantId,
            actorUserId,
            "availability_snapshot",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<AvailabilitySnapshotResponse> CreateWorkerCaptureAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partVendorLinkId,
        decimal? quantityAvailable,
        string availabilityStatus,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken = default)
    {
        var link = await LoadVendorLinkAsync(tenantId, partVendorLinkId, cancellationToken);
        var normalizedQuantity = AvailabilitySnapshotCaptureRules.NormalizeOptionalQuantity(quantityAvailable);
        var normalizedStatus = AvailabilitySnapshotCaptureRules.NormalizeOptionalStatus(availabilityStatus)
            ?? AvailabilityStatuses.InStock;
        var snapshotKey = AvailabilitySnapshotCaptureRules.BuildWorkerSnapshotKey(partVendorLinkId, effectiveFrom);

        await CloseOpenSnapshotsAsync(
            tenantId,
            link.Id,
            effectiveFrom,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new PartVendorAvailabilitySnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartVendorLinkId = link.Id,
            SnapshotKey = snapshotKey,
            QuantityAvailable = normalizedQuantity,
            AvailabilityStatus = normalizedStatus,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            Source = SnapshotSources.VendorFeed,
            Notes = "Automated vendor catalog availability capture.",
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PartVendorAvailabilitySnapshots.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "availability_snapshot.worker_capture",
            tenantId,
            actorUserId,
            "availability_snapshot",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private IQueryable<PartVendorAvailabilitySnapshot> BaseQuery(Guid tenantId) =>
        db.PartVendorAvailabilitySnapshots
            .AsNoTracking()
            .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.Part)
            .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.ExternalParty)
            .Where(x => x.TenantId == tenantId);

    private async Task<PartVendorAvailabilitySnapshot> LoadAsync(
        Guid tenantId,
        Guid availabilitySnapshotId,
        CancellationToken cancellationToken)
    {
        var entity = await BaseQuery(tenantId)
            .FirstOrDefaultAsync(x => x.Id == availabilitySnapshotId, cancellationToken);

        return entity
            ?? throw new StlApiException(
                "availability_snapshot.not_found",
                "Availability snapshot was not found.",
                404);
    }

    private async Task<PartVendorLink> LoadVendorLinkAsync(
        Guid tenantId,
        Guid partVendorLinkId,
        CancellationToken cancellationToken)
    {
        var link = await db.PartVendorLinks
            .Include(x => x.Part)
            .Include(x => x.ExternalParty)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == partVendorLinkId,
                cancellationToken);

        return link
            ?? throw new StlApiException(
                "availability_snapshot.vendor_link.not_found",
                "Part vendor link was not found.",
                404);
    }

    private async Task CloseOpenSnapshotsAsync(
        Guid tenantId,
        Guid partVendorLinkId,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken)
    {
        var openSnapshots = await db.PartVendorAvailabilitySnapshots
            .Where(x =>
                x.TenantId == tenantId
                && x.PartVendorLinkId == partVendorLinkId
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
        var exists = await db.PartVendorAvailabilitySnapshots.AnyAsync(
            x => x.TenantId == tenantId && x.SnapshotKey == snapshotKey,
            cancellationToken);

        if (exists)
        {
            throw new StlApiException(
                "availability_snapshot.key.duplicate",
                "An availability snapshot with this key already exists.",
                409);
        }
    }

    private static AvailabilitySnapshotResponse Map(PartVendorAvailabilitySnapshot entity)
    {
        var link = entity.PartVendorLink;
        var part = link.Part;
        var party = link.ExternalParty;
        var now = DateTimeOffset.UtcNow;

        return new AvailabilitySnapshotResponse(
            entity.Id,
            entity.SnapshotKey,
            entity.PartVendorLinkId,
            part.Id,
            part.PartKey,
            part.DisplayName,
            party.Id,
            party.PartyKey,
            party.DisplayName,
            link.VendorPartNumber,
            entity.QuantityAvailable,
            entity.AvailabilityStatus,
            entity.EffectiveFrom,
            entity.EffectiveTo,
            entity.Source,
            entity.Notes,
            entity.EffectiveFrom <= now && (entity.EffectiveTo == null || entity.EffectiveTo > now),
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static string NormalizeSnapshotKey(string snapshotKey)
    {
        var normalized = snapshotKey.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "availability_snapshot.key.required",
                "Snapshot key is required.",
                400);
        }

        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "availability_snapshot.key.too_long",
                "Snapshot key cannot exceed 128 characters.",
                400);
        }

        return normalized;
    }

    private static decimal? NormalizeQuantityAvailable(decimal? quantityAvailable)
    {
        if (quantityAvailable is null)
        {
            return null;
        }

        if (quantityAvailable.Value < 0)
        {
            throw new StlApiException(
                "availability_snapshot.quantity.invalid",
                "Quantity available cannot be negative.",
                400);
        }

        return decimal.Round(quantityAvailable.Value, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeAvailabilityStatus(string availabilityStatus)
    {
        var normalized = availabilityStatus.Trim().ToLowerInvariant();

        return normalized switch
        {
            AvailabilityStatuses.InStock => AvailabilityStatuses.InStock,
            AvailabilityStatuses.Limited => AvailabilityStatuses.Limited,
            AvailabilityStatuses.Backorder => AvailabilityStatuses.Backorder,
            AvailabilityStatuses.OutOfStock => AvailabilityStatuses.OutOfStock,
            AvailabilityStatuses.Discontinued => AvailabilityStatuses.Discontinued,
            _ => throw new StlApiException(
                "availability_snapshot.status.invalid",
                "Availability status must be in_stock, limited, backorder, out_of_stock, or discontinued.",
                400),
        };
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
            SnapshotSources.VendorFeed => SnapshotSources.VendorFeed,
            _ => throw new StlApiException(
                "availability_snapshot.source.invalid",
                "Source must be manual, quote, contract, or vendor_feed.",
                400),
        };
    }

    private static string NormalizeNotes(string notes) =>
        notes.Length <= 1024 ? notes : notes[..1024];
}
