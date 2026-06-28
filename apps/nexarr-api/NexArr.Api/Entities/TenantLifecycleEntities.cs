namespace NexArr.Api.Entities;

public sealed class PlatformTenantLifecycleSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-00000000000a");

    public Guid Id { get; set; } = SingletonId;

    public bool IsEnabled { get; set; }

    public bool AutoSuspendWhenNoValidLicense { get; set; }

    public int SuspendGraceDaysAfterLastLicenseExpiry { get; set; } = 7;

    public bool AutoReactivateWhenValidLicense { get; set; }

    public bool RevokeSessionsOnSuspend { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TenantLifecycleRun
{
    public Guid Id { get; set; }

    public string Outcome { get; set; } = string.Empty;

    public int PendingCount { get; set; }

    public int SuspendedCount { get; set; }

    public int ReactivatedCount { get; set; }

    public int SessionsRevokedCount { get; set; }

    public int SkippedCount { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
