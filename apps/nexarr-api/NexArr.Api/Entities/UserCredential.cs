namespace NexArr.Api.Entities;

public sealed class UserCredential
{
    public Guid UserId { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset PasswordChangedAt { get; set; }

    public bool RequiresPasswordChange { get; set; }

    public bool IsEmailVerified { get; set; } = true;

    public bool IsMfaEnabled { get; set; }

    public string? MfaSecret { get; set; }

    public string? MfaRecoveryCodeHashesJson { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTimeOffset? LockedUntil { get; set; }

    public PlatformUser User { get; set; } = null!;
}
