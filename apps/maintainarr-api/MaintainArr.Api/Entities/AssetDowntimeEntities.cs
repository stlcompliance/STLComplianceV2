using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class TenantDowntimeTrackingSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public bool AutoTrackOutOfService { get; set; } = true;

    public bool AutoTrackNotReady { get; set; } = true;

    public int AvailabilityPeriodDays { get; set; } = AssetDowntimeDefaults.AvailabilityPeriodDays;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetDowntimeEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string AssetTag { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public bool IsPlanned { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public string? StatusTrigger { get; set; }

    public Guid? WorkOrderId { get; set; }

    public Guid? DefectId { get; set; }

    public string? Notes { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? ClosedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetAvailabilitySnapshot : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string AssetTag { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public DateTimeOffset PeriodStart { get; set; }

    public DateTimeOffset PeriodEnd { get; set; }

    public decimal TotalHours { get; set; }

    public decimal DowntimeHours { get; set; }

    public decimal AvailabilityPercent { get; set; }

    public decimal PlannedDowntimeHours { get; set; }

    public decimal UnplannedDowntimeHours { get; set; }

    public bool HasActiveDowntime { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class FleetAvailabilitySnapshot : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset PeriodStart { get; set; }

    public DateTimeOffset PeriodEnd { get; set; }

    public int AssetCount { get; set; }

    public decimal TotalHours { get; set; }

    public decimal DowntimeHours { get; set; }

    public decimal AvailabilityPercent { get; set; }

    public decimal PlannedDowntimeHours { get; set; }

    public decimal UnplannedDowntimeHours { get; set; }

    public int ActiveDowntimeEventCount { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetDowntimeSyncRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int AssetsScanned { get; set; }

    public int EventsOpened { get; set; }

    public int EventsClosed { get; set; }

    public int SnapshotsRefreshed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class AssetDowntimeDefaults
{
    public const int AvailabilityPeriodDays = 30;

    public const int BatchSize = 50;
}

public static class AssetDowntimeSources
{
    public const string AutomaticStatus = "automatic_status";

    public const string Manual = "manual";
}

public static class AssetDowntimeReasons
{
    public const string OutOfService = "out_of_service";

    public const string RestrictedUse = "restricted_use";

    public const string InRepair = "in_repair";

    public const string AwaitingParts = "awaiting_parts";

    public const string AwaitingTechnician = "awaiting_technician";

    public const string AwaitingVendor = "awaiting_vendor";

    public const string AwaitingApproval = "awaiting_approval";

    public const string FailedInspection = "failed_inspection";

    public const string RegulatoryHold = "regulatory_hold";

    public const string AccidentHold = "accident_hold";

    public const string WarrantyHold = "warranty_hold";

    public const string AwaitingTransport = "awaiting_transport";

    public const string Unknown = "unknown";

    public static readonly IReadOnlySet<string> ManualReasons = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        InRepair,
        AwaitingParts,
        AwaitingTechnician,
        AwaitingVendor,
        AwaitingApproval,
        FailedInspection,
        RegulatoryHold,
        AccidentHold,
        WarrantyHold,
        AwaitingTransport,
        Unknown,
    };
}
