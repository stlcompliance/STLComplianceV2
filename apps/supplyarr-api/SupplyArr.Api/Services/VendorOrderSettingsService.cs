using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class VendorOrderSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<VendorOrderSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantVendorOrderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return entity is null
            ? new VendorOrderSettingsResponse(false, VendorOrderDefaults.DefaultMagicLinkTtlHours, null)
            : new VendorOrderSettingsResponse(
                entity.AllowDestinationSummaryInVendorPortal,
                entity.MagicLinkTtlHours,
                entity.UpdatedAt);
    }

    public async Task<VendorOrderSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertVendorOrderSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantVendorOrderSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantVendorOrderSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantVendorOrderSettings.Add(entity);
        }

        entity.AllowDestinationSummaryInVendorPortal = request.AllowDestinationSummaryInVendorPortal;
        entity.MagicLinkTtlHours = NormalizeMagicLinkTtlHours(request.MagicLinkTtlHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "supplyarr.vendor_order_settings.update",
            tenantId,
            actorUserId,
            "tenant_vendor_order_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new VendorOrderSettingsResponse(
            entity.AllowDestinationSummaryInVendorPortal,
            entity.MagicLinkTtlHours,
            entity.UpdatedAt);
    }

    internal async Task<TenantVendorOrderSettings> LoadOrDefaultAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.TenantVendorOrderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken)
            ?? new TenantVendorOrderSettings
            {
                Id = Guid.Empty,
                TenantId = tenantId,
                AllowDestinationSummaryInVendorPortal = false,
                MagicLinkTtlHours = VendorOrderDefaults.DefaultMagicLinkTtlHours,
            };
    }

    internal static int NormalizeMagicLinkTtlHours(int value)
    {
        if (value < 1)
        {
            return VendorOrderDefaults.DefaultMagicLinkTtlHours;
        }

        return Math.Min(value, 24 * 30);
    }
}
