using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class SupplierOrderSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<SupplierOrderSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantSupplierOrderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return entity is null
            ? new SupplierOrderSettingsResponse(false, SupplierOrderDefaults.DefaultMagicLinkTtlHours, null)
            : new SupplierOrderSettingsResponse(
                entity.AllowDestinationSummaryInSupplierPortal,
                entity.MagicLinkTtlHours,
                entity.UpdatedAt);
    }

    public async Task<SupplierOrderSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertSupplierOrderSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantSupplierOrderSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantSupplierOrderSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantSupplierOrderSettings.Add(entity);
        }

        entity.AllowDestinationSummaryInSupplierPortal = request.AllowDestinationSummaryInSupplierPortal;
        entity.MagicLinkTtlHours = NormalizeMagicLinkTtlHours(request.MagicLinkTtlHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "supplyarr.supplier_order_settings.update",
            tenantId,
            actorUserId,
            "tenant_supplier_order_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new SupplierOrderSettingsResponse(
            entity.AllowDestinationSummaryInSupplierPortal,
            entity.MagicLinkTtlHours,
            entity.UpdatedAt);
    }

    internal async Task<TenantSupplierOrderSettings> LoadOrDefaultAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.TenantSupplierOrderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken)
            ?? new TenantSupplierOrderSettings
            {
                Id = Guid.Empty,
                TenantId = tenantId,
                AllowDestinationSummaryInSupplierPortal = false,
                MagicLinkTtlHours = SupplierOrderDefaults.DefaultMagicLinkTtlHours,
            };
    }

    internal static int NormalizeMagicLinkTtlHours(int value)
    {
        if (value < 1)
        {
            return SupplierOrderDefaults.DefaultMagicLinkTtlHours;
        }

        return Math.Min(value, 24 * 30);
    }
}
