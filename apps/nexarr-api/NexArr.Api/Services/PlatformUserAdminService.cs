using System.Security.Claims;
using System.Text.Json;
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
    IPasswordHasher passwordHasher,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue,
    IConfiguration configuration,
    MfaSecretProtector mfaSecretProtector,
    MfaService mfaService,
    PlatformSessionSettingsService sessionSettingsService,
    IStaffArrPersonProvisioningClient staffArrProvisioning)
{
    private static readonly HashSet<string> AllowedTenantRoleKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "tenant_user",
        "tenant_admin",
    };

    private static readonly HashSet<string> SupportedPlatformRoleKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "platform_admin",
        "platform_owner",
        "platform_support",
        "tenant_admin",
        "tenant_user",
        "service_client",
        "product_service",
        "read_only_auditor",
    };

    private const string SensitiveActionConfirmationValue = "CONFIRM";

    public async Task<PlatformUsersListResponse> ListUsersAsync(
        ClaimsPrincipal principal,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var safePage = page <= 0 ? 1 : page;
        var safePageSize = Math.Clamp(pageSize <= 0 ? 50 : pageSize, 1, 200);
        var normalizedSearch = search?.Trim();

        var query = db.Users
            .AsNoTracking()
            .Include(x => x.Credential)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            var term = normalizedSearch.ToLowerInvariant();
            query = query.Where(x =>
                x.Email.ToLower().Contains(term)
                || x.DisplayName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.Email)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => new
            {
                User = x,
                FailedLoginCount = x.Credential == null ? 0 : x.Credential.FailedLoginCount,
                LockedUntil = x.Credential == null ? null : x.Credential.LockedUntil
            })
            .ToListAsync(cancellationToken);

        var userIds = items.Select(x => x.User.Id).ToList();
        var lastLoginByUserId = await GetLastUserActivityByActionAsync(userIds, "auth.login", cancellationToken);
        var lastLaunchByUserId = await GetLastUserActivityByActionAsync(userIds, "launch.handoff.create", cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var mappedItems = items.Select(x =>
        {
            var canLogin = x.User.Credential is not null;
            var status = PlatformUserStatusResolver.Resolve(
                x.User.IsActive,
                canLogin,
                x.User.Credential == null ? null : x.User.Credential.IsEmailVerified,
                x.User.Credential?.RequiresPasswordChange ?? false,
                x.LockedUntil,
                now);
            return new PlatformUserListItemResponse(
                x.User.Id,
                x.User.Email,
                x.User.DisplayName,
                x.User.IsActive,
                x.User.IsPlatformAdmin,
                x.FailedLoginCount,
                x.LockedUntil,
                x.User.CreatedAt,
                x.User.ModifiedAt,
                lastLoginByUserId.TryGetValue(x.User.Id, out var lastLoginAt) ? lastLoginAt : null,
                lastLaunchByUserId.TryGetValue(x.User.Id, out var lastLaunchAt) ? lastLaunchAt : null,
                canLogin,
                status,
                x.User.Credential?.IsMfaEnabled ?? false);
        }).ToList();

        return new PlatformUsersListResponse(totalCount, safePage, safePageSize, mappedItems);
    }

    public async Task<PlatformUserDetailResponse> GetUserAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var user = await db.Users
            .AsNoTracking()
            .Include(x => x.Credential)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        var lastLoginByUserId = await GetLastUserActivityByActionAsync([userId], "auth.login", cancellationToken);
        var lastLaunchByUserId = await GetLastUserActivityByActionAsync([userId], "launch.handoff.create", cancellationToken);

        return MapUser(
            user,
            DateTimeOffset.UtcNow,
            lastLoginByUserId.TryGetValue(userId, out var lastLoginAt) ? lastLoginAt : null,
            lastLaunchByUserId.TryGetValue(userId, out var lastLaunchAt) ? lastLaunchAt : null);
    }

    public async Task<PlatformUserDetailResponse> CreateUserAsync(
        ClaimsPrincipal principal,
        CreatePlatformUserRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var email = NormalizeEmail(request.Email);
        var displayName = NormalizeDisplayName(request.DisplayName);
        await ValidatePasswordAsync(request.Password, cancellationToken);

        var exists = await db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
        {
            throw new StlApiException("user.email_exists", "A platform user already exists with that email.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var actorUserId = principal.GetUserId();
        var user = new PlatformUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            IsActive = request.IsActive,
            IsPlatformAdmin = request.IsPlatformAdmin,
            CreatedAt = now,
            ModifiedAt = now,
            Credential = new UserCredential
            {
                PasswordHash = passwordHasher.Hash(request.Password),
                PasswordChangedAt = now,
                IsEmailVerified = !request.RequireEmailVerification,
                FailedLoginCount = 0,
                LockedUntil = null
            }
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "user.created",
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        await EnqueueUserEventAsync(
            PlatformOutboxEventKinds.UserCreated,
            actorUserId,
            user,
            "Platform user created.",
            new Dictionary<string, string>
            {
                ["source"] = "platform_admin",
            },
            cancellationToken);

        return MapUser(user);
    }

    public async Task<PlatformUserDetailResponse> InviteUserAsync(
        ClaimsPrincipal principal,
        InvitePlatformUserRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var email = NormalizeEmail(request.Email);
        var displayName = NormalizeDisplayName(request.DisplayName);

        var exists = await db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
        {
            throw new StlApiException("user.email_exists", "A platform user already exists with that email.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var actorUserId = principal.GetUserId();
        var user = new PlatformUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            IsActive = request.IsActive,
            IsPlatformAdmin = request.IsPlatformAdmin,
            CreatedAt = now,
            ModifiedAt = now,
            Credential = null
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "user.invited",
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        await EnqueueUserEventAsync(
            PlatformOutboxEventKinds.UserCreated,
            actorUserId,
            user,
            "Platform user invited.",
            new Dictionary<string, string>
            {
                ["source"] = "platform_admin",
                ["invited"] = "true",
                ["canLogin"] = "false",
            },
            cancellationToken);

        return MapUser(user);
    }

    public async Task<PlatformUserDetailResponse> UpdateUserAsync(
        ClaimsPrincipal principal,
        Guid userId,
        UpdatePlatformUserRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        var email = NormalizeEmail(request.Email);
        var displayName = NormalizeDisplayName(request.DisplayName);
        var emailInUse = await db.Users.AnyAsync(u => u.Id != userId && u.Email == email, cancellationToken);
        if (emailInUse)
        {
            throw new StlApiException("user.email_exists", "A platform user already exists with that email.", 409);
        }

        var previousEmail = user.Email;
        var previousDisplayName = user.DisplayName;
        var previousPlatformAdmin = user.IsPlatformAdmin;
        var actorUserId = principal.GetUserId();

        if (IsBreakGlassUser(user.Email) && !request.IsPlatformAdmin)
        {
            throw new StlApiException(
                "admin.break_glass_protected",
                "Break-glass administrator account cannot be demoted from platform admin.",
                409);
        }

        user.Email = email;
        user.DisplayName = displayName;
        user.IsPlatformAdmin = request.IsPlatformAdmin;
        user.ModifiedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "user.updated",
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        await EnqueueUserEventAsync(
            PlatformOutboxEventKinds.UserUpdated,
            actorUserId,
            user,
            "Platform user updated.",
            new Dictionary<string, string>
            {
                ["previousEmail"] = previousEmail,
                ["previousDisplayName"] = previousDisplayName,
                ["previousPlatformAdmin"] = previousPlatformAdmin.ToString(),
                ["source"] = "platform_admin",
            },
            cancellationToken);

        return MapUser(user);
    }

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

    public async Task<PlatformUserDisableResponse> DisableUserAsync(
        ClaimsPrincipal principal,
        Guid userId,
        string? confirmationToken = null,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        RequireSensitiveActionConfirmation(confirmationToken);

        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        if (IsBreakGlassUser(user.Email))
        {
            throw new StlApiException(
                "admin.break_glass_protected",
                "Break-glass administrator account cannot be disabled.",
                409);
        }

        var wasAlreadyDisabled = !user.IsActive;
        var now = DateTimeOffset.UtcNow;
        var actorUserId = principal.GetUserId();

        if (user.IsActive)
        {
            user.IsActive = false;
            user.ModifiedAt = now;
            if (user.Credential is not null)
            {
                user.Credential.LockedUntil = now.AddYears(100);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        await audit.WriteAsync(
            "user.disabled",
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
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
                    null,
                    actorUserId,
                    "user",
                    user.Id.ToString(),
                    $"Platform user disabled: {user.DisplayName}",
                    new Dictionary<string, string>
                    {
                        ["email"] = user.Email,
                        ["source"] = "platform_admin",
                    }),
                cancellationToken: cancellationToken);
        }

        return new PlatformUserDisableResponse(user.Id, wasAlreadyDisabled);
    }

    public async Task<PlatformUserLockResponse> LockUserAsync(
        ClaimsPrincipal principal,
        Guid userId,
        string? confirmationToken = null,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        RequireSensitiveActionConfirmation(confirmationToken);

        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        if (user.Credential is null)
        {
            throw new StlApiException("user.login_not_enabled", "Platform user does not have login credentials.", 409);
        }

        if (IsBreakGlassUser(user.Email))
        {
            throw new StlApiException(
                "admin.break_glass_protected",
                "Break-glass administrator account cannot be locked.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var actorUserId = principal.GetUserId();
        var wasAlreadyLocked = user.Credential.LockedUntil is DateTimeOffset lockedUntil && lockedUntil > now;

        if (!wasAlreadyLocked)
        {
            user.Credential.LockedUntil = now.AddYears(100);
            user.ModifiedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        await audit.WriteAsync(
            "user.locked",
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        if (!wasAlreadyLocked)
        {
            await EnqueueUserEventAsync(
                PlatformOutboxEventKinds.UserLocked,
                actorUserId,
                user,
                "Platform user locked.",
                new Dictionary<string, string>
                {
                    ["source"] = "platform_admin",
                    ["lockedUntil"] = user.Credential.LockedUntil?.ToString("O") ?? string.Empty,
                },
                cancellationToken);
        }

        return new PlatformUserLockResponse(user.Id, wasAlreadyLocked, user.Credential.LockedUntil);
    }

    public async Task<PlatformUserUnlockResponse> UnlockUserAsync(
        ClaimsPrincipal principal,
        Guid userId,
        string? confirmationToken = null,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        RequireSensitiveActionConfirmation(confirmationToken);

        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        if (user.Credential is null)
        {
            throw new StlApiException("user.login_not_enabled", "Platform user does not have login credentials.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var actorUserId = principal.GetUserId();
        var wasAlreadyUnlocked = user.Credential.LockedUntil is not DateTimeOffset lockedUntil || lockedUntil <= now;

        if (!wasAlreadyUnlocked)
        {
            user.Credential.LockedUntil = null;
            user.Credential.FailedLoginCount = 0;
            user.ModifiedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        await audit.WriteAsync(
            "user.unlocked",
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        if (!wasAlreadyUnlocked)
        {
            await EnqueueUserEventAsync(
                PlatformOutboxEventKinds.UserUnlocked,
                actorUserId,
                user,
                "Platform user unlocked.",
                new Dictionary<string, string>
                {
                    ["source"] = "platform_admin",
                },
                cancellationToken);
        }

        return new PlatformUserUnlockResponse(user.Id, wasAlreadyUnlocked);
    }

    public async Task<AdminResetUserPasswordResponse> ResetUserPasswordAsync(
        ClaimsPrincipal principal,
        Guid userId,
        AdminResetUserPasswordRequest request,
        string? confirmationToken = null,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        RequireSensitiveActionConfirmation(confirmationToken);
        await ValidatePasswordAsync(request.NewPassword, cancellationToken);

        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        if (user.Credential is null)
        {
            throw new StlApiException("user.login_not_enabled", "Platform user does not have login credentials.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var actorUserId = principal.GetUserId();
        user.Credential.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.Credential.PasswordChangedAt = now;
        user.Credential.FailedLoginCount = 0;
        user.Credential.LockedUntil = null;
        user.ModifiedAt = now;

        var revokedSessions = await RevokeActiveSessionsForUserAsync(
            user.Id,
            actorUserId,
            now,
            "password_reset_admin",
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "user.password_reset_admin",
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        await EnqueueUserEventAsync(
            PlatformOutboxEventKinds.UserUpdated,
            actorUserId,
            user,
            "Platform user password reset by admin.",
            new Dictionary<string, string>
            {
                ["source"] = "platform_admin",
                ["passwordReset"] = "true",
                ["revokedSessionCount"] = revokedSessions.ToString(),
            },
            cancellationToken);

        return new AdminResetUserPasswordResponse(user.Id, now);
    }

    private async Task<int> RevokeActiveSessionsForUserAsync(
        Guid userId,
        Guid actorUserId,
        DateTimeOffset revokedAt,
        string reasonCode,
        CancellationToken cancellationToken)
    {
        var activeSessions = await db.UserSessions
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.RevokedAt = revokedAt;
        }

        foreach (var session in activeSessions)
        {
            await audit.WriteAsync(
                "auth.session_revoked",
                "session",
                session.Id.ToString(),
                "Success",
                tenantId: session.ActiveTenantId,
                actorUserId: actorUserId,
                reasonCode: reasonCode,
                cancellationToken: cancellationToken);
        }

        return activeSessions.Count;
    }

    public async Task<PlatformUserMfaResponse> SetUserMfaAsync(
        ClaimsPrincipal principal,
        Guid userId,
        SetPlatformUserMfaRequest request,
        string? confirmationToken = null,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        RequireSensitiveActionConfirmation(confirmationToken);

        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        if (user.Credential is null)
        {
            throw new StlApiException("user.login_not_enabled", "Platform user does not have login credentials.", 409);
        }

        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;
        var wasAlreadySet = user.Credential.IsMfaEnabled == request.IsEnabled;
        string? mfaSecret = null;
        IReadOnlyList<string>? recoveryCodes = null;

        if (!wasAlreadySet)
        {
            user.Credential.IsMfaEnabled = request.IsEnabled;
            if (request.IsEnabled)
            {
                mfaSecret = mfaService.GenerateSecret();
                recoveryCodes = mfaService.GenerateRecoveryCodes();
                user.Credential.MfaSecret = mfaSecretProtector.Protect(mfaSecret);
                user.Credential.MfaRecoveryCodeHashesJson =
                    JsonSerializer.Serialize(mfaService.HashRecoveryCodes(recoveryCodes));
            }
            else
            {
                user.Credential.MfaSecret = null;
                user.Credential.MfaRecoveryCodeHashesJson = null;
            }
            user.ModifiedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }
        else if (request.IsEnabled)
        {
            if (string.IsNullOrWhiteSpace(user.Credential.MfaSecret))
            {
                mfaSecret = mfaService.GenerateSecret();
                recoveryCodes = mfaService.GenerateRecoveryCodes();
                user.Credential.MfaSecret = mfaSecretProtector.Protect(mfaSecret);
                user.Credential.MfaRecoveryCodeHashesJson =
                    JsonSerializer.Serialize(mfaService.HashRecoveryCodes(recoveryCodes));
                user.ModifiedAt = now;
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (!mfaSecretProtector.TryResolvePlaintext(user.Credential.MfaSecret, out mfaSecret))
                {
                    throw new StlApiException(
                        "user.mfa_secret_invalid",
                        "Stored MFA secret is invalid.",
                        409);
                }

                if (!mfaSecretProtector.IsProtectedPayload(user.Credential.MfaSecret))
                {
                    user.Credential.MfaSecret = mfaSecretProtector.Protect(mfaSecret);
                    user.ModifiedAt = now;
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }

        var action = request.IsEnabled ? "user.mfa_enabled" : "user.mfa_disabled";
        await audit.WriteAsync(
            action,
            "user",
            user.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        if (!wasAlreadySet)
        {
            await EnqueueUserEventAsync(
                PlatformOutboxEventKinds.UserUpdated,
                actorUserId,
                user,
                request.IsEnabled ? "Platform user MFA enabled." : "Platform user MFA disabled.",
                new Dictionary<string, string>
                {
                    ["source"] = "platform_admin",
                    ["mfaEnabled"] = request.IsEnabled.ToString().ToLowerInvariant(),
                },
                cancellationToken);
        }

        return new PlatformUserMfaResponse(
            user.Id,
            user.Credential.IsMfaEnabled,
            wasAlreadySet,
            user.ModifiedAt,
            mfaSecret,
            request.IsEnabled && !string.IsNullOrWhiteSpace(mfaSecret)
                ? mfaService.BuildProvisioningUri("STL Compliance Suite", user.Email, mfaSecret)
                : null,
            recoveryCodes);
    }

    public async Task<PlatformUserTenantMembershipsResponse> ListTenantMembershipsAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var userExists = await db.Users.AsNoTracking().AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            throw new StlApiException("user.not_found", "Platform user was not found.", 404);
        }

        var items = await (
            from membership in db.TenantMemberships.AsNoTracking()
            join tenant in db.Tenants.AsNoTracking() on membership.TenantId equals tenant.Id
            where membership.UserId == userId
            orderby tenant.DisplayName
            select new PlatformUserTenantMembershipItemResponse(
                membership.TenantId,
                tenant.Slug,
                tenant.DisplayName,
                membership.RoleKey,
                membership.IsActive,
                membership.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PlatformUserTenantMembershipsResponse(userId, items);
    }

    public async Task<PlatformUserSessionsResponse> ListUserSessionsAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var userExists = await db.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            throw new StlApiException("user.not_found", "Platform user was not found.", 404);
        }

        Guid? currentSessionId = null;
        try
        {
            currentSessionId = principal.GetSessionId();
        }
        catch (InvalidOperationException)
        {
            // Some integration contexts may not carry a user session id claim.
        }

        var now = DateTimeOffset.UtcNow;
        var sessions = await db.UserSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PlatformUserSessionItemResponse(
                x.Id,
                x.CreatedAt,
                x.ExpiresAt,
                x.RevokedAt,
                x.UserAgent,
                x.IpAddress,
                x.ActiveTenantId,
                currentSessionId.HasValue && x.Id == currentSessionId.Value,
                x.RevokedAt == null && x.ExpiresAt > now,
                x.IsRemembered))
            .ToListAsync(cancellationToken);

        return new PlatformUserSessionsResponse(userId, sessions);
    }

    public async Task<PlatformUserSessionRevokeResponse> RevokeUserSessionAsync(
        ClaimsPrincipal principal,
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var actorUserId = principal.GetUserId();
        var session = await db.UserSessions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == sessionId, cancellationToken)
            ?? throw new StlApiException("auth.session_not_found", "Session was not found.", 404);

        var wasAlreadyRevoked = session.RevokedAt is not null;
        if (!wasAlreadyRevoked)
        {
            session.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        await audit.WriteAsync(
            "auth.session_revoked",
            "session",
            session.Id.ToString(),
            "Success",
            tenantId: session.ActiveTenantId,
            actorUserId: actorUserId,
            reasonCode: "platform_admin",
            cancellationToken: cancellationToken);

        return new PlatformUserSessionRevokeResponse(userId, sessionId, wasAlreadyRevoked);
    }

    public async Task<AssignPlatformUserTenantMembershipResponse> AssignTenantMembershipAsync(
        ClaimsPrincipal principal,
        Guid userId,
        AssignPlatformUserTenantMembershipRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var roleKey = NormalizeTenantRoleKey(request.RoleKey);
        var actorUserId = principal.GetUserId();

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.TenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        var membership = await db.TenantMemberships
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == request.TenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var wasReactivated = false;

        if (membership is null)
        {
            membership = new TenantMembership
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                UserId = userId,
                RoleKey = roleKey,
                IsActive = true,
                CreatedAt = now,
            };
            db.TenantMemberships.Add(membership);
        }
        else if (!membership.IsActive)
        {
            membership.IsActive = true;
            membership.RoleKey = roleKey;
            wasReactivated = true;
        }
        else
        {
            membership.RoleKey = roleKey;
        }

        await db.SaveChangesAsync(cancellationToken);
        await staffArrProvisioning.EnsurePersonAsync(
            tenant.Id,
            user.Id,
            user.Email,
            user.DisplayName,
            actorUserId,
            cancellationToken);

        await audit.WriteAsync(
            "tenant.membership_added",
            "tenant_membership",
            membership.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        await outboxEnqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.TenantMembershipAdded,
            "tenant_membership",
            membership.Id.ToString(),
            $"{membership.Id}:{membership.IsActive}:{roleKey}",
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                tenant.Id,
                actorUserId,
                "tenant_membership",
                membership.Id.ToString(),
                wasReactivated
                    ? $"Tenant membership reactivated for {user.DisplayName}"
                    : $"Tenant membership upserted for {user.DisplayName}",
                new Dictionary<string, string>
                {
                    ["userId"] = user.Id.ToString(),
                    ["email"] = user.Email,
                    ["roleKey"] = roleKey,
                    ["wasReactivated"] = wasReactivated.ToString(),
                    ["source"] = "platform_admin_user",
                }),
            cancellationToken: cancellationToken);

        return new AssignPlatformUserTenantMembershipResponse(userId, tenant.Id, wasReactivated);
    }

    public async Task<RemovePlatformUserTenantMembershipResponse> RemoveTenantMembershipAsync(
        ClaimsPrincipal principal,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();

        var membership = await db.TenantMemberships
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.membership_not_found", "Tenant membership was not found.", 404);

        var wasAlreadyRemoved = !membership.IsActive;
        if (!wasAlreadyRemoved)
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
                $"{membership.Id}:removed",
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
                        ["source"] = "platform_admin_user",
                    }),
                cancellationToken: cancellationToken);
        }

        return new RemovePlatformUserTenantMembershipResponse(userId, tenantId, wasAlreadyRemoved);
    }

    public async Task<PlatformUserRolesResponse> ListRolesAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        var assignments = await db.PlatformRoleAssignments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.RoleKey)
            .ThenBy(x => x.TenantId)
            .ToListAsync(cancellationToken);

        var items = assignments
            .Select(x => new PlatformUserRoleItemResponse(x.RoleKey, true, x.TenantId))
            .ToList();

        if (user.IsPlatformAdmin && items.All(x => !string.Equals(x.RoleKey, "platform_admin", StringComparison.OrdinalIgnoreCase) || x.TenantId is not null))
        {
            items.Insert(0, new PlatformUserRoleItemResponse("platform_admin", true, null));
        }

        return new PlatformUserRolesResponse(userId, items);
    }

    public async Task<AssignPlatformUserRoleResponse> AssignRoleAsync(
        ClaimsPrincipal principal,
        Guid userId,
        AssignPlatformUserRoleRequest request,
        string? confirmationToken = null,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var roleKey = NormalizePlatformRoleKey(request.RoleKey);
        if (RequiresOwnerApproval(roleKey))
        {
            await authorization.RequirePlatformOwnerAsync(principal, cancellationToken);
            RequireSensitiveActionConfirmation(confirmationToken);
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);
        if (request.TenantId is Guid scopedTenantId)
        {
            var tenantExists = await db.Tenants.AsNoTracking().AnyAsync(x => x.Id == scopedTenantId, cancellationToken);
            if (!tenantExists)
            {
                throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
            }
        }

        var actorUserId = principal.GetUserId();
        var existingAssignment = await db.PlatformRoleAssignments.FirstOrDefaultAsync(
            x => x.UserId == userId && x.RoleKey == roleKey && x.TenantId == request.TenantId,
            cancellationToken);
        var wasAlreadyAssigned = existingAssignment is not null;

        if (existingAssignment is null)
        {
            db.PlatformRoleAssignments.Add(new PlatformRoleAssignment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleKey = roleKey,
                TenantId = request.TenantId,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedByUserId = actorUserId,
            });
        }

        if (string.Equals(roleKey, "platform_admin", StringComparison.OrdinalIgnoreCase))
        {
            user.IsPlatformAdmin = true;
            user.ModifiedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "platform.role.assigned",
            "platform_role_assignment",
            user.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        if (!wasAlreadyAssigned)
        {
            await EnqueueUserEventAsync(
                PlatformOutboxEventKinds.UserUpdated,
                actorUserId,
                user,
                $"Platform role assigned: {roleKey}",
                new Dictionary<string, string>
                {
                    ["source"] = "platform_admin_user",
                    ["roleKey"] = roleKey,
                    ["roleAction"] = "assign",
                    ["tenantId"] = request.TenantId?.ToString() ?? string.Empty,
                },
                cancellationToken);
        }

        return new AssignPlatformUserRoleResponse(user.Id, roleKey, wasAlreadyAssigned, request.TenantId);
    }

    public async Task<RemovePlatformUserRoleResponse> RemoveRoleAsync(
        ClaimsPrincipal principal,
        Guid userId,
        string roleKey,
        Guid? tenantId,
        string? confirmationToken = null,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        roleKey = NormalizePlatformRoleKey(roleKey);
        if (RequiresOwnerApproval(roleKey))
        {
            await authorization.RequirePlatformOwnerAsync(principal, cancellationToken);
            RequireSensitiveActionConfirmation(confirmationToken);
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        if (IsBreakGlassUser(user.Email)
            && tenantId is null
            && (string.Equals(roleKey, "platform_admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(roleKey, "platform_owner", StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException(
                "admin.break_glass_protected",
                "Break-glass administrator account cannot have protected platform roles removed.",
                409);
        }

        var actorUserId = principal.GetUserId();
        var assignments = await db.PlatformRoleAssignments
            .Where(x => x.UserId == userId && x.RoleKey == roleKey && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        var wasAlreadyRemoved = assignments.Count == 0;

        if (!wasAlreadyRemoved)
        {
            db.PlatformRoleAssignments.RemoveRange(assignments);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (string.Equals(roleKey, "platform_admin", StringComparison.OrdinalIgnoreCase))
        {
            var stillAdmin = await db.PlatformRoleAssignments.AnyAsync(
                x => x.UserId == userId && x.RoleKey == "platform_admin",
                cancellationToken);
            user.IsPlatformAdmin = stillAdmin;
            user.ModifiedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "platform.role.removed",
            "platform_role_assignment",
            user.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        if (!wasAlreadyRemoved)
        {
            await EnqueueUserEventAsync(
                PlatformOutboxEventKinds.UserUpdated,
                actorUserId,
                user,
                $"Platform role removed: {roleKey}",
                new Dictionary<string, string>
                {
                    ["source"] = "platform_admin_user",
                    ["roleKey"] = roleKey,
                    ["roleAction"] = "remove",
                    ["tenantId"] = tenantId?.ToString() ?? string.Empty,
                },
                cancellationToken);
        }

        return new RemovePlatformUserRoleResponse(user.Id, roleKey, wasAlreadyRemoved, tenantId);
    }

    public async Task<PlatformUserExternalIdentityProviderMappingsResponse> ListExternalIdentityProviderMappingsAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var userExists = await db.Users.AsNoTracking().AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            throw new StlApiException("user.not_found", "Platform user was not found.", 404);
        }

        var items = await db.ExternalIdentityProviderMappings
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.ProviderKey)
            .Select(x => new PlatformUserExternalIdentityProviderMappingItemResponse(
                x.Id,
                x.UserId,
                x.ProviderKey,
                x.ExternalSubject,
                x.ExternalEmail,
                x.CreatedAt,
                x.ModifiedAt))
            .ToListAsync(cancellationToken);

        return new PlatformUserExternalIdentityProviderMappingsResponse(userId, items);
    }

    public async Task<UpsertPlatformUserExternalIdentityProviderMappingResponse> UpsertExternalIdentityProviderMappingAsync(
        ClaimsPrincipal principal,
        Guid userId,
        UpsertPlatformUserExternalIdentityProviderMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var providerKey = NormalizeProviderKey(request.ProviderKey);
        var externalSubject = NormalizeExternalSubject(request.ExternalSubject);
        var externalEmail = NormalizeOptionalEmail(request.ExternalEmail);

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);

        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;

        var mapping = await db.ExternalIdentityProviderMappings
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProviderKey == providerKey, cancellationToken);

        var wasUpdated = mapping is not null;
        if (mapping is null)
        {
            var providerSubjectConflict = await db.ExternalIdentityProviderMappings.AnyAsync(
                x => x.ProviderKey == providerKey && x.ExternalSubject == externalSubject && x.UserId != userId,
                cancellationToken);
            if (providerSubjectConflict)
            {
                throw new StlApiException(
                    "external_identity.mapping_conflict",
                    "An identity mapping already exists for that provider and subject.",
                    409);
            }

            mapping = new ExternalIdentityProviderMapping
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProviderKey = providerKey,
                ExternalSubject = externalSubject,
                ExternalEmail = externalEmail,
                CreatedAt = now,
                ModifiedAt = now,
                CreatedByUserId = actorUserId,
                ModifiedByUserId = actorUserId,
            };
            db.ExternalIdentityProviderMappings.Add(mapping);
        }
        else
        {
            var providerSubjectConflict = await db.ExternalIdentityProviderMappings.AnyAsync(
                x => x.ProviderKey == providerKey
                    && x.ExternalSubject == externalSubject
                    && x.UserId != userId
                    && x.Id != mapping.Id,
                cancellationToken);
            if (providerSubjectConflict)
            {
                throw new StlApiException(
                    "external_identity.mapping_conflict",
                    "An identity mapping already exists for that provider and subject.",
                    409);
            }

            mapping.ExternalSubject = externalSubject;
            mapping.ExternalEmail = externalEmail;
            mapping.ModifiedAt = now;
            mapping.ModifiedByUserId = actorUserId;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            wasUpdated ? "external_identity.mapping_updated" : "external_identity.mapping_added",
            "external_identity_provider_mapping",
            mapping.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        await EnqueueUserEventAsync(
            PlatformOutboxEventKinds.UserUpdated,
            actorUserId,
            user,
            wasUpdated ? "External identity mapping updated." : "External identity mapping added.",
            new Dictionary<string, string>
            {
                ["source"] = "platform_admin_user",
                ["providerKey"] = mapping.ProviderKey,
                ["externalSubject"] = mapping.ExternalSubject,
                ["externalEmail"] = mapping.ExternalEmail ?? string.Empty,
                ["mappingAction"] = wasUpdated ? "update" : "add",
            },
            cancellationToken);

        return new UpsertPlatformUserExternalIdentityProviderMappingResponse(
            mapping.Id,
            mapping.UserId,
            mapping.ProviderKey,
            mapping.ExternalSubject,
            mapping.ExternalEmail,
            wasUpdated);
    }

    public async Task<RemovePlatformUserExternalIdentityProviderMappingResponse> RemoveExternalIdentityProviderMappingAsync(
        ClaimsPrincipal principal,
        Guid userId,
        Guid mappingId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new StlApiException("user.not_found", "Platform user was not found.", 404);
        var mapping = await db.ExternalIdentityProviderMappings
            .FirstOrDefaultAsync(x => x.Id == mappingId && x.UserId == userId, cancellationToken)
            ?? throw new StlApiException("external_identity.mapping_not_found", "Identity provider mapping was not found.", 404);

        db.ExternalIdentityProviderMappings.Remove(mapping);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "external_identity.mapping_removed",
            "external_identity_provider_mapping",
            mapping.Id.ToString(),
            "Success",
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        await EnqueueUserEventAsync(
            PlatformOutboxEventKinds.UserUpdated,
            actorUserId,
            user,
            "External identity mapping removed.",
            new Dictionary<string, string>
            {
                ["source"] = "platform_admin_user",
                ["providerKey"] = mapping.ProviderKey,
                ["externalSubject"] = mapping.ExternalSubject,
                ["mappingAction"] = "remove",
            },
            cancellationToken);

        return new RemovePlatformUserExternalIdentityProviderMappingResponse(userId, mappingId, false);
    }

    private async Task EnqueueUserEventAsync(
        string eventType,
        Guid actorUserId,
        PlatformUser user,
        string summary,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        metadata["email"] = user.Email;
        metadata["displayName"] = user.DisplayName;
        metadata["isActive"] = user.IsActive.ToString();
        metadata["isPlatformAdmin"] = user.IsPlatformAdmin.ToString();
        if (user.Credential is not null)
        {
            metadata["failedLoginCount"] = user.Credential.FailedLoginCount.ToString();
            metadata["lockedUntil"] = user.Credential.LockedUntil?.ToString("O") ?? string.Empty;
        }

        await outboxEnqueue.TryEnqueueAsync(
            eventType,
            "user",
            user.Id.ToString(),
            user.ModifiedAt.ToUnixTimeMilliseconds().ToString(),
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                null,
                actorUserId,
                "user",
                user.Id.ToString(),
                summary,
                metadata),
            cancellationToken: cancellationToken);
    }

    private static PlatformUserDetailResponse MapUser(PlatformUser user) =>
        MapUser(user, DateTimeOffset.UtcNow, null, null);

    private static PlatformUserDetailResponse MapUser(
        PlatformUser user,
        DateTimeOffset now,
        DateTimeOffset? lastLoginAt,
        DateTimeOffset? lastProductLaunchAt)
    {
        var canLogin = user.Credential is not null;
        var status = PlatformUserStatusResolver.Resolve(
            user.IsActive,
            canLogin,
            user.Credential?.IsEmailVerified,
            user.Credential?.RequiresPasswordChange ?? false,
            user.Credential?.LockedUntil,
            now);
        return new PlatformUserDetailResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsActive,
            user.IsPlatformAdmin,
            user.Credential?.FailedLoginCount ?? 0,
            user.Credential?.LockedUntil,
            user.CreatedAt,
            user.ModifiedAt,
            lastLoginAt,
            lastProductLaunchAt,
            canLogin,
            status,
            user.Credential?.IsMfaEnabled ?? false);
    }

    private async Task<Dictionary<Guid, DateTimeOffset>> GetLastUserActivityByActionAsync(
        IReadOnlyCollection<Guid> userIds,
        string action,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var rows = await db.AuditEvents
            .AsNoTracking()
            .Where(x =>
                x.ActorUserId.HasValue
                && userIds.Contains(x.ActorUserId.Value)
                && x.Action == action
                && x.Result == "Success")
            .Select(x => new
            {
                UserId = x.ActorUserId!.Value,
                x.OccurredAt
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.OccurredAt));
    }

    private static string NormalizeEmail(string email)
    {
        var normalized = PasswordResetRules.NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalized) || !normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new StlApiException("user.invalid_email", "Email is required.", 400);
        }

        return normalized;
    }

    private static string NormalizeDisplayName(string displayName)
    {
        var normalized = displayName?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException("user.invalid_display_name", "Display name is required.", 400);
        }

        return normalized.Length <= 200 ? normalized : normalized[..200];
    }

    private async Task ValidatePasswordAsync(string password, CancellationToken cancellationToken)
    {
        var settings = await sessionSettingsService.LoadOrDefaultAsync(cancellationToken);
        if (!PasswordResetRules.MeetsPasswordPolicy(
                password,
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

    private static string NormalizeTenantRoleKey(string roleKey)
    {
        var normalized = roleKey?.Trim().ToLowerInvariant() ?? "tenant_user";
        if (!AllowedTenantRoleKeys.Contains(normalized))
        {
            throw new StlApiException(
                "tenant.invalid_role",
                "Role must be tenant_user or tenant_admin.",
                400);
        }

        return normalized;
    }

    private static string NormalizePlatformRoleKey(string roleKey)
    {
        var normalized = roleKey?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!SupportedPlatformRoleKeys.Contains(normalized))
        {
            throw new StlApiException(
                "platform.invalid_role",
                "Role must be platform_admin.",
                400);
        }

        return normalized;
    }

    private static string NormalizeProviderKey(string providerKey)
    {
        var normalized = providerKey?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "external_identity.invalid_provider",
                "Provider key is required.",
                400);
        }

        return normalized.Length <= 64 ? normalized : normalized[..64];
    }

    private static string NormalizeExternalSubject(string externalSubject)
    {
        var normalized = externalSubject?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "external_identity.invalid_subject",
                "External subject is required.",
                400);
        }

        return normalized.Length <= 256 ? normalized : normalized[..256];
    }

    private static string? NormalizeOptionalEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalized = PasswordResetRules.NormalizeEmail(email);
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new StlApiException(
                "external_identity.invalid_email",
                "External email must be a valid email address when provided.",
                400);
        }

        return normalized.Length <= 320 ? normalized : normalized[..320];
    }

    private static bool RequiresOwnerApproval(string roleKey) =>
        string.Equals(roleKey, "platform_admin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(roleKey, "platform_owner", StringComparison.OrdinalIgnoreCase);

    private static void RequireSensitiveActionConfirmation(string? confirmationToken)
    {
        if (!string.Equals(confirmationToken?.Trim(), SensitiveActionConfirmationValue, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "admin.confirmation_required",
                "Sensitive action confirmation required.",
                409);
        }
    }

    private bool IsBreakGlassUser(string email)
    {
        var normalizedEmail = PasswordResetRules.NormalizeEmail(email);
        var configured =
            configuration["AUTH_BREAK_GLASS_ADMIN_EMAILS"]
            ?? configuration["Auth:BreakGlassAdminEmails"]
            ?? string.Empty;
        if (string.IsNullOrWhiteSpace(configured))
        {
            return false;
        }

        var protectedEmails = configured
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(PasswordResetRules.NormalizeEmail)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return protectedEmails.Contains(normalizedEmail);
    }
}
