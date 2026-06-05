using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionPushSubscriptionService(
    NexArrDbContext db,
    IPlatformAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<FieldCompanionPushSubscriptionResponse> UpsertAsync(
        Guid tenantId,
        Guid userId,
        UpsertFieldCompanionPushSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var endpoint = FieldCompanionPushSubscriptionRules.NormalizeEndpoint(request.Endpoint, allowInsecureHttp);
        var p256dh = FieldCompanionPushSubscriptionRules.NormalizeKey(
            request.Keys.P256dh,
            "fieldcompanion.push.p256dh_invalid",
            "Push subscription p256dh key is invalid.");
        var auth = FieldCompanionPushSubscriptionRules.NormalizeKey(
            request.Keys.Auth,
            "fieldcompanion.push.auth_invalid",
            "Push subscription auth key is invalid.");
        var userAgent = FieldCompanionPushSubscriptionRules.NormalizeUserAgent(request.UserAgent);

        var entity = await db.FieldCompanionPushSubscriptions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.Endpoint == endpoint,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new FieldCompanionPushSubscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                Endpoint = endpoint,
                CreatedAt = now,
            };
            db.FieldCompanionPushSubscriptions.Add(entity);
        }

        entity.P256dhKey = p256dh;
        entity.AuthKey = auth;
        entity.UserAgent = userAgent;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "fieldcompanion.push.subscribe",
            "fieldcompanion_push_subscription",
            entity.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: userId,
            cancellationToken: cancellationToken);

        return new FieldCompanionPushSubscriptionResponse(entity.Id, entity.Endpoint, entity.UpdatedAt);
    }

    public async Task UnsubscribeAsync(
        Guid tenantId,
        Guid userId,
        UnsubscribeFieldCompanionPushRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var endpoint = FieldCompanionPushSubscriptionRules.NormalizeEndpoint(request.Endpoint, allowInsecureHttp);
        var entity = await db.FieldCompanionPushSubscriptions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.Endpoint == endpoint,
            cancellationToken);

        if (entity is null)
        {
            return;
        }

        db.FieldCompanionPushSubscriptions.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "fieldcompanion.push.unsubscribe",
            "fieldcompanion_push_subscription",
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
        db.FieldCompanionPushSubscriptions.AnyAsync(
            x => x.TenantId == tenantId && x.UserId == userId,
            cancellationToken);

    public Task<List<FieldCompanionPushSubscription>> ListForUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        db.FieldCompanionPushSubscriptions
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .ToListAsync(cancellationToken);
}

public static class FieldCompanionPushSubscriptionRules
{
    public const int MaxEndpointLength = 2048;
    public const int MaxKeyLength = 256;
    public const int MaxUserAgentLength = 512;

    public static string NormalizeEndpoint(string raw, bool allowInsecureHttp)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new StlApiException("fieldcompanion.push.endpoint_required", "Push subscription endpoint is required.", 400);
        }

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxEndpointLength)
        {
            throw new StlApiException(
                "fieldcompanion.push.endpoint_too_long",
                $"Push subscription endpoint must be at most {MaxEndpointLength} characters.",
                400);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
        {
            throw new StlApiException(
                "fieldcompanion.push.endpoint_invalid",
                "Push subscription endpoint must be an absolute http or https URL.",
                400);
        }

        if (!allowInsecureHttp && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "fieldcompanion.push.endpoint_https_required",
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
