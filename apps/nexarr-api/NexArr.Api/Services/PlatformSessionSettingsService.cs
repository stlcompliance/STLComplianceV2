using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class PlatformSessionSettingsService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit,
    IOptions<StlJwtOptions> jwtOptions)
{
    public async Task<PlatformSessionSettingsResponse> GetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var settings = await LoadOrDefaultAsync(cancellationToken);
        return MapResponse(settings);
    }

    public async Task<PlatformSessionSettingsResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertPlatformSessionSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;
        var defaults = ResolveDefaults();

        var entity = await db.PlatformSessionSettings
            .FirstOrDefaultAsync(x => x.Id == PlatformSessionSettings.SingletonId, cancellationToken);

        if (entity is null)
        {
            entity = new PlatformSessionSettings
            {
                Id = PlatformSessionSettings.SingletonId,
                CreatedAt = now,
            };
            db.PlatformSessionSettings.Add(entity);
        }

        entity.AccessTokenMinutes = PlatformSessionSettingsRules.NormalizeAccessTokenMinutes(
            request.AccessTokenMinutes,
            defaults.AccessTokenMinutes);
        entity.RefreshTokenDays = PlatformSessionSettingsRules.NormalizeRefreshTokenDays(
            request.RefreshTokenDays,
            defaults.RefreshTokenDays);
        entity.RememberedRefreshTokenDays = PlatformSessionSettingsRules.NormalizeRememberedRefreshTokenDays(
            request.RememberedRefreshTokenDays,
            entity.RefreshTokenDays,
            defaults.RememberedRefreshTokenDays);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "platform_session.settings.update",
            "platform_session_settings",
            entity.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<PlatformSessionSettings> LoadOrDefaultAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await db.PlatformSessionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == PlatformSessionSettings.SingletonId, cancellationToken);

        if (settings is null)
        {
            var defaults = ResolveDefaults();
            return new PlatformSessionSettings
            {
                Id = PlatformSessionSettings.SingletonId,
                AccessTokenMinutes = defaults.AccessTokenMinutes,
                RefreshTokenDays = defaults.RefreshTokenDays,
                RememberedRefreshTokenDays = defaults.RememberedRefreshTokenDays,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
        }

        return NormalizeLoaded(settings);
    }

    private PlatformSessionSettings NormalizeLoaded(PlatformSessionSettings settings)
    {
        var defaults = ResolveDefaults();
        settings.AccessTokenMinutes = PlatformSessionSettingsRules.NormalizeAccessTokenMinutes(
            settings.AccessTokenMinutes,
            defaults.AccessTokenMinutes);
        settings.RefreshTokenDays = PlatformSessionSettingsRules.NormalizeRefreshTokenDays(
            settings.RefreshTokenDays,
            defaults.RefreshTokenDays);
        settings.RememberedRefreshTokenDays = PlatformSessionSettingsRules.NormalizeRememberedRefreshTokenDays(
            settings.RememberedRefreshTokenDays,
            settings.RefreshTokenDays,
            defaults.RememberedRefreshTokenDays);
        return settings;
    }

    private PlatformSessionSettingsDefaults ResolveDefaults()
    {
        var options = jwtOptions.Value;
        var accessTokenMinutes = PlatformSessionSettingsRules.ResolveConfiguredAccessTokenMinutes(options);
        var refreshTokenDays = PlatformSessionSettingsRules.ResolveConfiguredRefreshTokenDays(options);
        var rememberedRefreshTokenDays = PlatformSessionSettingsRules.ResolveConfiguredRememberedRefreshTokenDays(
            options,
            refreshTokenDays);

        return new PlatformSessionSettingsDefaults(
            accessTokenMinutes,
            refreshTokenDays,
            rememberedRefreshTokenDays);
    }

    private static PlatformSessionSettingsResponse MapResponse(PlatformSessionSettings settings) =>
        new(
            settings.AccessTokenMinutes,
            settings.RefreshTokenDays,
            settings.RememberedRefreshTokenDays,
            settings.UpdatedAt == default ? null : settings.UpdatedAt);

    private sealed record PlatformSessionSettingsDefaults(
        int AccessTokenMinutes,
        int RefreshTokenDays,
        int RememberedRefreshTokenDays);
}
