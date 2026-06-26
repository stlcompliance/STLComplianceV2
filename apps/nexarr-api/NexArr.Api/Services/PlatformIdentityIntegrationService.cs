using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformIdentityIntegrationService(
    NexArrDbContext db,
    IPasswordHasher passwordHasher,
    PlatformSessionSettingsService sessionSettingsService,
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

        var lastProductLaunchAt = await GetLastProductLaunchAtAsync(user.Id, cancellationToken);
        var launchEligible = await IsLaunchEligibleAsync(user, tenantId, cancellationToken);
        return Map(user, lastProductLaunchAt, launchEligible);
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
        var actorUserId = request.RequestedByUserId;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var settings = await sessionSettingsService.LoadOrDefaultAsync(cancellationToken);
            if (!PasswordResetRules.MeetsPasswordPolicy(
                    request.Password,
                    settings.PasswordMinLength,
                    settings.RequirePasswordComplexity))
            {
                throw new StlApiException(
                    "auth.password_policy",
                    PasswordResetRules.PasswordPolicyMessage(
                        settings.PasswordMinLength,
                        settings.RequirePasswordComplexity),
                    400);
            }
        }

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

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Credential = new UserCredential
                {
                    UserId = user.Id,
                    PasswordHash = passwordHasher.Hash(request.Password),
                    PasswordChangedAt = now,
                    RequiresPasswordChange = request.RequiresPasswordChange,
                    IsEmailVerified = true,
                    FailedLoginCount = 0,
                    LockedUntil = null,
                };
            }

            db.Users.Add(user);
            wasCreated = true;
        }
        else
        {
            user.DisplayName = displayName;
            user.ModifiedAt = now;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Credential ??= new UserCredential
                {
                    UserId = user.Id,
                    IsEmailVerified = true,
                };

                user.Credential.PasswordHash = passwordHasher.Hash(request.Password);
                user.Credential.PasswordChangedAt = now;
                user.Credential.RequiresPasswordChange = request.RequiresPasswordChange;
            }
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
            actorUserId: actorUserId,
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
                        ["canLogin"] = (user.Credential is not null).ToString(),
                        ["requiresPasswordChange"] = user.Credential?.RequiresPasswordChange.ToString() ?? "False",
                        ["source"] = sourceProductKey,
                    }),
                cancellationToken: cancellationToken);
        }

        user = (await LoadUserAsync(user.Id, cancellationToken))!;
        var lastProductLaunchAt = await GetLastProductLaunchAtAsync(user.Id, cancellationToken);
        var launchEligible = await IsLaunchEligibleAsync(user, request.TenantId, cancellationToken);
        return new CreatePlatformIdentityResponse(
            wasCreated,
            membershipWasCreated,
            Map(user, lastProductLaunchAt, launchEligible));
    }

    public async Task<PlatformIdentityResponse> SyncAsync(
        Guid personId,
        SyncPlatformIdentityRequest request,
        string sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .Include(u => u.Credential)
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.Id == personId, cancellationToken)
            ?? throw new StlApiException("identity.not_found", "Platform identity was not found.", 404);

        var tenantExists = await db.Tenants.AnyAsync(t => t.Id == request.TenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var actorUserId = request.RequestedByUserId;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = NormalizeEmail(request.Email);
            var emailInUse = await db.Users.AnyAsync(
                x => x.Id != user.Id && x.Email == normalizedEmail,
                cancellationToken);
            if (emailInUse)
            {
                throw new StlApiException("identity.email_exists", "A platform identity already exists with that email.", 409);
            }

            user.Email = normalizedEmail;
        }
        user.DisplayName = NormalizeDisplayName(request.DisplayName);
        user.ModifiedAt = now;

        var roleKey = NormalizeRoleKey(request.RoleKey);
        var membership = user.Memberships.FirstOrDefault(x => x.TenantId == request.TenantId);
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
        }
        else
        {
            membership.IsActive = true;
            membership.RoleKey = roleKey;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "identity.sync",
            "user",
            user.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: actorUserId,
            reasonCode: sourceProductKey,
            cancellationToken: cancellationToken);

        var reloaded = (await LoadUserAsync(user.Id, cancellationToken))!;
        var lastProductLaunchAt = await GetLastProductLaunchAtAsync(user.Id, cancellationToken);
        var launchEligible = await IsLaunchEligibleAsync(reloaded, request.TenantId, cancellationToken);
        return Map(reloaded, lastProductLaunchAt, launchEligible);
    }

    private async Task<PlatformUser?> LoadUserAsync(Guid personId, CancellationToken cancellationToken) =>
        await db.Users
            .AsNoTracking()
            .Include(u => u.Credential)
            .Include(u => u.Sessions)
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.Id == personId, cancellationToken);

    private async Task<DateTimeOffset?> GetLastProductLaunchAtAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.AuditEvents
            .AsNoTracking()
            .Where(x =>
                x.ActorUserId == userId
                && x.Action == "launch.handoff.create"
                && x.Result == "Success")
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => (DateTimeOffset?)x.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> IsLaunchEligibleAsync(
        PlatformUser user,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!user.IsActive)
        {
            return false;
        }

        var hasActiveMembership = user.Memberships.Any(x => x.TenantId == tenantId && x.IsActive);
        if (!hasActiveMembership)
        {
            return false;
        }

        return true;
    }

    private static PlatformIdentityResponse Map(
        PlatformUser user,
        DateTimeOffset? lastProductLaunchAt,
        bool launchEligible)
    {
        var lastLoginAt = user.Sessions
            .Where(s => s.RevokedAt is null)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => (DateTimeOffset?)s.CreatedAt)
            .FirstOrDefault();
        var canLogin = user.Credential is not null;
        var status = PlatformUserStatusResolver.Resolve(
            user.IsActive,
            canLogin,
            user.Credential?.IsEmailVerified,
            user.Credential?.RequiresPasswordChange ?? false,
            user.Credential?.LockedUntil,
            DateTimeOffset.UtcNow);

        return new PlatformIdentityResponse(
            user.Id,
            user.Email,
            null,
            null,
            null,
            user.DisplayName,
            user.IsActive,
            canLogin,
            user.Credential?.IsMfaEnabled ?? false,
            user.Credential?.RequiresPasswordChange ?? false,
            launchEligible,
            status,
            user.IsPlatformAdmin,
            lastLoginAt,
            lastProductLaunchAt,
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
