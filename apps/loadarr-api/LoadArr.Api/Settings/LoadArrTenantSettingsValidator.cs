namespace LoadArr.Api.Settings;

public sealed class LoadArrTenantSettingsValidator
{
    private static readonly IReadOnlySet<string> TaskPriorityModes = Set(
        LoadArrTenantSettingsOptionValues.TaskPriorityManual,
        LoadArrTenantSettingsOptionValues.TaskPriorityFifo,
        LoadArrTenantSettingsOptionValues.TaskPriorityDueTime,
        LoadArrTenantSettingsOptionValues.TaskPriorityMaintenancePriority,
        LoadArrTenantSettingsOptionValues.TaskPriorityDockPriority,
        LoadArrTenantSettingsOptionValues.TaskPriorityExceptionPriority);

    private static readonly IReadOnlySet<string> ShiftCalendarSources = Set(
        LoadArrTenantSettingsOptionValues.ShiftCalendarStaffArr,
        LoadArrTenantSettingsOptionValues.ShiftCalendarLoadArrManual,
        LoadArrTenantSettingsOptionValues.ShiftCalendarNone);

    private static readonly IReadOnlySet<string> LateArrivalBehaviors = Set(
        LoadArrTenantSettingsOptionValues.LateArrivalAllow,
        LoadArrTenantSettingsOptionValues.LateArrivalWarn,
        LoadArrTenantSettingsOptionValues.LateArrivalRequireSupervisor,
        LoadArrTenantSettingsOptionValues.LateArrivalRescheduleRequired);

    private static readonly IReadOnlySet<string> PutawayStrategies = Set(
        LoadArrTenantSettingsOptionValues.PutawayManual,
        LoadArrTenantSettingsOptionValues.PutawayNearestAvailable,
        LoadArrTenantSettingsOptionValues.PutawayFixedBinFirst,
        LoadArrTenantSettingsOptionValues.PutawayZoneFirst,
        LoadArrTenantSettingsOptionValues.PutawayFifo,
        LoadArrTenantSettingsOptionValues.PutawayFefo,
        LoadArrTenantSettingsOptionValues.PutawayMaintenancePriority,
        LoadArrTenantSettingsOptionValues.PutawayCrossDockFirst);

    private static readonly IReadOnlySet<string> InventoryStatuses = Set(
        LoadArrTenantSettingsOptionValues.InventoryStatusAvailable,
        LoadArrTenantSettingsOptionValues.InventoryStatusReceivedPendingPutaway,
        LoadArrTenantSettingsOptionValues.InventoryStatusStaged,
        LoadArrTenantSettingsOptionValues.InventoryStatusInspectionHold,
        LoadArrTenantSettingsOptionValues.InventoryStatusQuarantine,
        LoadArrTenantSettingsOptionValues.InventoryStatusDamaged,
        LoadArrTenantSettingsOptionValues.InventoryStatusAllocated,
        LoadArrTenantSettingsOptionValues.InventoryStatusReserved,
        LoadArrTenantSettingsOptionValues.InventoryStatusCycleCountHold,
        LoadArrTenantSettingsOptionValues.InventoryStatusMaintenanceHandoff,
        LoadArrTenantSettingsOptionValues.InventoryStatusShipped,
        LoadArrTenantSettingsOptionValues.InventoryStatusScrapped);

    private static readonly IReadOnlySet<string> AdvisoryRulingBehaviors = Set(
        LoadArrTenantSettingsOptionValues.AdvisoryIgnore,
        LoadArrTenantSettingsOptionValues.AdvisoryWarn,
        LoadArrTenantSettingsOptionValues.AdvisoryRequireAcknowledgement);

    private static readonly IReadOnlySet<string> FailedRulingBehaviors = Set(
        LoadArrTenantSettingsOptionValues.FailedWarn,
        LoadArrTenantSettingsOptionValues.FailedBlock,
        LoadArrTenantSettingsOptionValues.FailedRequireSupervisorOverride);

    private static readonly IReadOnlySet<string> OfflineConflictPolicies = Set(
        LoadArrTenantSettingsOptionValues.OfflineBlockSync,
        LoadArrTenantSettingsOptionValues.OfflineSupervisorReview,
        LoadArrTenantSettingsOptionValues.OfflineLastWriteWinsNonInventory);

    private static readonly IReadOnlySet<string> LabelSizes = Set(
        LoadArrTenantSettingsOptionValues.Label4x6,
        LoadArrTenantSettingsOptionValues.Label2x1,
        LoadArrTenantSettingsOptionValues.LabelLetter,
        LoadArrTenantSettingsOptionValues.LabelCustom);

    private static readonly IReadOnlySet<string> PrinterRoutingModes = Set(
        LoadArrTenantSettingsOptionValues.PrinterUserSelected,
        LoadArrTenantSettingsOptionValues.PrinterLocationDefault,
        LoadArrTenantSettingsOptionValues.PrinterTaskTypeDefault);

    public LoadArrTenantSettingsValidationResult Validate(LoadArrTenantSettingsSections settings)
    {
        var errors = new List<LoadArrTenantSettingsValidationMessage>();
        var warnings = new List<LoadArrTenantSettingsValidationMessage>();
        var hints = new List<LoadArrTenantSettingsDependencyHint>();

        ValidateWarehouseOperatingModel(settings.WarehouseOperatingModel, errors, warnings, hints);
        ValidateReceiving(settings.Receiving, errors);
        ValidateDockAppointments(settings.DockAppointments, errors, warnings);
        ValidatePutaway(settings.Putaway, settings.Compliance, errors, warnings, hints);
        ValidateInventoryControl(settings.InventoryControl, settings.WarehouseOperatingModel, settings.Movement, errors, warnings);
        ValidateTraceability(settings.Traceability, errors);
        ValidateMovement(settings.Movement, errors, warnings);
        ValidateExceptions(settings.Exceptions, errors);
        ValidateCompliance(settings.Compliance, errors, warnings, hints);
        ValidateTaskAssignment(settings.TaskAssignment, errors, warnings, hints);
        ValidateMobileScanner(settings.MobileScanner, errors, warnings);
        ValidateLabelsAndDocuments(settings.LabelingAndDocuments, settings.Traceability, errors, hints);
        ValidateNotificationsAndEvents(settings.NotificationsAndEvents, warnings, hints);

        return new LoadArrTenantSettingsValidationResult(errors, warnings, hints);
    }

    public IReadOnlyList<string> GetUnacknowledgedWarningCodes(
        LoadArrTenantSettingsValidationResult validation,
        IReadOnlyList<string>? acknowledgedWarnings)
    {
        var acknowledged = new HashSet<string>(acknowledgedWarnings ?? [], StringComparer.OrdinalIgnoreCase);
        return validation.Warnings
            .Select(x => x.Code)
            .Where(code => !acknowledged.Contains(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void ValidateWarehouseOperatingModel(
        WarehouseOperatingModelSettings settings,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsValidationMessage> warnings,
        List<LoadArrTenantSettingsDependencyHint> hints)
    {
        Enum(settings.DefaultTaskPriorityMode, TaskPriorityModes, LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel, "defaultTaskPriorityMode", errors);
        Enum(settings.DefaultShiftCalendarSource, ShiftCalendarSources, LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel, "defaultShiftCalendarSource", errors);

        if (!settings.EnableInventoryMovements && settings.EnablePutaway)
        {
            Error(errors, "loadarr.settings.operating_model.putaway_requires_movements", LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel, "enablePutaway", "Putaway cannot stay enabled when inventory movements are disabled.");
        }

        if (!settings.EnableWarehouseTaskQueues && settings.RequireScanConfirmationForWarehouseTasks)
        {
            Error(errors, "loadarr.settings.operating_model.scan_requires_tasks", LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel, "requireScanConfirmationForWarehouseTasks", "Scan confirmation for warehouse tasks requires warehouse task queues.");
        }

        if (settings.DefaultShiftCalendarSource == LoadArrTenantSettingsOptionValues.ShiftCalendarStaffArr)
        {
            Hint(hints, "loadarr.settings.dependency.staffarr_shift_calendar", LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel, "Shift calendar sourcing uses StaffArr APIs or service-token handoffs.", ["staffarr"]);
        }

        if (settings.DefaultShiftCalendarSource == LoadArrTenantSettingsOptionValues.ShiftCalendarNone)
        {
            Warning(warnings, "loadarr.settings.operating_model.no_shift_calendar", LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel, "defaultShiftCalendarSource", "Disabling shift calendar sourcing can allow task timing that ignores StaffArr availability.");
        }
    }

    private static void ValidateReceiving(ReceivingPolicySettings settings, List<LoadArrTenantSettingsValidationMessage> errors)
    {
        Range(settings.OverReceiptTolerancePercent, 0, 100, LoadArrTenantSettingsSectionKeys.Receiving, "overReceiptTolerancePercent", errors);

        if (!settings.AllowOverReceipt && settings.OverReceiptTolerancePercent != 0)
        {
            Error(errors, "loadarr.settings.receiving.over_receipt_tolerance_requires_over_receipt", LoadArrTenantSettingsSectionKeys.Receiving, "overReceiptTolerancePercent", "Over-receipt tolerance must be zero when over-receipt is disabled.");
        }

        if (settings.AutoRouteReceivingExceptionToAssurArr && !settings.AutoCreateExceptionOnVariance)
        {
            Error(errors, "loadarr.settings.receiving.assurarr_route_requires_exception", LoadArrTenantSettingsSectionKeys.Receiving, "autoRouteReceivingExceptionToAssurArr", "Routing receiving exceptions to AssurArr requires automatic exception creation on variance.");
        }
    }

    private static void ValidateDockAppointments(
        DockAppointmentPolicySettings settings,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsValidationMessage> warnings)
    {
        Range(settings.DefaultAppointmentMinutes, 5, 1440, LoadArrTenantSettingsSectionKeys.DockAppointments, "defaultAppointmentMinutes", errors);
        Min(settings.EarlyArrivalGraceMinutes, 0, LoadArrTenantSettingsSectionKeys.DockAppointments, "earlyArrivalGraceMinutes", errors);
        Min(settings.LateArrivalGraceMinutes, 0, LoadArrTenantSettingsSectionKeys.DockAppointments, "lateArrivalGraceMinutes", errors);
        Min(settings.NoShowThresholdMinutes, 0, LoadArrTenantSettingsSectionKeys.DockAppointments, "noShowThresholdMinutes", errors);
        Enum(settings.LateArrivalBehavior, LateArrivalBehaviors, LoadArrTenantSettingsSectionKeys.DockAppointments, "lateArrivalBehavior", errors);

        if (settings.RequireAppointmentForInboundReceiving && settings.AllowWalkInReceiving)
        {
            Warning(warnings, "loadarr.settings.dock.walk_ins_require_supervised_exception_appointments", LoadArrTenantSettingsSectionKeys.DockAppointments, "allowWalkInReceiving", "Walk-ins may remain enabled with required appointments only when the workflow creates supervised exception appointments.");
        }

        if (settings.AllowSameDockOverlappingAppointments)
        {
            Warning(warnings, "loadarr.settings.dock.overlapping_appointments", LoadArrTenantSettingsSectionKeys.DockAppointments, "allowSameDockOverlappingAppointments", "Overlapping appointments can overbook a StaffArr-owned dock location.");
        }
    }

    private static void ValidatePutaway(
        PutawayPolicySettings putaway,
        ComplianceEnforcementPolicySettings compliance,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsValidationMessage> warnings,
        List<LoadArrTenantSettingsDependencyHint> hints)
    {
        Enum(putaway.DefaultPutawayStrategy, PutawayStrategies, LoadArrTenantSettingsSectionKeys.Putaway, "defaultPutawayStrategy", errors);

        if (putaway.AllowPutawayOverride && !putaway.RequireReasonForPutawayOverride)
        {
            Warning(warnings, "loadarr.settings.putaway.override_without_reason", LoadArrTenantSettingsSectionKeys.Putaway, "requireReasonForPutawayOverride", "Putaway overrides without reasons should be limited to product admins and preserved in audit.");
        }

        if (putaway.RespectComplianceStorageCompatibilityRules && !compliance.EnableComplianceCoreChecks)
        {
            Error(errors, "loadarr.settings.putaway.compliance_storage_requires_core", LoadArrTenantSettingsSectionKeys.Putaway, "respectComplianceStorageCompatibilityRules", "Compliance storage compatibility rules require Compliance Core checks to be enabled.");
        }

        if (putaway.RespectComplianceStorageCompatibilityRules)
        {
            Hint(hints, "loadarr.settings.dependency.compliance_storage", LoadArrTenantSettingsSectionKeys.Putaway, "Storage compatibility must be evaluated through Compliance Core APIs or events.", ["compliancecore"]);
        }
    }

    private static void ValidateInventoryControl(
        InventoryControlPolicySettings inventory,
        WarehouseOperatingModelSettings operatingModel,
        MovementPolicySettings movement,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsValidationMessage> warnings)
    {
        NonNegative(inventory.AdjustmentApprovalThresholdValue, LoadArrTenantSettingsSectionKeys.InventoryControl, "adjustmentApprovalThresholdValue", errors);
        NonNegative(inventory.CycleCountVarianceToleranceQuantity, LoadArrTenantSettingsSectionKeys.InventoryControl, "cycleCountVarianceToleranceQuantity", errors);
        NonNegative(inventory.CycleCountVarianceTolerancePercent, LoadArrTenantSettingsSectionKeys.InventoryControl, "cycleCountVarianceTolerancePercent", errors);
        if (inventory.DefaultCountCadenceDays is not null)
        {
            Min(inventory.DefaultCountCadenceDays.Value, 0, LoadArrTenantSettingsSectionKeys.InventoryControl, "defaultCountCadenceDays", errors);
        }

        Enum(inventory.DefaultInventoryStatusAfterReceipt, InventoryStatuses, LoadArrTenantSettingsSectionKeys.InventoryControl, "defaultInventoryStatusAfterReceipt", errors);
        Enum(inventory.DefaultInventoryStatusForDamagedReceipt, InventoryStatuses, LoadArrTenantSettingsSectionKeys.InventoryControl, "defaultInventoryStatusForDamagedReceipt", errors);
        Enum(inventory.DefaultInventoryStatusForVarianceReceipt, InventoryStatuses, LoadArrTenantSettingsSectionKeys.InventoryControl, "defaultInventoryStatusForVarianceReceipt", errors);

        if (!inventory.EnableStockLedger)
        {
            if (inventory.AllowInventoryAdjustment || operatingModel.EnableStockAdjustments)
            {
                Error(errors, "loadarr.settings.inventory.ledger_required_for_adjustments", LoadArrTenantSettingsSectionKeys.InventoryControl, "enableStockLedger", "Inventory adjustments require stock ledger integrity.");
            }

            if (operatingModel.EnableInventoryMovements || movement.AllowAdHocMovement)
            {
                Error(errors, "loadarr.settings.inventory.ledger_required_for_movements", LoadArrTenantSettingsSectionKeys.InventoryControl, "enableStockLedger", "Inventory movement features require stock ledger integrity.");
            }
        }

        if (inventory.AllowNegativeInventory)
        {
            Warning(warnings, "loadarr.settings.inventory.negative_inventory", LoadArrTenantSettingsSectionKeys.InventoryControl, "allowNegativeInventory", "Negative inventory can obscure physical stock truth and requires product admin confirmation.");
        }
    }

    private static void ValidateTraceability(TraceabilityPolicySettings settings, List<LoadArrTenantSettingsValidationMessage> errors)
    {
        if (string.IsNullOrWhiteSpace(settings.LpnFormat) ||
            !settings.LpnFormat.Contains("{sequence}", StringComparison.OrdinalIgnoreCase) &&
            !settings.LpnFormat.Contains("{guid}", StringComparison.OrdinalIgnoreCase) &&
            !settings.LpnFormat.Contains("{uuid}", StringComparison.OrdinalIgnoreCase) &&
            !settings.LpnFormat.Contains("{ulid}", StringComparison.OrdinalIgnoreCase))
        {
            Error(errors, "loadarr.settings.traceability.lpn_format_requires_unique_token", LoadArrTenantSettingsSectionKeys.Traceability, "lpnFormat", "LPN format must include a sequence or another uniqueness token.");
        }

        if (!settings.EnableLpn && settings.RequireLpnScan)
        {
            Error(errors, "loadarr.settings.traceability.lpn_scan_requires_lpn", LoadArrTenantSettingsSectionKeys.Traceability, "requireLpnScan", "LPN scan cannot be required when LPN is disabled.");
        }

        if (!settings.EnableLpn && settings.AutoGenerateLpn)
        {
            Error(errors, "loadarr.settings.traceability.auto_lpn_requires_lpn", LoadArrTenantSettingsSectionKeys.Traceability, "autoGenerateLpn", "Automatic LPN generation cannot be enabled when LPN is disabled.");
        }
    }

    private static void ValidateMovement(
        MovementPolicySettings settings,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsValidationMessage> warnings)
    {
        if (settings.AllowMovementFromHoldStatus && !settings.RequireApprovalToMoveHeldStock)
        {
            Error(errors, "loadarr.settings.movement.held_stock_movement_requires_approval", LoadArrTenantSettingsSectionKeys.Movement, "requireApprovalToMoveHeldStock", "Held stock movement must require approval and audit.");
        }

        if (settings.AllowMovementOutOfQuarantine && !settings.RequireAssurArrDispositionBeforeReleaseFromQuarantine)
        {
            Warning(warnings, "loadarr.settings.movement.quarantine_release_without_assurarr", LoadArrTenantSettingsSectionKeys.Movement, "requireAssurArrDispositionBeforeReleaseFromQuarantine", "Moving stock out of quarantine without AssurArr disposition requires product admin confirmation.");
        }
    }

    private static void ValidateExceptions(ExceptionHandoffPolicySettings settings, List<LoadArrTenantSettingsValidationMessage> errors)
    {
        if (!settings.AutoCreateAssurArrCase && settings.RequireAssurArrDispositionBeforeRelease && !settings.AllowLocalWarehouseDisposition)
        {
            Error(errors, "loadarr.settings.exceptions.disposition_requires_assurarr_or_local_path", LoadArrTenantSettingsSectionKeys.Exceptions, "requireAssurArrDispositionBeforeRelease", "AssurArr disposition before release requires AssurArr case creation or an approved local disposition path.");
        }

        if (settings.AllowLocalWarehouseDisposition && !settings.RequireSupervisorReview)
        {
            Error(errors, "loadarr.settings.exceptions.local_disposition_requires_supervisor", LoadArrTenantSettingsSectionKeys.Exceptions, "requireSupervisorReview", "Local warehouse disposition requires supervisor review.");
        }
    }

    private static void ValidateCompliance(
        ComplianceEnforcementPolicySettings settings,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsValidationMessage> warnings,
        List<LoadArrTenantSettingsDependencyHint> hints)
    {
        Enum(settings.AdvisoryRulingBehavior, AdvisoryRulingBehaviors, LoadArrTenantSettingsSectionKeys.Compliance, "advisoryRulingBehavior", errors);
        Enum(settings.FailedRulingBehavior, FailedRulingBehaviors, LoadArrTenantSettingsSectionKeys.Compliance, "failedRulingBehavior", errors);

        if (!settings.EnableComplianceCoreChecks && AnyComplianceCheckEnabled(settings))
        {
            Error(errors, "loadarr.settings.compliance.checks_require_core", LoadArrTenantSettingsSectionKeys.Compliance, "enableComplianceCoreChecks", "Compliance check settings must be disabled when Compliance Core checks are disabled.");
        }

        if (!settings.EnableComplianceCoreChecks)
        {
            Warning(warnings, "loadarr.settings.compliance.core_disabled", LoadArrTenantSettingsSectionKeys.Compliance, "enableComplianceCoreChecks", "Disabling Compliance Core checks can allow warehouse execution without compliance rulings.");
        }

        if (string.Equals(settings.FailedRulingBehavior, "ignore", StringComparison.OrdinalIgnoreCase))
        {
            Error(errors, "loadarr.settings.compliance.failed_ruling_cannot_ignore", LoadArrTenantSettingsSectionKeys.Compliance, "failedRulingBehavior", "Failed compliance rulings must never be silently ignored.");
        }

        if (!settings.StoreComplianceDecisionSnapshots)
        {
            Error(errors, "loadarr.settings.compliance.decision_snapshots_required", LoadArrTenantSettingsSectionKeys.Compliance, "storeComplianceDecisionSnapshots", "LoadArr must store compliance decision snapshots when rulings affect warehouse task execution.");
        }

        Hint(hints, "loadarr.settings.dependency.compliance_core", LoadArrTenantSettingsSectionKeys.Compliance, "Compliance meaning and rulings must come from Compliance Core APIs or events.", ["compliancecore"]);
    }

    private static void ValidateTaskAssignment(
        TaskAssignmentPolicySettings settings,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsValidationMessage> warnings,
        List<LoadArrTenantSettingsDependencyHint> hints)
    {
        Min(settings.TaskEscalationThresholdMinutes, 0, LoadArrTenantSettingsSectionKeys.TaskAssignment, "taskEscalationThresholdMinutes", errors);
        Min(settings.TaskAgingWarningThresholdMinutes, 0, LoadArrTenantSettingsSectionKeys.TaskAssignment, "taskAgingWarningThresholdMinutes", errors);
        if (settings.TaskEscalationThresholdMinutes < settings.TaskAgingWarningThresholdMinutes)
        {
            Error(errors, "loadarr.settings.tasks.escalation_before_aging", LoadArrTenantSettingsSectionKeys.TaskAssignment, "taskEscalationThresholdMinutes", "Task escalation threshold must be greater than or equal to the aging warning threshold.");
        }

        if (settings.AutoAssignmentEnabled && !settings.RequireStaffArrActivePersonStatus)
        {
            Warning(warnings, "loadarr.settings.tasks.auto_assignment_without_staffarr_status", LoadArrTenantSettingsSectionKeys.TaskAssignment, "requireStaffArrActivePersonStatus", "Auto-assignment without StaffArr active person checks requires product admin confirmation.");
        }

        if (settings.AutoAssignmentEnabled && !settings.RequireStaffArrPermissionCheck)
        {
            Warning(warnings, "loadarr.settings.tasks.auto_assignment_without_staffarr_permission", LoadArrTenantSettingsSectionKeys.TaskAssignment, "requireStaffArrPermissionCheck", "Auto-assignment without StaffArr permission checks requires product admin confirmation.");
        }

        if (settings.AutoAssignmentEnabled && !settings.RequireTrainArrQualificationCheck)
        {
            Warning(warnings, "loadarr.settings.tasks.auto_assignment_without_trainarr", LoadArrTenantSettingsSectionKeys.TaskAssignment, "requireTrainArrQualificationCheck", "Auto-assignment without TrainArr qualification checks requires product admin confirmation.");
        }

        Hint(hints, "loadarr.settings.dependency.staffarr_task_assignment", LoadArrTenantSettingsSectionKeys.TaskAssignment, "Worker status and permission checks must use StaffArr APIs, events, or service tokens.", ["staffarr"]);
        Hint(hints, "loadarr.settings.dependency.trainarr_task_assignment", LoadArrTenantSettingsSectionKeys.TaskAssignment, "Qualification checks must use TrainArr APIs, events, or service tokens.", ["trainarr"]);
    }

    private static void ValidateMobileScanner(
        MobileScannerPolicySettings settings,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsValidationMessage> warnings)
    {
        Enum(settings.OfflineSyncConflictPolicy, OfflineConflictPolicies, LoadArrTenantSettingsSectionKeys.MobileScanner, "offlineSyncConflictPolicy", errors);

        if (settings.AllowManualBarcodeEntry && !settings.RequireReasonForManualBarcodeEntry)
        {
            Warning(warnings, "loadarr.settings.mobile.manual_entry_without_reason", LoadArrTenantSettingsSectionKeys.MobileScanner, "requireReasonForManualBarcodeEntry", "Manual barcode entry without a reason weakens scanner traceability.");
        }

        if (settings.AllowOfflineTaskExecution)
        {
            Warning(warnings, "loadarr.settings.mobile.offline_execution", LoadArrTenantSettingsSectionKeys.MobileScanner, "allowOfflineTaskExecution", "Offline execution must sync through owning-product validation and must not silently resolve inventory conflicts.");
        }

        if (settings.AllowOfflineTaskExecution &&
            settings.OfflineSyncConflictPolicy == LoadArrTenantSettingsOptionValues.OfflineLastWriteWinsNonInventory)
        {
            Warning(warnings, "loadarr.settings.mobile.last_write_wins_non_inventory_only", LoadArrTenantSettingsSectionKeys.MobileScanner, "offlineSyncConflictPolicy", "Last-write-wins can apply only to non-inventory fields and never to quantity, status, location, or ledger-affecting fields.");
        }
    }

    private static void ValidateLabelsAndDocuments(
        LabelingAndDocumentPolicySettings documents,
        TraceabilityPolicySettings traceability,
        List<LoadArrTenantSettingsValidationMessage> errors,
        List<LoadArrTenantSettingsDependencyHint> hints)
    {
        Enum(documents.DefaultLabelSize, LabelSizes, LoadArrTenantSettingsSectionKeys.LabelingAndDocuments, "defaultLabelSize", errors);
        Enum(documents.DefaultPrinterRoutingMode, PrinterRoutingModes, LoadArrTenantSettingsSectionKeys.LabelingAndDocuments, "defaultPrinterRoutingMode", errors);

        if (documents.GenerateLpnLabels && !traceability.EnableLpn)
        {
            Error(errors, "loadarr.settings.documents.lpn_labels_require_lpn", LoadArrTenantSettingsSectionKeys.LabelingAndDocuments, "generateLpnLabels", "LPN labels require traceability LPN support.");
        }

        if (documents.SendReceivedDocumentPacketToRecordArr)
        {
            Hint(hints, "loadarr.settings.dependency.recordarr_packets", LoadArrTenantSettingsSectionKeys.LabelingAndDocuments, "Document packet handoff must use RecordArr APIs or events, not direct database writes.", ["recordarr"]);
        }
    }

    private static void ValidateNotificationsAndEvents(
        NotificationAndEventPolicySettings settings,
        List<LoadArrTenantSettingsValidationMessage> warnings,
        List<LoadArrTenantSettingsDependencyHint> hints)
    {
        if (!settings.EmitStockLedgerEvents ||
            !settings.EmitReceivingLifecycleEvents ||
            !settings.EmitDockLifecycleEvents ||
            !settings.EmitPutawayLifecycleEvents ||
            !settings.EmitExceptionLifecycleEvents ||
            !settings.EmitTaskLifecycleEvents)
        {
            Warning(warnings, "loadarr.settings.events.lifecycle_events_disabled", LoadArrTenantSettingsSectionKeys.NotificationsAndEvents, "eventEmission", "Internal audit must still persist state changes when lifecycle event emission is disabled.");
        }

        if (settings.NotifyExternalContactsThroughSupplyArr)
        {
            Hint(hints, "loadarr.settings.dependency.external_notifications_supplyarr", LoadArrTenantSettingsSectionKeys.NotificationsAndEvents, "External contacts must be notified through SupplyArr-owned integrations, not LoadArr-owned external party records.", ["supplyarr"]);
        }

        Hint(hints, "loadarr.settings.dependency.owning_product_notifications", LoadArrTenantSettingsSectionKeys.NotificationsAndEvents, "Cross-product notifications must go through owning-product APIs, events, or service tokens.", ["supplyarr", "maintainarr", "routarr", "assurarr", "staffarr", "compliancecore"]);
    }

    private static bool AnyComplianceCheckEnabled(ComplianceEnforcementPolicySettings settings) =>
        settings.CheckComplianceBeforeReceiving ||
        settings.CheckComplianceBeforePutaway ||
        settings.CheckComplianceBeforeMovement ||
        settings.CheckHazardousStorageCompatibility ||
        settings.CheckTemperatureStorageConstraints ||
        settings.CheckPpeRequirementsForHandlingTask ||
        settings.CheckRestrictedMaterialAccess ||
        settings.CheckTrainingRequirementBeforeTaskAssignment ||
        settings.BlockNonQualifiedWorkerAssignment;

    private static void Enum(
        string value,
        IReadOnlySet<string> allowed,
        string sectionKey,
        string fieldPath,
        List<LoadArrTenantSettingsValidationMessage> errors)
    {
        if (!allowed.Contains(value))
        {
            Error(errors, $"loadarr.settings.{sectionKey}.{fieldPath}.invalid_enum", sectionKey, fieldPath, $"Unsupported value '{value}' for {fieldPath}.");
        }
    }

    private static void Range(
        decimal value,
        decimal min,
        decimal max,
        string sectionKey,
        string fieldPath,
        List<LoadArrTenantSettingsValidationMessage> errors)
    {
        if (value < min || value > max)
        {
            Error(errors, $"loadarr.settings.{sectionKey}.{fieldPath}.out_of_range", sectionKey, fieldPath, $"{fieldPath} must be between {min} and {max}.");
        }
    }

    private static void Range(
        int value,
        int min,
        int max,
        string sectionKey,
        string fieldPath,
        List<LoadArrTenantSettingsValidationMessage> errors)
    {
        if (value < min || value > max)
        {
            Error(errors, $"loadarr.settings.{sectionKey}.{fieldPath}.out_of_range", sectionKey, fieldPath, $"{fieldPath} must be between {min} and {max}.");
        }
    }

    private static void Min(
        int value,
        int min,
        string sectionKey,
        string fieldPath,
        List<LoadArrTenantSettingsValidationMessage> errors)
    {
        if (value < min)
        {
            Error(errors, $"loadarr.settings.{sectionKey}.{fieldPath}.negative", sectionKey, fieldPath, $"{fieldPath} must be greater than or equal to {min}.");
        }
    }

    private static void NonNegative(
        decimal? value,
        string sectionKey,
        string fieldPath,
        List<LoadArrTenantSettingsValidationMessage> errors)
    {
        if (value is < 0)
        {
            Error(errors, $"loadarr.settings.{sectionKey}.{fieldPath}.negative", sectionKey, fieldPath, $"{fieldPath} must be non-negative.");
        }
    }

    private static void Error(
        List<LoadArrTenantSettingsValidationMessage> errors,
        string code,
        string sectionKey,
        string fieldPath,
        string message) =>
        errors.Add(new LoadArrTenantSettingsValidationMessage(code, sectionKey, fieldPath, message, "error"));

    private static void Warning(
        List<LoadArrTenantSettingsValidationMessage> warnings,
        string code,
        string sectionKey,
        string fieldPath,
        string message) =>
        warnings.Add(new LoadArrTenantSettingsValidationMessage(code, sectionKey, fieldPath, message, "warning"));

    private static void Hint(
        List<LoadArrTenantSettingsDependencyHint> hints,
        string code,
        string sectionKey,
        string message,
        IReadOnlyList<string> sourceProducts) =>
        hints.Add(new LoadArrTenantSettingsDependencyHint(code, sectionKey, message, sourceProducts));

    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}

