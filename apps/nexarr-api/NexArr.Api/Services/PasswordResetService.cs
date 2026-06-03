using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PasswordResetService(
    NexArrDbContext db,
    IPasswordHasher passwordHasher,
    IPlatformAuditService audit,
    IHostEnvironment hostEnvironment,
    PlatformSessionSettingsService sessionSettingsService)
{
    public async Task<ForgotPasswordResponse> RequestForgotAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = PasswordResetRules.NormalizeEmail(request.Email);
        var user = await db.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        string? devResetToken = null;

        if (user is not null && user.IsActive && user.Credential is not null)
        {
            var now = DateTimeOffset.UtcNow;
            var plaintextToken = GenerateResetToken();
            var tokenHash = HashResetToken(plaintextToken);

            var pendingTokens = await db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.UsedAt == null && t.ExpiresAt > now)
                .ToListAsync(cancellationToken);
            foreach (var pending in pendingTokens)
            {
                pending.UsedAt = now;
            }

            db.PasswordResetTokens.Add(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = now.AddMinutes(PasswordResetRules.TokenLifetimeMinutes),
                CreatedAt = now,
            });

            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "auth.password_reset_requested",
                "user",
                user.Id.ToString(),
                "Success",
                actorUserId: user.Id,
                cancellationToken: cancellationToken);

            if (hostEnvironment.IsDevelopment() || string.Equals(
                    hostEnvironment.EnvironmentName,
                    "Testing",
                    StringComparison.OrdinalIgnoreCase))
            {
                devResetToken = plaintextToken;
            }
        }

        return new ForgotPasswordResponse(
            "If an account exists for that email, password reset instructions have been sent.",
            devResetToken);
    }

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await sessionSettingsService.LoadOrDefaultAsync(cancellationToken);
        if (!PasswordResetRules.MeetsPasswordPolicy(
                request.NewPassword,
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

        var tokenHash = HashResetToken(request.Token.Trim());
        var now = DateTimeOffset.UtcNow;

        var record = await db.PasswordResetTokens
            .Include(t => t.User)
            .ThenInclude(u => u.Credential)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (record is null
            || record.UsedAt is not null
            || record.ExpiresAt <= now
            || record.User.Credential is null
            || !record.User.IsActive)
        {
            await audit.WriteAsync(
                "auth.password_reset_completed",
                "user",
                record?.UserId.ToString() ?? "unknown",
                "Denied",
                reasonCode: "invalid_token",
                cancellationToken: cancellationToken);
            throw new StlApiException(
                "auth.invalid_reset_token",
                "Password reset link is invalid or has expired.",
                400);
        }

        record.UsedAt = now;
        record.User.Credential.PasswordHash = passwordHasher.Hash(request.NewPassword);
        record.User.Credential.PasswordChangedAt = now;
        record.User.ModifiedAt = now;

        var activeSessions = await db.UserSessions
            .Where(s => s.UserId == record.UserId && s.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var session in activeSessions)
        {
            session.RevokedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "auth.password_reset_completed",
            "user",
            record.UserId.ToString(),
            "Success",
            actorUserId: record.UserId,
            cancellationToken: cancellationToken);
    }

    public static string GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string HashResetToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
