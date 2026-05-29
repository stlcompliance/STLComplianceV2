using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class PlatformOutboxPublisherSettingsService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<PlatformOutboxPublisherSettingsResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var settings = await LoadOrDefaultAsync(cancellationToken);
        return MapResponse(settings);
    }

    public async Task<PlatformOutboxPublisherSettingsResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertPlatformOutboxPublisherSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;

        var entity = await db.PlatformOutboxPublisherSettings
            .FirstOrDefaultAsync(x => x.Id == PlatformOutboxPublisherSettings.SingletonId, cancellationToken);

        if (entity is null)
        {
            entity = new PlatformOutboxPublisherSettings
            {
                Id = PlatformOutboxPublisherSettings.SingletonId,
                CreatedAt = now,
            };
            db.PlatformOutboxPublisherSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.MaxRetryAttempts = PlatformOutboxRules.NormalizeMaxRetryAttempts(request.MaxRetryAttempts);
        entity.RetryIntervalMinutes = PlatformOutboxRules.NormalizeRetryIntervalMinutes(request.RetryIntervalMinutes);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "platform_outbox.settings.update",
            "platform_outbox_publisher_settings",
            entity.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<PlatformOutboxPublisherSettings> LoadOrDefaultAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await db.PlatformOutboxPublisherSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == PlatformOutboxPublisherSettings.SingletonId, cancellationToken);

        return settings ?? new PlatformOutboxPublisherSettings
        {
            Id = PlatformOutboxPublisherSettings.SingletonId,
            IsEnabled = true,
            MaxRetryAttempts = PlatformOutboxRules.DefaultMaxRetryAttempts,
            RetryIntervalMinutes = PlatformOutboxRules.DefaultRetryIntervalMinutes,
        };
    }

    private static PlatformOutboxPublisherSettingsResponse MapResponse(PlatformOutboxPublisherSettings settings) =>
        new(
            settings.IsEnabled,
            settings.MaxRetryAttempts,
            settings.RetryIntervalMinutes,
            settings.UpdatedAt == default ? null : settings.UpdatedAt);
}
