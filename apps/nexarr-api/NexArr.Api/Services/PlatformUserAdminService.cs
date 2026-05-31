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
    IPasswordHasher passwordHasher,
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
{
    public async Task<PlatformUserDetailResponse> CreateUserAsync(
        ClaimsPrincipal principal,
        CreatePlatformUserRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var email = NormalizeEmail(request.Email);
        var displayName = NormalizeDisplayName(request.DisplayName);
        ValidatePassword(request.Password);

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

    public async Task<PlatformUserLockResponse> LockUserAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

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
        new(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsActive,
            user.IsPlatformAdmin,
            user.Credential?.FailedLoginCount ?? 0,
            user.Credential?.LockedUntil,
            user.CreatedAt,
            user.ModifiedAt);

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

    private static void ValidatePassword(string password)
    {
        if (!PasswordResetRules.MeetsPasswordPolicy(password))
        {
            throw new StlApiException(
                "auth.password_policy",
                PasswordResetRules.PasswordPolicyMessage(),
                400);
        }
    }
}
