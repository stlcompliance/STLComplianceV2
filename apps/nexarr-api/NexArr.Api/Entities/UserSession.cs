namespace NexArr.Api.Entities;

public sealed class UserSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string RefreshTokenHash { get; set; } = string.Empty;

    public Guid? ActiveTenantId { get; set; }

    public bool IsRemembered { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public PlatformUser User { get; set; } = null!;
}
