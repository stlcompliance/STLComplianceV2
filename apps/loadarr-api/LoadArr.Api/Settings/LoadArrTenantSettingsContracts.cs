using System.Text.Json;

namespace LoadArr.Api.Settings;

public sealed record LoadArrTenantSettingsSections
{
    public WarehouseOperatingModelSettings WarehouseOperatingModel { get; init; } = new();

    public ReceivingPolicySettings Receiving { get; init; } = new();

    public DockAppointmentPolicySettings DockAppointments { get; init; } = new();

    public PutawayPolicySettings Putaway { get; init; } = new();

    public InventoryControlPolicySettings InventoryControl { get; init; } = new();

    public TraceabilityPolicySettings Traceability { get; init; } = new();

    public MovementPolicySettings Movement { get; init; } = new();

    public ExceptionHandoffPolicySettings Exceptions { get; init; } = new();

    public ComplianceEnforcementPolicySettings Compliance { get; init; } = new();

    public TaskAssignmentPolicySettings TaskAssignment { get; init; } = new();

    public MobileScannerPolicySettings MobileScanner { get; init; } = new();

    public LabelingAndDocumentPolicySettings LabelingAndDocuments { get; init; } = new();

    public NotificationAndEventPolicySettings NotificationsAndEvents { get; init; } = new();
}

public sealed record WarehouseOperatingModelSettings
{
    public bool EnableReceiving { get; init; } = true;

    public bool EnablePutaway { get; init; } = true;

    public bool EnableInventoryMovements { get; init; } = true;

    public bool EnableDockScheduling { get; init; } = true;

    public bool EnableCycleCounting { get; init; } = true;

    public bool EnableStockAdjustments { get; init; } = true;

    public bool EnableWarehouseTaskQueues { get; init; } = true;

    public bool EnableMobileScannerWorkflows { get; init; } = true;

    public bool RequireScanConfirmationForWarehouseTasks { get; init; } = true;

    public bool AllowDesktopTaskCompletion { get; init; } = true;

    public string? DefaultWarehouseTimeZone { get; init; }

    public string DefaultTaskPriorityMode { get; init; } = LoadArrTenantSettingsOptionValues.TaskPriorityManual;

    public string DefaultShiftCalendarSource { get; init; } = LoadArrTenantSettingsOptionValues.ShiftCalendarStaffArr;
}

public sealed record ReceivingPolicySettings
{
    public bool AllowPurchaseOrderReceiving { get; init; } = true;

    public bool AllowAsnReceiving { get; init; } = true;

    public bool AllowTransferOrderReceiving { get; init; } = true;

    public bool AllowBlindReceiving { get; init; }

    public bool AllowPartialReceiving { get; init; } = true;

    public bool AllowOverReceipt { get; init; } = true;

    public decimal OverReceiptTolerancePercent { get; init; } = 5m;

    public bool AutoCloseUnderReceipt { get; init; }

    public bool RequireReceiverNotesOnVariance { get; init; } = true;

    public bool RequirePhotoForDamagedReceipt { get; init; } = true;

    public bool RequireAttachmentForExceptionReceipt { get; init; }

    public bool RequireVendorPackingSlip { get; init; }

    public bool RequireBolOrPod { get; init; }

    public bool RequireSupervisorApprovalForVariance { get; init; } = true;

    public bool AutoCreateExceptionOnVariance { get; init; } = true;

    public bool AutoRouteReceivingExceptionToAssurArr { get; init; } = true;

    public bool AutoNotifySupplyArrOnReceiptConfirmation { get; init; } = true;

    public bool AutoNotifyMaintainArrForMaintenanceRequestedParts { get; init; } = true;

    public bool AutoNotifyRoutArrOnCheckInCheckOut { get; init; } = true;
}

public sealed record DockAppointmentPolicySettings
{
    public bool RequireAppointmentForInboundReceiving { get; init; }

    public bool AllowWalkInReceiving { get; init; } = true;

    public int DefaultAppointmentMinutes { get; init; } = 60;

    public int EarlyArrivalGraceMinutes { get; init; } = 15;

    public int LateArrivalGraceMinutes { get; init; } = 15;

    public int NoShowThresholdMinutes { get; init; } = 30;

    public string LateArrivalBehavior { get; init; } = LoadArrTenantSettingsOptionValues.LateArrivalAllow;

    public bool AutoStartDetentionClock { get; init; }

    public bool AutoEndDetentionClock { get; init; } = true;

    public bool AllowDockReassignment { get; init; } = true;

    public bool AllowSameDockOverlappingAppointments { get; init; }

    public bool RequireTrailerNumberAtCheckIn { get; init; } = true;

    public bool RequireDriverNameAtCheckIn { get; init; } = true;

    public bool RequireCarrierAtCheckIn { get; init; } = true;

    public bool RequireSealNumberAtCheckIn { get; init; }

    public bool RequireYardLocationSelectionAtCheckIn { get; init; }

    public bool AcceptRoutArrEtaUpdates { get; init; } = true;

    public bool AcceptRoutArrAppointmentUpdates { get; init; } = true;

    public bool AutoCreateAppointmentFromRoutArrInboundTrip { get; init; } = true;

    public bool AutoCreateAppointmentFromSupplyArrExpectation { get; init; } = true;
}

public sealed record PutawayPolicySettings
{
    public bool EnableDirectedPutaway { get; init; } = true;

    public bool EnableManualPutaway { get; init; } = true;

    public bool AutoCreatePutawayTaskAfterReceipt { get; init; } = true;

    public bool RequireSourceLocationScan { get; init; } = true;

    public bool RequireDestinationLocationScan { get; init; } = true;

    public bool AllowPutawayOverride { get; init; } = true;

    public bool RequireReasonForPutawayOverride { get; init; } = true;

    public string DefaultPutawayStrategy { get; init; } = LoadArrTenantSettingsOptionValues.PutawayManual;

    public bool RespectBinCapacity { get; init; } = true;

    public bool RespectItemStorageRules { get; init; } = true;

    public bool RespectComplianceStorageCompatibilityRules { get; init; } = true;

    public bool AllowMixedSkuBins { get; init; }

    public bool AllowMixedLotBins { get; init; }

    public bool AllowMixedStatusBins { get; init; }

    public bool AllowPartialPutaway { get; init; } = true;

    public bool AutoPrioritizeMaintenanceCriticalParts { get; init; } = true;

    public bool AutoPrioritizeCrossDockMaterial { get; init; } = true;

    public bool RouteInspectionRequiredMaterialToHold { get; init; } = true;
}

public sealed record InventoryControlPolicySettings
{
    public bool EnableStockLedger { get; init; } = true;

    public bool AllowNegativeInventory { get; init; }

    public bool AllowInventoryAdjustment { get; init; } = true;

    public decimal? AdjustmentApprovalThresholdValue { get; init; } = 500m;

    public bool RequireReasonCodeForAdjustment { get; init; } = true;

    public bool RequireAttachmentForAdjustment { get; init; }

    public bool RequireCycleCountApprovalOnVariance { get; init; } = true;

    public decimal? CycleCountVarianceToleranceQuantity { get; init; }

    public decimal? CycleCountVarianceTolerancePercent { get; init; } = 5m;

    public bool EnableAbcCycleCountFrequency { get; init; }

    public int? DefaultCountCadenceDays { get; init; }

    public bool FreezeLocationDuringCount { get; init; } = true;

    public bool AllowMovementDuringCount { get; init; }

    public bool RequireRecountOnVariance { get; init; } = true;

    public bool RequireSupervisorApprovalForWriteOff { get; init; } = true;

    public bool RequireComplianceCheckBeforeReleasingHeldMaterial { get; init; } = true;

    public string DefaultInventoryStatusAfterReceipt { get; init; } = LoadArrTenantSettingsOptionValues.InventoryStatusReceivedPendingPutaway;

    public string DefaultInventoryStatusForDamagedReceipt { get; init; } = LoadArrTenantSettingsOptionValues.InventoryStatusDamaged;

    public string DefaultInventoryStatusForVarianceReceipt { get; init; } = LoadArrTenantSettingsOptionValues.InventoryStatusInspectionHold;
}

public sealed record TraceabilityPolicySettings
{
    public bool EnableLpn { get; init; } = true;

    public bool AutoGenerateLpn { get; init; } = true;

    public string LpnFormat { get; init; } = "LOAD-{yyyyMMdd}-{sequence}";

    public bool RequireLpnScan { get; init; } = true;

    public bool RequirePalletId { get; init; }

    public bool RequireCartonId { get; init; }

    public bool RequireLotWhenItemRequiresLotTracking { get; init; } = true;

    public bool RequireSerialWhenItemRequiresSerialTracking { get; init; } = true;

    public bool RequireExpirationDateWhenItemRequiresExpirationTracking { get; init; } = true;

    public bool RequireManufactureDateWhenItemRequiresManufactureDateTracking { get; init; }

    public bool AllowMixedLotsInLpn { get; init; }

    public bool AllowMixedSkusInLpn { get; init; }

    public bool RequireSealNumberForTrailerOrContainerReceiving { get; init; }

    public bool RequireChainOfCustodyForControlledMaterials { get; init; } = true;
}

public sealed record MovementPolicySettings
{
    public bool AllowAdHocMovement { get; init; } = true;

    public bool RequireMovementReasonCode { get; init; } = true;

    public bool RequireSourceScanForMovement { get; init; } = true;

    public bool RequireDestinationScanForMovement { get; init; } = true;

    public bool AllowMovementFromHoldStatus { get; init; }

    public bool RequireApprovalToMoveHeldStock { get; init; } = true;

    public bool AllowMovementIntoQuarantine { get; init; } = true;

    public bool AllowMovementOutOfQuarantine { get; init; }

    public bool RequireAssurArrDispositionBeforeReleaseFromQuarantine { get; init; } = true;

    public bool EnableReplenishmentTasks { get; init; } = true;

    public bool EnableInternalTransferTasks { get; init; } = true;

    public bool EnableMaintenanceIssueHandoffTasks { get; init; } = true;

    public bool AutoCreateMovementTaskFromMaintainArrRequest { get; init; } = true;

    public bool AutoCreateMovementTaskFromOrdArrFulfillmentNeed { get; init; } = true;

    public bool AutoCreateMovementTaskFromReceivingCompletion { get; init; } = true;
}

public sealed record ExceptionHandoffPolicySettings
{
    public bool EnableReceivingExceptions { get; init; } = true;

    public bool EnableInventoryExceptions { get; init; } = true;

    public bool EnableDamageExceptions { get; init; } = true;

    public bool EnableShortageExceptions { get; init; } = true;

    public bool EnableOverageExceptions { get; init; } = true;

    public bool EnableWrongItemExceptions { get; init; } = true;

    public bool EnableExpiredMaterialExceptions { get; init; } = true;

    public bool EnableFailedInspectionExceptions { get; init; } = true;

    public bool AutoCreateAssurArrCase { get; init; } = true;

    public bool RequireAssurArrDispositionBeforeRelease { get; init; } = true;

    public bool AllowLocalWarehouseDisposition { get; init; }

    public bool RequirePhotoEvidence { get; init; } = true;

    public bool RequireSupervisorReview { get; init; } = true;

    public bool RequireVendorNotificationThroughSupplyArr { get; init; }

    public bool AutoHoldAffectedInventory { get; init; } = true;

    public bool AutoBlockPutawayOnException { get; init; } = true;
}

public sealed record ComplianceEnforcementPolicySettings
{
    public bool EnableComplianceCoreChecks { get; init; } = true;

    public bool CheckComplianceBeforeReceiving { get; init; } = true;

    public bool CheckComplianceBeforePutaway { get; init; } = true;

    public bool CheckComplianceBeforeMovement { get; init; } = true;

    public bool CheckHazardousStorageCompatibility { get; init; } = true;

    public bool CheckTemperatureStorageConstraints { get; init; } = true;

    public bool CheckPpeRequirementsForHandlingTask { get; init; } = true;

    public bool CheckRestrictedMaterialAccess { get; init; } = true;

    public bool CheckTrainingRequirementBeforeTaskAssignment { get; init; } = true;

    public bool BlockNonQualifiedWorkerAssignment { get; init; } = true;

    public string AdvisoryRulingBehavior { get; init; } = LoadArrTenantSettingsOptionValues.AdvisoryRequireAcknowledgement;

    public string FailedRulingBehavior { get; init; } = LoadArrTenantSettingsOptionValues.FailedBlock;

    public bool RequireComplianceOverrideReason { get; init; } = true;

    public bool RequireSupervisorApprovalForComplianceOverride { get; init; } = true;

    public bool StoreComplianceDecisionSnapshots { get; init; } = true;
}

public sealed record TaskAssignmentPolicySettings
{
    public bool ManualTaskAssignmentEnabled { get; init; } = true;

    public bool AutoAssignmentEnabled { get; init; }

    public bool AllowWorkerSelfClaim { get; init; } = true;

    public bool RequireStaffArrActivePersonStatus { get; init; } = true;

    public bool RequireStaffArrPermissionCheck { get; init; } = true;

    public bool RequireTrainArrQualificationCheck { get; init; } = true;

    public bool RespectShiftAvailability { get; init; } = true;

    public bool RespectZoneAssignment { get; init; } = true;

    public bool RespectEquipmentQualification { get; init; } = true;

    public int TaskEscalationThresholdMinutes { get; init; } = 45;

    public int TaskAgingWarningThresholdMinutes { get; init; } = 30;

    public bool AllowTaskReassignment { get; init; } = true;

    public bool RequireReasonForTaskReassignment { get; init; } = true;
}

public sealed record MobileScannerPolicySettings
{
    public bool RequireBarcodeScanForReceipt { get; init; } = true;

    public bool RequireBarcodeScanForPutaway { get; init; } = true;

    public bool RequireBarcodeScanForMovement { get; init; } = true;

    public bool AllowManualBarcodeEntry { get; init; } = true;

    public bool RequireReasonForManualBarcodeEntry { get; init; } = true;

    public bool EnableCameraScanning { get; init; } = true;

    public bool EnableExternalScannerSupport { get; init; } = true;

    public bool AllowOfflineTaskExecution { get; init; }

    public string OfflineSyncConflictPolicy { get; init; } = LoadArrTenantSettingsOptionValues.OfflineBlockSync;

    public bool RequirePhotoCaptureForDamage { get; init; } = true;

    public bool RequireSignatureCapture { get; init; }

    public bool RequireLocationConfirmation { get; init; }

    public bool RequireSecondPersonVerificationForHighRiskMoves { get; init; }
}

public sealed record LabelingAndDocumentPolicySettings
{
    public bool GenerateReceivingLabels { get; init; } = true;

    public bool GeneratePalletLabels { get; init; } = true;

    public bool GenerateBinLabels { get; init; }

    public bool GenerateQuarantineLabels { get; init; } = true;

    public bool GenerateLpnLabels { get; init; } = true;

    public string DefaultLabelSize { get; init; } = LoadArrTenantSettingsOptionValues.Label4x6;

    public string DefaultPrinterRoutingMode { get; init; } = LoadArrTenantSettingsOptionValues.PrinterUserSelected;

    public bool RequireLabelPrintBeforePutaway { get; init; }

    public bool AttachPackingSlipToReceipt { get; init; } = true;

    public bool AttachBolOrPodToReceipt { get; init; } = true;

    public bool SendReceivedDocumentPacketToRecordArr { get; init; } = true;

    public bool GenerateReceivingSummaryPdf { get; init; } = true;

    public bool GenerateDiscrepancyReport { get; init; } = true;

    public bool GeneratePutawayReport { get; init; }
}

public sealed record NotificationAndEventPolicySettings
{
    public bool NotifySupplyArrOnReceipt { get; init; } = true;

    public bool NotifySupplyArrOnVariance { get; init; } = true;

    public bool NotifyMaintainArrWhenPartsArrive { get; init; } = true;

    public bool NotifyRoutArrOnCheckInCheckOut { get; init; } = true;

    public bool NotifyAssurArrOnDamageOrQualityException { get; init; } = true;

    public bool NotifyStaffArrOnWorkerIncident { get; init; } = true;

    public bool NotifyComplianceCoreOnComplianceRelevantWarehouseEvent { get; init; } = true;

    public bool NotifyExternalContactsThroughSupplyArr { get; init; }

    public bool EmitStockLedgerEvents { get; init; } = true;

    public bool EmitReceivingLifecycleEvents { get; init; } = true;

    public bool EmitDockLifecycleEvents { get; init; } = true;

    public bool EmitPutawayLifecycleEvents { get; init; } = true;

    public bool EmitExceptionLifecycleEvents { get; init; } = true;

    public bool EmitTaskLifecycleEvents { get; init; } = true;
}

public sealed record LoadArrTenantSettingsResponse(
    int Version,
    string RowVersion,
    DateTimeOffset CreatedAt,
    string? CreatedByPersonId,
    DateTimeOffset UpdatedAt,
    string? UpdatedByPersonId,
    string? UpdatedByDisplayNameSnapshot,
    LoadArrTenantSettingsSections Settings,
    LoadArrTenantSettingsValidationResult Validation);

public sealed record LoadArrTenantSettingsReplaceRequest(
    string RowVersion,
    LoadArrTenantSettingsSections Settings,
    string? Reason = null,
    IReadOnlyList<string>? WarningsAcknowledged = null);

public sealed record LoadArrTenantSettingsSectionPatchRequest(
    string RowVersion,
    JsonElement Section,
    string? Reason = null,
    IReadOnlyList<string>? WarningsAcknowledged = null);

public sealed record LoadArrTenantSettingsResetRequest(
    string RowVersion,
    string? Reason = null,
    IReadOnlyList<string>? WarningsAcknowledged = null);

public sealed record LoadArrTenantSettingsFullResetRequest(
    string RowVersion,
    string ConfirmationPhrase,
    string? Reason = null,
    IReadOnlyList<string>? WarningsAcknowledged = null);

public sealed record LoadArrTenantSettingsAuditEntryResponse(
    int SettingsVersionBefore,
    int SettingsVersionAfter,
    string SectionKey,
    string? ChangedByPersonId,
    string? ChangedByDisplayNameSnapshot,
    DateTimeOffset ChangedAt,
    string? Reason,
    string ChangeSource,
    IReadOnlyList<string> ChangedFields,
    IReadOnlyList<string> WarningsAcknowledged,
    string BeforeSummary,
    string AfterSummary);

public sealed record LoadArrTenantSettingsAuditListResponse(
    IReadOnlyList<LoadArrTenantSettingsAuditEntryResponse> Items,
    int Total,
    int Limit,
    int Offset);

public sealed record LoadArrTenantSettingsValidationResult(
    IReadOnlyList<LoadArrTenantSettingsValidationMessage> Errors,
    IReadOnlyList<LoadArrTenantSettingsValidationMessage> Warnings,
    IReadOnlyList<LoadArrTenantSettingsDependencyHint> DependencyHints)
{
    public static LoadArrTenantSettingsValidationResult Empty { get; } = new([], [], []);
}

public sealed record LoadArrTenantSettingsValidationMessage(
    string Code,
    string SectionKey,
    string FieldPath,
    string Message,
    string Severity);

public sealed record LoadArrTenantSettingsDependencyHint(
    string Code,
    string SectionKey,
    string Message,
    IReadOnlyList<string> SourceProducts);

public sealed record LoadArrTenantSettingsOptionsResponse(
    IReadOnlyList<LoadArrTenantSettingsSectionOption> Sections,
    IReadOnlyDictionary<string, IReadOnlyList<LoadArrTenantSettingsEnumOption>> EnumOptions,
    IReadOnlyList<string> EventNames);

public sealed record LoadArrTenantSettingsSectionOption(
    string Key,
    string Label,
    string Description,
    object DefaultValue,
    IReadOnlyList<LoadArrTenantSettingsFieldOption> Fields);

public sealed record LoadArrTenantSettingsFieldOption(
    string Key,
    string Label,
    string InputType,
    decimal? Min = null,
    decimal? Max = null,
    string? EnumKey = null,
    bool Risky = false);

public sealed record LoadArrTenantSettingsEnumOption(
    string Value,
    string Label,
    string Description,
    bool Risky = false);

public sealed record LoadArrTenantSettingsExportResponse(
    int Version,
    DateTimeOffset ExportedAt,
    LoadArrTenantSettingsSections Settings,
    IReadOnlyList<LoadArrTenantSettingsAuditEntryResponse> RecentAuditEntries);

public sealed record LoadArrTenantSettingsProblemResponse(
    string ErrorCode,
    string Message,
    LoadArrTenantSettingsValidationResult? Validation = null);

public static class LoadArrTenantSettingsSectionKeys
{
    public const string WarehouseOperatingModel = "warehouseOperatingModel";

    public const string Receiving = "receiving";

    public const string DockAppointments = "dockAppointments";

    public const string Putaway = "putaway";

    public const string InventoryControl = "inventoryControl";

    public const string Traceability = "traceability";

    public const string Movement = "movement";

    public const string Exceptions = "exceptions";

    public const string Compliance = "compliance";

    public const string TaskAssignment = "taskAssignment";

    public const string MobileScanner = "mobileScanner";

    public const string LabelingAndDocuments = "labelingAndDocuments";

    public const string NotificationsAndEvents = "notificationsAndEvents";

    public const string All = "all";

    public static IReadOnlyList<string> AllSectionKeys { get; } =
    [
        WarehouseOperatingModel,
        Receiving,
        DockAppointments,
        Putaway,
        InventoryControl,
        Traceability,
        Movement,
        Exceptions,
        Compliance,
        TaskAssignment,
        MobileScanner,
        LabelingAndDocuments,
        NotificationsAndEvents
    ];
}

public static class LoadArrTenantSettingsOptionValues
{
    public const string TaskPriorityManual = "manual";
    public const string TaskPriorityFifo = "fifo";
    public const string TaskPriorityDueTime = "due_time";
    public const string TaskPriorityMaintenancePriority = "maintenance_priority";
    public const string TaskPriorityDockPriority = "dock_priority";
    public const string TaskPriorityExceptionPriority = "exception_priority";

    public const string ShiftCalendarStaffArr = "staffarr";
    public const string ShiftCalendarLoadArrManual = "loadarr_manual";
    public const string ShiftCalendarNone = "none";

    public const string LateArrivalAllow = "allow";
    public const string LateArrivalWarn = "warn";
    public const string LateArrivalRequireSupervisor = "require_supervisor";
    public const string LateArrivalRescheduleRequired = "reschedule_required";

    public const string PutawayManual = "manual";
    public const string PutawayNearestAvailable = "nearest_available";
    public const string PutawayFixedBinFirst = "fixed_bin_first";
    public const string PutawayZoneFirst = "zone_first";
    public const string PutawayFifo = "fifo";
    public const string PutawayFefo = "fefo";
    public const string PutawayMaintenancePriority = "maintenance_priority";
    public const string PutawayCrossDockFirst = "cross_dock_first";

    public const string InventoryStatusAvailable = "available";
    public const string InventoryStatusReceivedPendingPutaway = "received_pending_putaway";
    public const string InventoryStatusStaged = "staged";
    public const string InventoryStatusInspectionHold = "inspection_hold";
    public const string InventoryStatusQuarantine = "quarantine";
    public const string InventoryStatusDamaged = "damaged";
    public const string InventoryStatusAllocated = "allocated";
    public const string InventoryStatusReserved = "reserved";
    public const string InventoryStatusCycleCountHold = "cycle_count_hold";
    public const string InventoryStatusMaintenanceHandoff = "maintenance_handoff";
    public const string InventoryStatusShipped = "shipped";
    public const string InventoryStatusScrapped = "scrapped";

    public const string AdvisoryIgnore = "ignore";
    public const string AdvisoryWarn = "warn";
    public const string AdvisoryRequireAcknowledgement = "require_acknowledgement";

    public const string FailedWarn = "warn";
    public const string FailedBlock = "block";
    public const string FailedRequireSupervisorOverride = "require_supervisor_override";

    public const string OfflineBlockSync = "block_sync";
    public const string OfflineSupervisorReview = "supervisor_review";
    public const string OfflineLastWriteWinsNonInventory = "last_write_wins_for_non_inventory_fields";

    public const string Label4x6 = "label_4x6";
    public const string Label2x1 = "label_2x1";
    public const string LabelLetter = "letter";
    public const string LabelCustom = "custom";

    public const string PrinterUserSelected = "user_selected";
    public const string PrinterLocationDefault = "location_default";
    public const string PrinterTaskTypeDefault = "task_type_default";
}
