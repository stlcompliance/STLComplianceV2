namespace NexArr.Api.Entities;

public sealed class UserCredential
{
    public Guid UserId { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset PasswordChangedAt { get; set; }

    public PlatformUser User { get; set; } = null!;
}
