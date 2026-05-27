namespace NexArr.Api.Entities;

public sealed class ServiceTokenRecord
{
    public Guid Id { get; set; }

    public Guid ServiceClientId { get; set; }

    public string Jti { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public Guid? TenantId { get; set; }

    public string AllowedProductKeys { get; set; } = string.Empty;

    public string? ActionScope { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public Guid IssuedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ServiceClient ServiceClient { get; set; } = null!;
}
