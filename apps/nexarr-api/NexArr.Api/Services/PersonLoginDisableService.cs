using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PersonLoginDisableService(
    NexArrDbContext db,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
{
    public const string DisableLoginActionScope = "nexarr.users.login_disable";

    public static readonly Guid IntegrationActorUserId = Guid.Parse("00000000-0000-0000-0000-00000000000b");

    public async Task<PersonLoginDisableResponse> DisableLoginAsync(
        PersonLoginDisableRequest request,
        CancellationToken cancellationToken = default)
    {
        var reason = NormalizeReason(request.Reason);

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);
        if (tenant is null)
        {
            throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
        }

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.ExternalUserId, cancellationToken);
        if (user is null)
        {
            throw new StlApiException("user.not_found", "Platform user was not found.", 404);
        }

        var hasMembership = await db.TenantMemberships.AnyAsync(
            m => m.TenantId == request.TenantId
                && m.UserId == request.ExternalUserId
                && m.IsActive,
            cancellationToken);
        if (!hasMembership)
        {
            throw new StlApiException(
                "user.tenant_membership_missing",
                "User does not have an active membership for this tenant.",
                403);
        }

        var wasAlreadyDisabled = !user.IsActive;
        var now = DateTimeOffset.UtcNow;

        if (user.IsActive)
        {
            user.IsActive = false;
            user.ModifiedAt = now;
        }

        var sessionsRevoked = await RevokeActiveSessionsAsync(request.ExternalUserId, now, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "user.disabled",
            "user",
            user.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: IntegrationActorUserId,
            reasonCode: reason,
            cancellationToken: cancellationToken);

        if (!wasAlreadyDisabled)
        {
            await outboxEnqueue.TryEnqueueAsync(
                PlatformOutboxEventKinds.UserDisabled,
                "user",
                user.Id.ToString(),
                user.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
                new PlatformOutboxPayload(
                    PlatformOutboxRules.DefaultSchemaVersion,
                    request.TenantId,
                    IntegrationActorUserId,
                    "user",
                    user.Id.ToString(),
                    "Platform login disabled for workforce offboarding.",
                    new Dictionary<string, string>
                    {
                        ["staffarrPersonId"] = request.StaffarrPersonId.ToString(),
                        ["reason"] = reason,
                    }),
                cancellationToken: cancellationToken);
        }

        return new PersonLoginDisableResponse(request.ExternalUserId, wasAlreadyDisabled, sessionsRevoked);
    }

    private async Task<int> RevokeActiveSessionsAsync(
        Guid userId,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken)
    {
        var sessions = await db.UserSessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAt = revokedAt;
        }

        if (sessions.Count == 0)
        {
            return 0;
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var session in sessions)
        {
            await audit.WriteAsync(
                "auth.session_revoked",
                "session",
                session.Id.ToString(),
                "Success",
                tenantId: session.ActiveTenantId,
                actorUserId: IntegrationActorUserId,
                reasonCode: "login_disable",
                cancellationToken: cancellationToken);
        }

        return sessions.Count;
    }

    private static string NormalizeReason(string? reason)
    {
        var trimmed = reason?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return "Workforce offboarding";
        }

        return trimmed.Length <= 512 ? trimmed : trimmed[..512];
    }
}
