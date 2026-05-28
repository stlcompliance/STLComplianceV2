using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionPushSubscriptionService(
    NexArrDbContext db,
    IPlatformAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<CompanionPushSubscriptionResponse> UpsertAsync(
        Guid tenantId,
        Guid userId,
        UpsertCompanionPushSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var endpoint = CompanionPushSubscriptionRules.NormalizeEndpoint(request.Endpoint, allowInsecureHttp);
        var p256dh = CompanionPushSubscriptionRules.NormalizeKey(
            request.Keys.P256dh,
            "companion.push.p256dh_invalid",
            "Push subscription p256dh key is invalid.");
        var auth = CompanionPushSubscriptionRules.NormalizeKey(
            request.Keys.Auth,
            "companion.push.auth_invalid",
            "Push subscription auth key is invalid.");
        var userAgent = CompanionPushSubscriptionRules.NormalizeUserAgent(request.UserAgent);

        var entity = await db.CompanionPushSubscriptions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.Endpoint == endpoint,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new CompanionPushSubscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                Endpoint = endpoint,
                CreatedAt = now,
            };
            db.CompanionPushSubscriptions.Add(entity);
        }

        entity.P256dhKey = p256dh;
        entity.AuthKey = auth;
        entity.UserAgent = userAgent;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "companion.push.subscribe",
            "companion_push_subscription",
            entity.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: userId,
            cancellationToken: cancellationToken);

        return new CompanionPushSubscriptionResponse(entity.Id, entity.Endpoint, entity.UpdatedAt);
    }

    public async Task UnsubscribeAsync(
        Guid tenantId,
        Guid userId,
        UnsubscribeCompanionPushRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var endpoint = CompanionPushSubscriptionRules.NormalizeEndpoint(request.Endpoint, allowInsecureHttp);
        var entity = await db.CompanionPushSubscriptions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.Endpoint == endpoint,
            cancellationToken);

        if (entity is null)
        {
            return;
        }

        db.CompanionPushSubscriptions.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "companion.push.unsubscribe",
            "companion_push_subscription",
            entity.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: userId,
            cancellationToken: cancellationToken);
    }

    public Task<bool> UserHasSubscriptionAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        db.CompanionPushSubscriptions.AnyAsync(
            x => x.TenantId == tenantId && x.UserId == userId,
            cancellationToken);

    public Task<List<CompanionPushSubscription>> ListForUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        db.CompanionPushSubscriptions
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .ToListAsync(cancellationToken);
}

public static class CompanionPushSubscriptionRules
{
    public const int MaxEndpointLength = 2048;
    public const int MaxKeyLength = 256;
    public const int MaxUserAgentLength = 512;

    public static string NormalizeEndpoint(string raw, bool allowInsecureHttp)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new StlApiException("companion.push.endpoint_required", "Push subscription endpoint is required.", 400);
        }

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxEndpointLength)
        {
            throw new StlApiException(
                "companion.push.endpoint_too_long",
                $"Push subscription endpoint must be at most {MaxEndpointLength} characters.",
                400);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
        {
            throw new StlApiException(
                "companion.push.endpoint_invalid",
                "Push subscription endpoint must be an absolute http or https URL.",
                400);
        }

        if (!allowInsecureHttp && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "companion.push.endpoint_https_required",
                "Push subscription endpoint must use https.",
                400);
        }

        return uri.ToString();
    }

    public static string NormalizeKey(string raw, string reasonCode, string message)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new StlApiException(reasonCode, message, 400);
        }

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxKeyLength)
        {
            throw new StlApiException(reasonCode, message, 400);
        }

        return trimmed;
    }

    public static string? NormalizeUserAgent(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        return trimmed.Length <= MaxUserAgentLength ? trimmed : trimmed[..MaxUserAgentLength];
    }
}
