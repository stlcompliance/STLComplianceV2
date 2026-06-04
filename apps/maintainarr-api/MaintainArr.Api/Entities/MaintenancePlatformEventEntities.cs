using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class TenantMaintenancePlatformEventSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int MaxAttempts { get; set; } = 5;

    public int RetryIntervalMinutes { get; set; } = 15;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class MaintenancePlatformOutboxEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string ProcessingStatus { get; set; } = MaintenancePlatformEventStatuses.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}

public sealed class MaintenancePlatformEventProcessingRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public int PendingFound { get; set; }

    public int ProcessedCount { get; set; }

    public int RetriedCount { get; set; }

    public int AbandonedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class MaintenancePlatformEventStatuses
{
    public const string Pending = "pending";

    public const string Processed = "processed";

    public const string Abandoned = "abandoned";
}

public static class MaintenancePlatformOutboxEventKinds
{
    public const string AssetReadinessChanged = "asset.readiness_changed";

    public const string AssetOutOfService = "asset.out_of_service";

    public const string AssetReturnedToService = "asset.returned_to_service";

    public const string InspectionStarted = "inspection.started";

    public const string InspectionAnswerSubmitted = "inspection.answer_submitted";

    public const string InspectionCompleted = "inspection.completed";

    public const string InspectionFailed = "inspection.failed";

    public const string DefectCreated = "defect.created";

    public const string DefectRepaired = "defect.repaired";

    public const string DefectClosed = "defect.closed";

    public const string WorkOrderCreated = "work_order.created";

    public const string WorkOrderAssigned = "work_order.assigned";

    public const string WorkOrderStarted = "work_order.started";

    public const string WorkOrderCompleted = "work_order.completed";

    public const string WorkOrderBlocked = "work_order.blocked";

    public const string WorkOrderUnblocked = "work_order.unblocked";

    public const string PmDue = "pm.due";

    public const string PmOverdue = "pm.overdue";

    public const string PmPlanCreated = "pm_plan.created";

    public const string PmPlanActivated = "pm_plan.activated";

    public const string MeterReadingRecorded = "meter_reading.recorded";

    public const string MeterReadingRejected = "meter_reading.rejected";
}

public static class MaintenancePlatformEventRelatedEntityTypes
{
    public const string Asset = "asset";

    public const string InspectionRun = "inspection_run";

    public const string Defect = "defect";

    public const string WorkOrder = "work_order";

    public const string PmSchedule = "pm_schedule";

    public const string MeterReading = "meter_reading";
}
