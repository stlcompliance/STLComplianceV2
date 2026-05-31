using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformIdentityIntegrationService(
    NexArrDbContext db,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
{
    public const string ReadIdentityActionScope = "nexarr.identities.read";
    public const string CreateIdentityActionScope = "nexarr.identities.create";

    public async Task<PlatformIdentityResponse> ResolveAsync(
        Guid personId,
        Guid tenantId,
        string sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var user = await LoadUserAsync(personId, cancellationToken)
            ?? throw new StlApiException("identity.not_found", "Platform identity was not found.", 404);

        if (!user.Memberships.Any(m => m.TenantId == tenantId && m.IsActive))
        {
            throw new StlApiException("identity.tenant_membership_not_found", "Platform identity is not active for this tenant.", 404);
        }

        await audit.WriteAsync(
            "identity.resolve",
            "user",
            user.Id.ToString(),
            "Success",
            tenantId: tenantId,
            reasonCode: sourceProductKey,
            cancellationToken: cancellationToken);

        return Map(user);
    }

    public async Task<CreatePlatformIdentityResponse> CreateMinimalAsync(
        CreatePlatformIdentityRequest request,
        string sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var tenantExists = await db.Tenants.AnyAsync(t => t.Id == request.TenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
        }

        var email = NormalizeEmail(request.Email);
        var displayName = NormalizeDisplayName(request.DisplayName);
        var roleKey = NormalizeRoleKey(request.RoleKey);
        var now = DateTimeOffset.UtcNow;

        var user = await db.Users
            .Include(u => u.Credential)
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        var wasCreated = false;
        if (user is null)
        {
            user = new PlatformUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = displayName,
                IsActive = true,
                IsPlatformAdmin = false,
                CreatedAt = now,
                ModifiedAt = now,
            };
            db.Users.Add(user);
            wasCreated = true;
        }
        else
        {
            user.DisplayName = displayName;
            user.ModifiedAt = now;
        }

        var membership = user.Memberships.FirstOrDefault(m => m.TenantId == request.TenantId);
        var membershipWasCreated = false;
        if (membership is null)
        {
            user.Memberships.Add(new TenantMembership
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                UserId = user.Id,
                RoleKey = roleKey,
                IsActive = true,
                CreatedAt = now,
            });
            membershipWasCreated = true;
        }
        else if (!membership.IsActive || !string.Equals(membership.RoleKey, roleKey, StringComparison.OrdinalIgnoreCase))
        {
            membership.IsActive = true;
            membership.RoleKey = roleKey;
            membershipWasCreated = true;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            wasCreated ? "identity.create" : "identity.sync",
            "user",
            user.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            reasonCode: sourceProductKey,
            cancellationToken: cancellationToken);

        if (wasCreated)
        {
            await outboxEnqueue.TryEnqueueAsync(
                PlatformOutboxEventKinds.UserCreated,
                "user",
                user.Id.ToString(),
                user.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
                new PlatformOutboxPayload(
                    PlatformOutboxRules.DefaultSchemaVersion,
                    request.TenantId,
                    null,
                    "user",
                    user.Id.ToString(),
                    $"Minimal platform identity created: {user.DisplayName}",
                    new Dictionary<string, string>
                    {
                        ["email"] = user.Email,
                        ["displayName"] = user.DisplayName,
                        ["canLogin"] = "False",
                        ["source"] = sourceProductKey,
                    }),
                cancellationToken: cancellationToken);
        }

        user = (await LoadUserAsync(user.Id, cancellationToken))!;
        return new CreatePlatformIdentityResponse(wasCreated, membershipWasCreated, Map(user));
    }

    private async Task<PlatformUser?> LoadUserAsync(Guid personId, CancellationToken cancellationToken) =>
        await db.Users
            .AsNoTracking()
            .Include(u => u.Credential)
            .Include(u => u.Sessions)
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.Id == personId, cancellationToken);

    private static PlatformIdentityResponse Map(PlatformUser user)
    {
        var lastLoginAt = user.Sessions
            .Where(s => s.RevokedAt is null)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => (DateTimeOffset?)s.CreatedAt)
            .FirstOrDefault();

        return new PlatformIdentityResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsActive,
            user.Credential is not null,
            user.IsPlatformAdmin,
            lastLoginAt,
            user.CreatedAt,
            user.ModifiedAt,
            user.Memberships
                .OrderBy(m => m.TenantId)
                .Select(m => new PlatformIdentityTenantMembershipResponse(m.TenantId, m.RoleKey, m.IsActive))
                .ToList());
    }

    private static string NormalizeEmail(string email)
    {
        var normalized = PasswordResetRules.NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalized) || !normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new StlApiException("identity.invalid_email", "Email is required.", 400);
        }

        return normalized;
    }

    private static string NormalizeDisplayName(string displayName)
    {
        var normalized = displayName?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException("identity.invalid_display_name", "Display name is required.", 400);
        }

        return normalized.Length <= 200 ? normalized : normalized[..200];
    }

    private static string NormalizeRoleKey(string? roleKey)
    {
        var normalized = roleKey?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? "employee" : normalized[..Math.Min(normalized.Length, 64)];
    }
}
