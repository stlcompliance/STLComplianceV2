using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class TenantMaintenanceHistoryRollupSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int StalenessHours { get; set; } = MaintenanceHistoryRollupDefaults.StalenessHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class MaintenanceHistoryRollup : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string AssetTag { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public int EventCount { get; set; }

    public int InspectionCount { get; set; }

    public int DefectCount { get; set; }

    public int WorkOrderCount { get; set; }

    public int PmCount { get; set; }

    public DateTimeOffset? LastEventAt { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<MaintenanceHistoryEvent> Events { get; set; } = [];
}

public sealed class MaintenanceHistoryEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public Guid RollupId { get; set; }

    public string EntryId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public Guid? ActorUserId { get; set; }

    public string SourceEntityType { get; set; } = string.Empty;

    public string SourceEntityId { get; set; } = string.Empty;

    public string? RelatedEntityId { get; set; }

    public MaintenanceHistoryRollup Rollup { get; set; } = null!;
}

public sealed class MaintenanceHistoryRollupRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int RefreshedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class MaintenanceHistoryRollupDefaults
{
    public const int StalenessHours = 1;
}
