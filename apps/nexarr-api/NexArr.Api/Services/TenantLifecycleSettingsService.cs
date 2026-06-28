using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class TenantLifecycleSettingsService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<TenantLifecycleSettingsResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var settings = await LoadOrDefaultAsync(cancellationToken);
        return MapResponse(settings);
    }

    public async Task<TenantLifecycleSettingsResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertTenantLifecycleSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;

        var entity = await db.PlatformTenantLifecycleSettings
            .FirstOrDefaultAsync(
                x => x.Id == PlatformTenantLifecycleSettings.SingletonId,
                cancellationToken);

        if (entity is null)
        {
            entity = new PlatformTenantLifecycleSettings
            {
                Id = PlatformTenantLifecycleSettings.SingletonId,
                CreatedAt = now,
            };
            db.PlatformTenantLifecycleSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.AutoSuspendWhenNoValidLicense = false;
        entity.SuspendGraceDaysAfterLastLicenseExpiry = TenantLifecycleRules.DefaultSuspendGraceDays;
        entity.AutoReactivateWhenValidLicense = false;
        entity.RevokeSessionsOnSuspend = request.RevokeSessionsOnSuspend;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant_lifecycle.settings.update",
            "platform_tenant_lifecycle_settings",
            entity.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<PlatformTenantLifecycleSettings> LoadOrDefaultAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await db.PlatformTenantLifecycleSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == PlatformTenantLifecycleSettings.SingletonId,
                cancellationToken);

        return settings ?? new PlatformTenantLifecycleSettings
        {
            Id = PlatformTenantLifecycleSettings.SingletonId,
            IsEnabled = false,
            AutoSuspendWhenNoValidLicense = false,
            SuspendGraceDaysAfterLastLicenseExpiry = TenantLifecycleRules.DefaultSuspendGraceDays,
            AutoReactivateWhenValidLicense = false,
            RevokeSessionsOnSuspend = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    private static TenantLifecycleSettingsResponse MapResponse(
        PlatformTenantLifecycleSettings settings) =>
        new(
            settings.IsEnabled,
            settings.AutoSuspendWhenNoValidLicense,
            settings.SuspendGraceDaysAfterLastLicenseExpiry,
            settings.AutoReactivateWhenValidLicense,
            settings.RevokeSessionsOnSuspend,
            settings.UpdatedAt == default ? null : settings.UpdatedAt);
}
