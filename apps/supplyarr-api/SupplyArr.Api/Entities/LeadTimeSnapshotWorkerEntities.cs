using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TenantLeadTimeSnapshotSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int StalenessHours { get; set; } = LeadTimeSnapshotWorkerDefaults.StalenessHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class PartVendorLeadTimeCaptureState : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PartVendorLinkId { get; set; }

    public int? LastCapturedLeadTimeDays { get; set; }

    public Guid? LastLeadTimeSnapshotId { get; set; }

    public DateTimeOffset? LastCapturedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PartVendorLink PartVendorLink { get; set; } = null!;
}

public sealed class LeadTimeSnapshotRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int CapturedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class LeadTimeSnapshotWorkerDefaults
{
    public const int StalenessHours = 24;
}
