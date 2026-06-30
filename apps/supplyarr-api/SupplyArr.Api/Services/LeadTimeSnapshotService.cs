using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class LeadTimeSnapshotService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<LeadTimeSnapshotResponse>> ListAsync(
        Guid tenantId,
        Guid? partVendorLinkId = null,
        Guid? partId = null,
        Guid? supplierId = null,
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

        if (supplierId is not null)
        {
            query = query.Where(x => x.PartVendorLink.ExternalPartyId == supplierId);
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

    public async Task<LeadTimeSnapshotResponse> GetAsync(
        Guid tenantId,
        Guid leadTimeSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, leadTimeSnapshotId, cancellationToken);
        return Map(entity);
    }

    public async Task<LeadTimeSnapshotResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateLeadTimeSnapshotRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = await LoadVendorLinkAsync(tenantId, request.PartVendorLinkId, cancellationToken);
        var snapshotKey = NormalizeSnapshotKey(request.SnapshotKey);
        await EnsureUniqueKeyAsync(tenantId, snapshotKey, cancellationToken);

        var leadTimeDays = NormalizeLeadTimeDays(request.LeadTimeDays);
        var effectiveFrom = request.EffectiveFrom ?? DateTimeOffset.UtcNow;
        var source = NormalizeSource(request.Source);
        var notes = NormalizeNotes(request.Notes ?? string.Empty);

        await CloseOpenSnapshotsAsync(
            tenantId,
            link.Id,
            effectiveFrom,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new PartVendorLeadTimeSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartVendorLinkId = link.Id,
            SnapshotKey = snapshotKey,
            LeadTimeDays = leadTimeDays,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            Source = source,
            Notes = notes,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PartVendorLeadTimeSnapshots.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "lead_time_snapshot.create",
            tenantId,
            actorUserId,
            "lead_time_snapshot",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<LeadTimeSnapshotResponse> CreateWorkerCaptureAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partVendorLinkId,
        int leadTimeDays,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken = default)
    {
        var link = await LoadVendorLinkAsync(tenantId, partVendorLinkId, cancellationToken);
        var normalizedLeadTimeDays = LeadTimeSnapshotCaptureRules.NormalizeLeadTimeDays(leadTimeDays);
        var snapshotKey = LeadTimeSnapshotCaptureRules.BuildWorkerSnapshotKey(partVendorLinkId, effectiveFrom);

        await CloseOpenSnapshotsAsync(
            tenantId,
            link.Id,
            effectiveFrom,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new PartVendorLeadTimeSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartVendorLinkId = link.Id,
            SnapshotKey = snapshotKey,
            LeadTimeDays = normalizedLeadTimeDays,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            Source = SnapshotSources.VendorFeed,
            Notes = "Automated vendor catalog lead-time capture.",
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PartVendorLeadTimeSnapshots.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "lead_time_snapshot.worker_capture",
            tenantId,
            actorUserId,
            "lead_time_snapshot",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private IQueryable<PartVendorLeadTimeSnapshot> BaseQuery(Guid tenantId) =>
        db.PartVendorLeadTimeSnapshots
            .AsNoTracking()
            .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.Part)
            .Include(x => x.PartVendorLink)
                .ThenInclude(x => x.ExternalParty)
                    .ThenInclude(x => x.ParentExternalParty)
            .Where(x => x.TenantId == tenantId);

    private async Task<PartVendorLeadTimeSnapshot> LoadAsync(
        Guid tenantId,
        Guid leadTimeSnapshotId,
        CancellationToken cancellationToken)
    {
        var entity = await BaseQuery(tenantId)
            .FirstOrDefaultAsync(x => x.Id == leadTimeSnapshotId, cancellationToken);

        return entity
            ?? throw new StlApiException(
                "lead_time_snapshot.not_found",
                "Lead-time snapshot was not found.",
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
                .ThenInclude(x => x.ParentExternalParty)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == partVendorLinkId,
                cancellationToken);

        return link
            ?? throw new StlApiException(
                "lead_time_snapshot.vendor_link.not_found",
                "Part vendor link was not found.",
                404);
    }

    private async Task CloseOpenSnapshotsAsync(
        Guid tenantId,
        Guid partVendorLinkId,
        DateTimeOffset effectiveFrom,
        CancellationToken cancellationToken)
    {
        var openSnapshots = await db.PartVendorLeadTimeSnapshots
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
        var exists = await db.PartVendorLeadTimeSnapshots.AnyAsync(
            x => x.TenantId == tenantId && x.SnapshotKey == snapshotKey,
            cancellationToken);

        if (exists)
        {
            throw new StlApiException(
                "lead_time_snapshot.key.duplicate",
                "A lead-time snapshot with this key already exists.",
                409);
        }
    }

    private static LeadTimeSnapshotResponse Map(PartVendorLeadTimeSnapshot entity)
    {
        var link = entity.PartVendorLink;
        var part = link.Part;
        var supplier = link.ExternalParty;
        var now = DateTimeOffset.UtcNow;
        var serviceTypes = ParseServiceTypes(supplier.ServiceTypesJson);

        return new LeadTimeSnapshotResponse(
            entity.Id,
            entity.SnapshotKey,
            entity.PartVendorLinkId,
            part.Id,
            part.PartKey,
            part.DisplayName,
            supplier.Id,
            supplier.PartyKey,
            supplier.DisplayName,
            supplier.ParentExternalPartyId,
            supplier.ParentExternalParty?.DisplayName,
            supplier.UnitKind,
            serviceTypes,
            link.VendorPartNumber,
            entity.LeadTimeDays,
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
                "lead_time_snapshot.key.required",
                "Snapshot key is required.",
                400);
        }

        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "lead_time_snapshot.key.too_long",
                "Snapshot key cannot exceed 128 characters.",
                400);
        }

        return normalized;
    }

    private static int NormalizeLeadTimeDays(int leadTimeDays)
    {
        if (leadTimeDays < 0)
        {
            throw new StlApiException(
                "lead_time_snapshot.lead_time_days.invalid",
                "Lead time days cannot be negative.",
                400);
        }

        return leadTimeDays;
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
            _ => throw new StlApiException(
                "lead_time_snapshot.source.invalid",
                "Source must be manual, quote, or contract.",
                400),
        };
    }

    private static string NormalizeNotes(string notes) =>
        notes.Length <= 1024 ? notes : notes[..1024];
}
