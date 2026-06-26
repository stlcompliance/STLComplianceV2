using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public class LaunchDestinationReconciliationSettingsService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<LaunchDestinationReconciliationSettingsResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var settings = await LoadOrDefaultAsync(cancellationToken);
        return MapResponse(settings);
    }

    public async Task<LaunchDestinationReconciliationSettingsResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertLaunchDestinationReconciliationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;

        var entity = await db.PlatformLaunchDestinationReconciliationSettings
            .FirstOrDefaultAsync(
                x => x.Id == PlatformLaunchDestinationReconciliationSettings.SingletonId,
                cancellationToken);

        if (entity is null)
        {
            entity = new PlatformLaunchDestinationReconciliationSettings
            {
                Id = PlatformLaunchDestinationReconciliationSettings.SingletonId,
                CreatedAt = now,
            };
            db.PlatformLaunchDestinationReconciliationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.AutoGrantFromLicense = request.AutoGrantFromLicense;
        entity.AutoRevokeStaleLaunchDestinations = request.AutoRevokeStaleLaunchDestinations;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "launch_destination_reconciliation.settings.update",
            "platform_launch_destination_reconciliation_settings",
            entity.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<PlatformLaunchDestinationReconciliationSettings> LoadOrDefaultAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await db.PlatformLaunchDestinationReconciliationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == PlatformLaunchDestinationReconciliationSettings.SingletonId,
                cancellationToken);

        return settings ?? new PlatformLaunchDestinationReconciliationSettings
        {
            Id = PlatformLaunchDestinationReconciliationSettings.SingletonId,
            IsEnabled = false,
            AutoGrantFromLicense = true,
            AutoRevokeStaleLaunchDestinations = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    private static LaunchDestinationReconciliationSettingsResponse MapResponse(
        PlatformLaunchDestinationReconciliationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.AutoGrantFromLicense,
            settings.AutoRevokeStaleLaunchDestinations,
            settings.UpdatedAt == default ? null : settings.UpdatedAt);
}

public sealed class CompatibilityLegacyEntitlementReconciliationSettingsService(
    LaunchDestinationReconciliationSettingsService inner)
{
    public Task<LaunchDestinationReconciliationSettingsResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default) =>
        inner.GetAsync(principal, cancellationToken);

    public Task<LaunchDestinationReconciliationSettingsResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertLaunchDestinationReconciliationSettingsRequest request,
        CancellationToken cancellationToken = default) =>
        inner.UpsertAsync(principal, request, cancellationToken);

    internal Task<PlatformLaunchDestinationReconciliationSettings> LoadOrDefaultAsync(
        CancellationToken cancellationToken = default) =>
        inner.LoadOrDefaultAsync(cancellationToken);
}
