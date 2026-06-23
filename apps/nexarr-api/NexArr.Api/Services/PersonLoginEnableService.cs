using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PersonLoginEnableService(
    NexArrDbContext db,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
{
    public const string EnableLoginActionScope = "nexarr.users.login_enable";

    public static readonly Guid IntegrationActorUserId = Guid.Parse("00000000-0000-0000-0000-00000000000c");

    public async Task<PersonLoginEnableResponse> EnableLoginAsync(
        PersonLoginEnableRequest request,
        CancellationToken cancellationToken = default)
    {
        var reason = NormalizeReason(request.Reason);
        var actorUserId = request.RequestedByUserId ?? IntegrationActorUserId;

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

        var wasAlreadyEnabled = user.IsActive;
        var now = DateTimeOffset.UtcNow;

        if (!user.IsActive)
        {
            user.IsActive = true;
            user.ModifiedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        await audit.WriteAsync(
            "user.enabled",
            "user",
            user.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: actorUserId,
            reasonCode: reason,
            cancellationToken: cancellationToken);

        if (!wasAlreadyEnabled)
        {
            await outboxEnqueue.TryEnqueueAsync(
                PlatformOutboxEventKinds.UserEnabled,
                "user",
                user.Id.ToString(),
                user.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
                new PlatformOutboxPayload(
                    PlatformOutboxRules.DefaultSchemaVersion,
                    request.TenantId,
                    actorUserId,
                    "user",
                    user.Id.ToString(),
                    "Platform login enabled for workforce onboarding.",
                    new Dictionary<string, string>
                    {
                        ["staffarrPersonId"] = request.StaffarrPersonId.ToString(),
                        ["reason"] = reason,
                        ["requestedByUserId"] = actorUserId.ToString(),
                    }),
                cancellationToken: cancellationToken);
        }

        return new PersonLoginEnableResponse(request.ExternalUserId, wasAlreadyEnabled);
    }

    private static string NormalizeReason(string? reason)
    {
        var trimmed = reason?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return "Workforce onboarding";
        }

        return trimmed.Length <= 512 ? trimmed : trimmed[..512];
    }
}
