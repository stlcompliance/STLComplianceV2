using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class EntitlementReconciliationSettingsService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<EntitlementReconciliationSettingsResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var settings = await LoadOrDefaultAsync(cancellationToken);
        return MapResponse(settings);
    }

    public async Task<EntitlementReconciliationSettingsResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertEntitlementReconciliationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;

        var entity = await db.PlatformEntitlementReconciliationSettings
            .FirstOrDefaultAsync(
                x => x.Id == PlatformEntitlementReconciliationSettings.SingletonId,
                cancellationToken);

        if (entity is null)
        {
            entity = new PlatformEntitlementReconciliationSettings
            {
                Id = PlatformEntitlementReconciliationSettings.SingletonId,
                CreatedAt = now,
            };
            db.PlatformEntitlementReconciliationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.AutoGrantFromLicense = request.AutoGrantFromLicense;
        entity.AutoRevokeStaleEntitlements = request.AutoRevokeStaleEntitlements;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "entitlement_reconciliation.settings.update",
            "platform_entitlement_reconciliation_settings",
            entity.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<PlatformEntitlementReconciliationSettings> LoadOrDefaultAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await db.PlatformEntitlementReconciliationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == PlatformEntitlementReconciliationSettings.SingletonId,
                cancellationToken);

        return settings ?? new PlatformEntitlementReconciliationSettings
        {
            Id = PlatformEntitlementReconciliationSettings.SingletonId,
            IsEnabled = false,
            AutoGrantFromLicense = true,
            AutoRevokeStaleEntitlements = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    private static EntitlementReconciliationSettingsResponse MapResponse(
        PlatformEntitlementReconciliationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.AutoGrantFromLicense,
            settings.AutoRevokeStaleEntitlements,
            settings.UpdatedAt == default ? null : settings.UpdatedAt);
}
