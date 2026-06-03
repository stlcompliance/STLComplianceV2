namespace NexArr.Api.Entities;

public sealed class PlatformSessionSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-0000000000c1");

    public Guid Id { get; set; } = SingletonId;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 7;

    public int RememberedRefreshTokenDays { get; set; } = 7;

    public bool? RequirePlatformAdminMfa { get; set; }

    public int PasswordMinLength { get; set; } = 12;

    public bool RequirePasswordComplexity { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
