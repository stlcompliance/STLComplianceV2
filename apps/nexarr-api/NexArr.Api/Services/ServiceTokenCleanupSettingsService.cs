using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class ServiceTokenCleanupSettingsService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<ServiceTokenCleanupSettingsResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var settings = await LoadOrDefaultAsync(cancellationToken);
        return MapResponse(settings);
    }

    public async Task<ServiceTokenCleanupSettingsResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertServiceTokenCleanupSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;

        var entity = await db.PlatformServiceTokenCleanupSettings
            .FirstOrDefaultAsync(x => x.Id == PlatformServiceTokenCleanupSettings.SingletonId, cancellationToken);

        if (entity is null)
        {
            entity = new PlatformServiceTokenCleanupSettings
            {
                Id = PlatformServiceTokenCleanupSettings.SingletonId,
                CreatedAt = now,
            };
            db.PlatformServiceTokenCleanupSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.RetentionDaysAfterExpiry = ServiceTokenCleanupRules.NormalizeRetentionDaysAfterExpiry(
            request.RetentionDaysAfterExpiry);
        entity.RetentionDaysAfterRevoke = ServiceTokenCleanupRules.NormalizeRetentionDaysAfterRevoke(
            request.RetentionDaysAfterRevoke);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "service_token_cleanup.settings.update",
            "platform_service_token_cleanup_settings",
            entity.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<PlatformServiceTokenCleanupSettings> LoadOrDefaultAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await db.PlatformServiceTokenCleanupSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == PlatformServiceTokenCleanupSettings.SingletonId, cancellationToken);

        return settings ?? new PlatformServiceTokenCleanupSettings
        {
            Id = PlatformServiceTokenCleanupSettings.SingletonId,
            IsEnabled = false,
            RetentionDaysAfterExpiry = ServiceTokenCleanupRules.DefaultRetentionDaysAfterExpiry,
            RetentionDaysAfterRevoke = ServiceTokenCleanupRules.DefaultRetentionDaysAfterRevoke,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    private static ServiceTokenCleanupSettingsResponse MapResponse(PlatformServiceTokenCleanupSettings settings) =>
        new(
            settings.IsEnabled,
            settings.RetentionDaysAfterExpiry,
            settings.RetentionDaysAfterRevoke,
            settings.UpdatedAt == default ? null : settings.UpdatedAt);
}
