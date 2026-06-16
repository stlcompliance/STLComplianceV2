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

    public const string InspectionPaused = "inspection.paused";

    public const string InspectionResumed = "inspection.resumed";

    public const string InspectionAnswerSubmitted = "inspection.answer_submitted";

    public const string InspectionCompleted = "inspection.completed";

    public const string InspectionFailed = "inspection.failed";

    public const string InspectionDefectCreated = "inspection.defect_created";

    public const string DefectCreated = "defect.created";

    public const string DefectRepaired = "defect.repaired";

    public const string DefectClosed = "defect.closed";

    public const string WorkOrderCreated = "work_order.created";

    public const string WorkOrderRequested = "work_order.requested";

    public const string WorkOrderTriaged = "work_order.triaged";

    public const string WorkOrderApproved = "work_order.approved";

    public const string WorkOrderRejected = "work_order.rejected";

    public const string WorkOrderPlanned = "work_order.planned";

    public const string WorkOrderAssigned = "work_order.assigned";

    public const string WorkOrderScheduled = "maintainarr.workOrder.scheduled";

    public const string WorkOrderRescheduled = "maintainarr.workOrder.rescheduled";

    public const string WorkOrderUnscheduled = "maintainarr.workOrder.unscheduled";

    public const string WorkOrderStarted = "work_order.started";

    public const string WorkOrderPaused = "work_order.paused";

    public const string WorkOrderCompleted = "work_order.completed";

    public const string WorkOrderBlocked = "work_order.blocked";

    public const string WorkOrderUnblocked = "work_order.unblocked";

    public const string WorkOrderClosed = "work_order.closed";

    public const string WorkOrderCanceled = "work_order.canceled";

    public const string LaborEntryCreated = "labor_entry.created";

    public const string LaborEntrySubmitted = "labor_entry.submitted";

    public const string LaborEntryApproved = "labor_entry.approved";

    public const string LaborEntryRejected = "labor_entry.rejected";

    public const string MaintenanceVendorWorkCreated = "vendor_work.created";

    public const string MaintenanceVendorWorkCompleted = "vendor_work.completed";

    public const string PmDue = "pm.due";

    public const string PmOverdue = "pm.overdue";

    public const string PmPlanCreated = "pm_plan.created";

    public const string PmPlanActivated = "pm_plan.activated";

    public const string PmOccurrenceCreated = "pm_occurrence.created";

    public const string PmOccurrenceDue = "pm_occurrence.due";

    public const string PmOccurrenceOverdue = "pm_occurrence.overdue";

    public const string PmOccurrenceWorkOrderGenerated = "pm_occurrence.work_order_generated";

    public const string PmOccurrenceInspectionGenerated = "pm_occurrence.inspection_generated";

    public const string PmOccurrenceCompleted = "pm_occurrence.completed";

    public const string PmOccurrenceSkipped = "pm_occurrence.skipped";

    public const string MeterReadingRecorded = "meter_reading.recorded";

    public const string MeterReadingRejected = "meter_reading.rejected";

    public const string ComponentCreated = "component.created";

    public const string ComponentInstalled = "component.installed";

    public const string ComponentRemoved = "component.removed";

    public const string ComponentFailed = "component.failed";

    public const string ComponentReplaced = "component.replaced";

    public const string ComponentRetired = "component.retired";
}

public static class MaintenancePlatformEventRelatedEntityTypes
{
    public const string Asset = "asset";

    public const string InspectionRun = "inspection_run";

    public const string Defect = "defect";

    public const string WorkOrder = "work_order";

    public const string LaborEntry = "labor_entry";

    public const string VendorWork = "vendor_work";

    public const string PmSchedule = "pm_schedule";

    public const string PmOccurrence = "pm_occurrence";

    public const string MeterReading = "meter_reading";

    public const string Component = "component";
}
