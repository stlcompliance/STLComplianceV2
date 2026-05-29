using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintainArr.Api.Services;

public sealed class TechnicianRefSyncService(
    MaintainArrDbContext db,
    TechnicianRefService technicianRefService,
    StaffArrPersonLookupClient staffarrLookup)
{
    public const string RefreshTechnicianRefsActionScope = "maintainarr.technician_refs.refresh";

    public static readonly TimeSpan DefaultStaleAfter = TimeSpan.FromHours(24);

    public async Task<PendingTechnicianRefRefreshResponse> ListPendingStaleAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        TimeSpan? staleAfter,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var threshold = asOf - (staleAfter ?? DefaultStaleAfter);
        var normalizedBatchSize = Math.Clamp(batchSize ?? 50, 1, 200);

        var query = db.StaffPersonRefs.AsNoTracking();
        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        var items = await query
            .Where(x => x.LastSeenAt < threshold)
            .OrderBy(x => x.LastSeenAt)
            .Take(normalizedBatchSize)
            .Select(x => new PendingTechnicianRefRefreshItem(
                x.StaffarrPersonId,
                x.LastSeenAt))
            .ToListAsync(cancellationToken);

        return new PendingTechnicianRefRefreshResponse(items.Count, items);
    }

    public async Task<ProcessTechnicianRefRefreshResponse> ProcessBatchAsync(
        ProcessTechnicianRefRefreshRequest request,
        CancellationToken cancellationToken = default)
    {
        var pending = await ListPendingStaleAsync(
            request.TenantId,
            request.AsOfUtc,
            request.BatchSize,
            request.StaleAfter,
            cancellationToken);

        var refreshed = 0;
        var skipped = 0;
        var failed = 0;

        if (!staffarrLookup.IsConfigured)
        {
            return new ProcessTechnicianRefRefreshResponse(
                pending.PendingCount,
                0,
                pending.PendingCount,
                0);
        }

        foreach (var item in pending.Items)
        {
            if (!Guid.TryParse(item.PersonId, out var personId))
            {
                skipped++;
                continue;
            }

            var refRow = await db.StaffPersonRefs
                .AsNoTracking()
                .Where(x => x.StaffarrPersonId == item.PersonId)
                .Select(x => new { x.TenantId })
                .FirstOrDefaultAsync(cancellationToken);

            if (refRow is null)
            {
                skipped++;
                continue;
            }

            var tenantId = request.TenantId ?? refRow.TenantId;

            try
            {
                var lookup = await staffarrLookup.TryLookupAsync(tenantId, personId, cancellationToken);
                if (lookup is null)
                {
                    skipped++;
                    continue;
                }

                await technicianRefService.UpsertAsync(
                    tenantId,
                    null,
                    new UpsertTechnicianRefRequest(
                        personId.ToString("D"),
                        lookup.DisplayName,
                        lookup.EmploymentStatus,
                        ResolvePrimarySite(lookup),
                        lookup.LookedUpAt,
                        $"staffarr.lookup:{lookup.LookedUpAt:O}"),
                    cancellationToken);
                refreshed++;
            }
            catch
            {
                failed++;
            }
        }

        return new ProcessTechnicianRefRefreshResponse(
            pending.PendingCount,
            refreshed,
            skipped,
            failed);
    }

    internal static string? ResolvePrimarySite(StaffArrIntegrationPersonLookupResponse lookup)
    {
        if (!string.IsNullOrWhiteSpace(lookup.Placement.PrimaryOrgUnitName))
        {
            return lookup.Placement.PrimaryOrgUnitName;
        }

        return null;
    }
}
