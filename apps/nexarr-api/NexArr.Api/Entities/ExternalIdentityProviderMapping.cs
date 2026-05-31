namespace NexArr.Api.Entities;

public sealed class ExternalIdentityProviderMapping
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string ExternalSubject { get; set; } = string.Empty;

    public string? ExternalEmail { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public Guid ModifiedByUserId { get; set; }

    public PlatformUser User { get; set; } = null!;
}
