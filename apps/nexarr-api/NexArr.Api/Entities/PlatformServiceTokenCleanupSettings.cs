namespace NexArr.Api.Entities;

public sealed class PlatformServiceTokenCleanupSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; set; } = SingletonId;

    public bool IsEnabled { get; set; }

    public int RetentionDaysAfterExpiry { get; set; } = 7;

    public int RetentionDaysAfterRevoke { get; set; } = 30;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
