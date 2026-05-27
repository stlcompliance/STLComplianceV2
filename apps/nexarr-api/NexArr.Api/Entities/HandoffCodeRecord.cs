namespace NexArr.Api.Entities;

public sealed class HandoffCodeRecord
{
    public Guid Id { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public Guid SessionId { get; set; }

    public string TargetProductKey { get; set; } = string.Empty;

    public string? CallbackUrl { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RedeemedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public PlatformUser User { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
