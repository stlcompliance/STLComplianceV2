using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class TenantPmDueScanSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int ScanIntervalMinutes { get; set; } = PmDueScanSettingsDefaults.ScanIntervalMinutes;

    public int BatchSize { get; set; } = PmDueScanSettingsDefaults.BatchSize;

    public int OverdueGraceDays { get; set; } = PmDueScanSettingsDefaults.OverdueGraceDays;

    public DateTimeOffset? LastRunAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class PmDueScanRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int MarkedDueCount { get; set; }

    public int MarkedOverdueCount { get; set; }

    public int SkippedCount { get; set; }

    public int WorkOrdersCreatedCount { get; set; }

    public int WorkOrdersLinkedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class PmDueScanSettingsDefaults
{
    public const int ScanIntervalMinutes = 15;

    public const int BatchSize = 100;

    public const int OverdueGraceDays = 1;
}
