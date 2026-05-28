namespace NexArr.Api.Entities;

public sealed class ServiceTokenCleanupRun
{
    public Guid Id { get; set; }

    public string Outcome { get; set; } = string.Empty;

    public int PurgedCount { get; set; }

    public int ExpiredPurgeCount { get; set; }

    public int RevokedPurgeCount { get; set; }

    public int SkippedCount { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
