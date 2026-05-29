using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class TenantMembershipAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
{
    private static readonly HashSet<string> AllowedRoleKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "tenant_user",
        "tenant_admin",
    };

    public async Task<TenantMembersListResponse> ListMembersAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        var members = await (
            from m in db.TenantMemberships.AsNoTracking()
            join u in db.Users.AsNoTracking() on m.UserId equals u.Id
            where m.TenantId == tenant.Id && m.IsActive
            orderby u.DisplayName
            select new TenantMemberResponse(
                m.Id,
                m.UserId,
                u.Email,
                u.DisplayName,
                m.RoleKey,
                m.IsActive,
                m.CreatedAt))
            .ToListAsync(cancellationToken);

        return new TenantMembersListResponse(tenant.Id, members);
    }

    public async Task<AddTenantMemberResponse> AddMemberAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        AddTenantMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);

        var roleKey = NormalizeRoleKey(request.RoleKey);

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        var existing = await db.TenantMemberships
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == request.UserId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var actorUserId = principal.GetUserId();
        var wasReactivated = false;

        if (existing is not null)
        {
            if (existing.IsActive)
            {
                throw new StlApiException(
                    "tenant.membership_exists",
                    "User already has an active membership for this tenant.",
                    409);
            }

            existing.IsActive = true;
            existing.RoleKey = roleKey;
            wasReactivated = true;
        }
        else
        {
            existing = new TenantMembership
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = request.UserId,
                RoleKey = roleKey,
                IsActive = true,
                CreatedAt = now,
            };
            db.TenantMemberships.Add(existing);
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant.membership_added",
            "tenant_membership",
            existing.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        var changeToken = wasReactivated
            ? $"reactivated:{existing.Id}:{now.ToUnixTimeMilliseconds()}"
            : existing.CreatedAt.ToUnixTimeMilliseconds().ToString();

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.TenantMembershipAdded,
            "tenant_membership",
            existing.Id.ToString(),
            changeToken,
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                tenantId,
                actorUserId,
                "tenant_membership",
                existing.Id.ToString(),
                wasReactivated
                    ? $"Tenant membership reactivated for {user.DisplayName}"
                    : $"Tenant membership added for {user.DisplayName}",
                new Dictionary<string, string>
                {
                    ["userId"] = user.Id.ToString(),
                    ["email"] = user.Email,
                    ["roleKey"] = roleKey,
                    ["wasReactivated"] = wasReactivated.ToString(),
                }),
            cancellationToken: cancellationToken);

        return new AddTenantMemberResponse(existing.Id, user.Id, wasReactivated);
    }

    public async Task<RemoveTenantMemberResponse> RemoveMemberAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);

        var membership = await db.TenantMemberships
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId, cancellationToken)
            ?? throw new StlApiException(
                "tenant.membership_not_found",
                "Tenant membership was not found.",
                404);

        var actorUserId = principal.GetUserId();
        var wasAlreadyRemoved = !membership.IsActive;
        var now = DateTimeOffset.UtcNow;

        if (membership.IsActive)
        {
            membership.IsActive = false;
            await db.SaveChangesAsync(cancellationToken);
        }

        await audit.WriteAsync(
            "tenant.membership_removed",
            "tenant_membership",
            membership.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        if (!wasAlreadyRemoved)
        {
            await outboxEnqueue.TryEnqueueAsync(
                PlatformOutboxEventKinds.TenantMembershipRemoved,
                "tenant_membership",
                membership.Id.ToString(),
                now.ToUnixTimeMilliseconds().ToString(),
                new PlatformOutboxPayload(
                    PlatformOutboxRules.DefaultSchemaVersion,
                    tenantId,
                    actorUserId,
                    "tenant_membership",
                    membership.Id.ToString(),
                    $"Tenant membership removed for {membership.User.DisplayName}",
                    new Dictionary<string, string>
                    {
                        ["userId"] = membership.UserId.ToString(),
                        ["email"] = membership.User.Email,
                        ["roleKey"] = membership.RoleKey,
                    }),
                cancellationToken: cancellationToken);
        }

        return new RemoveTenantMemberResponse(membership.Id, membership.UserId, wasAlreadyRemoved);
    }

    private static string NormalizeRoleKey(string roleKey)
    {
        var normalized = roleKey?.Trim().ToLowerInvariant() ?? "tenant_user";
        if (!AllowedRoleKeys.Contains(normalized))
        {
            throw new StlApiException(
                "tenant.invalid_role",
                "Role must be tenant_user or tenant_admin.",
                400);
        }

        return normalized;
    }
}
