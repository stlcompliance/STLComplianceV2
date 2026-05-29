using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformUserAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
{
    public async Task<PlatformUserEnableResponse> EnableUserAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        var wasAlreadyEnabled = user.IsActive;
        var now = DateTimeOffset.UtcNow;
        var actorUserId = principal.GetUserId();

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
            actorUserId: actorUserId,
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
                    null,
                    actorUserId,
                    "user",
                    user.Id.ToString(),
                    $"Platform user enabled: {user.DisplayName}",
                    new Dictionary<string, string>
                    {
                        ["email"] = user.Email,
                        ["source"] = "platform_admin",
                    }),
                cancellationToken: cancellationToken);
        }

        return new PlatformUserEnableResponse(user.Id, wasAlreadyEnabled);
    }
}
