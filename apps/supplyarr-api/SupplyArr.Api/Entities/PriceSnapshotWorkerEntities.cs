using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TenantPriceSnapshotSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int StalenessHours { get; set; } = PriceSnapshotWorkerDefaults.StalenessHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class PartVendorPriceCaptureState : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PartVendorLinkId { get; set; }

    public decimal? LastCapturedUnitPrice { get; set; }

    public string LastCapturedCurrencyCode { get; set; } = "USD";

    public decimal? LastCapturedMinimumOrderQuantity { get; set; }

    public Guid? LastPricingSnapshotId { get; set; }

    public DateTimeOffset? LastCapturedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PartVendorLink PartVendorLink { get; set; } = null!;
}

public sealed class PriceSnapshotRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int CapturedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class PriceSnapshotWorkerDefaults
{
    public const int StalenessHours = 24;
}
