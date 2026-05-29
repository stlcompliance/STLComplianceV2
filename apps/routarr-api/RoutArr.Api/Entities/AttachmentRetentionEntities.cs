using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TenantAttachmentRetentionSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int RetentionDaysAfterTripClose { get; set; } = AttachmentRetentionDefaults.RetentionDaysAfterTripClose;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AttachmentRetentionRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Outcome { get; set; } = string.Empty;

    public int AttachmentsPurgedCount { get; set; }

    public long BytesReclaimed { get; set; }

    public int SkippedCount { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class AttachmentRetentionDefaults
{
    public const int RetentionDaysAfterTripClose = 365;
}
