namespace LoadArr.Api.Settings;

public sealed class LoadArrTenantSettingsDefaults
{
    public LoadArrTenantSettingsSections CreateDefaultSettings(string? tenantTimeZone = null) =>
        new()
        {
            WarehouseOperatingModel = new WarehouseOperatingModelSettings
            {
                DefaultWarehouseTimeZone = tenantTimeZone,
                DefaultTaskPriorityMode = LoadArrTenantSettingsOptionValues.TaskPriorityManual,
                DefaultShiftCalendarSource = LoadArrTenantSettingsOptionValues.ShiftCalendarStaffArr
            },
            Receiving = new ReceivingPolicySettings
            {
                AllowBlindReceiving = false,
                AllowPartialReceiving = true,
                AllowOverReceipt = true,
                OverReceiptTolerancePercent = 5m,
                AutoCreateExceptionOnVariance = true
            },
            DockAppointments = new DockAppointmentPolicySettings
            {
                RequireAppointmentForInboundReceiving = false,
                AllowWalkInReceiving = true,
                DefaultAppointmentMinutes = 60,
                LateArrivalGraceMinutes = 15
            },
            Putaway = new PutawayPolicySettings
            {
                AutoCreatePutawayTaskAfterReceipt = true,
                EnableDirectedPutaway = true,
                RequireDestinationLocationScan = true,
                AllowPutawayOverride = true,
                RequireReasonForPutawayOverride = true
            },
            InventoryControl = new InventoryControlPolicySettings
            {
                EnableStockLedger = true,
                AllowNegativeInventory = false,
                RequireReasonCodeForAdjustment = true,
                AdjustmentApprovalThresholdValue = 500m
            },
            Traceability = new TraceabilityPolicySettings
            {
                EnableLpn = true,
                AutoGenerateLpn = true,
                AllowMixedLotsInLpn = false
            },
            Exceptions = new ExceptionHandoffPolicySettings
            {
                AutoCreateAssurArrCase = true,
                AutoHoldAffectedInventory = true,
                RequirePhotoEvidence = true
            },
            Compliance = new ComplianceEnforcementPolicySettings
            {
                EnableComplianceCoreChecks = true,
                BlockNonQualifiedWorkerAssignment = true,
                RequireComplianceOverrideReason = true
            },
            TaskAssignment = new TaskAssignmentPolicySettings
            {
                AllowWorkerSelfClaim = true,
                RequireStaffArrActivePersonStatus = true,
                RequireTrainArrQualificationCheck = true
            },
            MobileScanner = new MobileScannerPolicySettings
            {
                RequireBarcodeScanForReceipt = true,
                RequireBarcodeScanForPutaway = true,
                AllowManualBarcodeEntry = true,
                RequireReasonForManualBarcodeEntry = true
            },
            LabelingAndDocuments = new LabelingAndDocumentPolicySettings
            {
                SendReceivedDocumentPacketToRecordArr = true
            },
            NotificationsAndEvents = new NotificationAndEventPolicySettings
            {
                EmitReceivingLifecycleEvents = true,
                EmitStockLedgerEvents = true,
                EmitDockLifecycleEvents = true,
                EmitExceptionLifecycleEvents = true
            }
        };

    public LoadArrTenantSettingsOptionsResponse CreateOptions()
    {
        var defaults = CreateDefaultSettings();
        return new LoadArrTenantSettingsOptionsResponse(
            Sections:
            [
                Section(LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel, "Warehouse operating model", "Controls enabled warehouse execution lanes and default task behavior.", defaults.WarehouseOperatingModel,
                [
                    Bool("enableReceiving", "Enable receiving"),
                    Bool("enablePutaway", "Enable putaway"),
                    Bool("enableInventoryMovements", "Enable inventory movements"),
                    Bool("enableDockScheduling", "Enable dock scheduling"),
                    Bool("enableCycleCounting", "Enable cycle counting"),
                    Bool("enableStockAdjustments", "Enable stock adjustments"),
                    Bool("enableWarehouseTaskQueues", "Enable warehouse task queues"),
                    Bool("enableMobileScannerWorkflows", "Enable mobile scanner workflows"),
                    Bool("requireScanConfirmationForWarehouseTasks", "Require scan confirmation for warehouse tasks"),
                    Bool("allowDesktopTaskCompletion", "Allow desktop task completion"),
                    Text("defaultWarehouseTimeZone", "Default warehouse time zone"),
                    Enum("defaultTaskPriorityMode", "Default task priority mode", "taskPriorityMode"),
                    Enum("defaultShiftCalendarSource", "Default shift calendar source", "shiftCalendarSource")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.Receiving, "Receiving", "Controls purchase order, ASN, transfer, blind, variance, and handoff behavior.", defaults.Receiving,
                [
                    Bool("allowPurchaseOrderReceiving", "Allow purchase order receiving"),
                    Bool("allowAsnReceiving", "Allow ASN receiving"),
                    Bool("allowTransferOrderReceiving", "Allow transfer order receiving"),
                    Bool("allowBlindReceiving", "Allow blind receiving", risky: true),
                    Bool("allowPartialReceiving", "Allow partial receiving"),
                    Bool("allowOverReceipt", "Allow over-receipt"),
                    Number("overReceiptTolerancePercent", "Over-receipt tolerance percent", 0, 100),
                    Bool("autoCloseUnderReceipt", "Auto-close under-receipt"),
                    Bool("requireReceiverNotesOnVariance", "Require receiver notes on variance"),
                    Bool("requirePhotoForDamagedReceipt", "Require photo for damaged receipt"),
                    Bool("requireAttachmentForExceptionReceipt", "Require attachment for exception receipt"),
                    Bool("requireVendorPackingSlip", "Require vendor packing slip"),
                    Bool("requireBolOrPod", "Require BOL or POD"),
                    Bool("requireSupervisorApprovalForVariance", "Require supervisor approval for variance"),
                    Bool("autoCreateExceptionOnVariance", "Auto-create exception on variance"),
                    Bool("autoRouteReceivingExceptionToAssurArr", "Route receiving exception to AssurArr"),
                    Bool("autoNotifySupplyArrOnReceiptConfirmation", "Notify SupplyArr on receipt confirmation"),
                    Bool("autoNotifyMaintainArrForMaintenanceRequestedParts", "Notify MaintainArr for requested parts"),
                    Bool("autoNotifyRoutArrOnCheckInCheckOut", "Notify RoutArr on check-in/check-out")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.DockAppointments, "Dock and appointments", "Controls inbound appointment, grace period, dock reassignment, and RoutArr update behavior.", defaults.DockAppointments,
                [
                    Bool("requireAppointmentForInboundReceiving", "Require appointment for inbound receiving"),
                    Bool("allowWalkInReceiving", "Allow walk-in receiving"),
                    Number("defaultAppointmentMinutes", "Default appointment minutes", 5, 1440),
                    Number("earlyArrivalGraceMinutes", "Early arrival grace minutes", 0),
                    Number("lateArrivalGraceMinutes", "Late arrival grace minutes", 0),
                    Number("noShowThresholdMinutes", "No-show threshold minutes", 0),
                    Enum("lateArrivalBehavior", "Late arrival behavior", "lateArrivalBehavior"),
                    Bool("autoStartDetentionClock", "Auto-start detention clock"),
                    Bool("autoEndDetentionClock", "Auto-end detention clock"),
                    Bool("allowDockReassignment", "Allow dock reassignment"),
                    Bool("allowSameDockOverlappingAppointments", "Allow same dock overlapping appointments", risky: true),
                    Bool("requireTrailerNumberAtCheckIn", "Require trailer number at check-in"),
                    Bool("requireDriverNameAtCheckIn", "Require driver name at check-in"),
                    Bool("requireCarrierAtCheckIn", "Require carrier at check-in"),
                    Bool("requireSealNumberAtCheckIn", "Require seal number at check-in"),
                    Bool("requireYardLocationSelectionAtCheckIn", "Require yard location selection at check-in"),
                    Bool("acceptRoutArrEtaUpdates", "Accept RoutArr ETA updates"),
                    Bool("acceptRoutArrAppointmentUpdates", "Accept RoutArr appointment updates"),
                    Bool("autoCreateAppointmentFromRoutArrInboundTrip", "Auto-create appointment from RoutArr inbound trip"),
                    Bool("autoCreateAppointmentFromSupplyArrExpectation", "Auto-create appointment from SupplyArr expectation")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.Putaway, "Putaway", "Controls directed/manual putaway, storage rules, overrides, and priority behavior.", defaults.Putaway,
                [
                    Bool("enableDirectedPutaway", "Enable directed putaway"),
                    Bool("enableManualPutaway", "Enable manual putaway"),
                    Bool("autoCreatePutawayTaskAfterReceipt", "Auto-create putaway task after receipt"),
                    Bool("requireSourceLocationScan", "Require source location scan"),
                    Bool("requireDestinationLocationScan", "Require destination location scan"),
                    Bool("allowPutawayOverride", "Allow putaway override", risky: true),
                    Bool("requireReasonForPutawayOverride", "Require reason for putaway override"),
                    Enum("defaultPutawayStrategy", "Default putaway strategy", "putawayStrategy"),
                    Bool("respectBinCapacity", "Respect bin capacity"),
                    Bool("respectItemStorageRules", "Respect item storage rules"),
                    Bool("respectComplianceStorageCompatibilityRules", "Respect Compliance Core storage compatibility rules"),
                    Bool("allowMixedSkuBins", "Allow mixed SKU bins", risky: true),
                    Bool("allowMixedLotBins", "Allow mixed lot bins", risky: true),
                    Bool("allowMixedStatusBins", "Allow mixed status bins", risky: true),
                    Bool("allowPartialPutaway", "Allow partial putaway"),
                    Bool("autoPrioritizeMaintenanceCriticalParts", "Prioritize maintenance-critical parts"),
                    Bool("autoPrioritizeCrossDockMaterial", "Prioritize cross-dock material"),
                    Bool("routeInspectionRequiredMaterialToHold", "Route inspection-required material to hold")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.InventoryControl, "Inventory control", "Controls stock ledger, adjustments, count variance, and receipt statuses.", defaults.InventoryControl,
                [
                    Bool("enableStockLedger", "Enable stock ledger"),
                    Bool("allowNegativeInventory", "Allow negative inventory", risky: true),
                    Bool("allowInventoryAdjustment", "Allow inventory adjustment"),
                    Number("adjustmentApprovalThresholdValue", "Adjustment approval threshold value", 0),
                    Bool("requireReasonCodeForAdjustment", "Require reason code for adjustment"),
                    Bool("requireAttachmentForAdjustment", "Require attachment for adjustment"),
                    Bool("requireCycleCountApprovalOnVariance", "Require cycle count approval on variance"),
                    Number("cycleCountVarianceToleranceQuantity", "Cycle count variance tolerance quantity", 0),
                    Number("cycleCountVarianceTolerancePercent", "Cycle count variance tolerance percent", 0),
                    Bool("enableAbcCycleCountFrequency", "Enable ABC cycle count frequency"),
                    Number("defaultCountCadenceDays", "Default count cadence days", 0),
                    Bool("freezeLocationDuringCount", "Freeze location during count"),
                    Bool("allowMovementDuringCount", "Allow movement during count"),
                    Bool("requireRecountOnVariance", "Require recount on variance"),
                    Bool("requireSupervisorApprovalForWriteOff", "Require supervisor approval for write-off"),
                    Bool("requireComplianceCheckBeforeReleasingHeldMaterial", "Require compliance check before releasing held material"),
                    Enum("defaultInventoryStatusAfterReceipt", "Default inventory status after receipt", "inventoryStatus"),
                    Enum("defaultInventoryStatusForDamagedReceipt", "Default inventory status for damaged receipt", "inventoryStatus"),
                    Enum("defaultInventoryStatusForVarianceReceipt", "Default inventory status for variance receipt", "inventoryStatus")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.Traceability, "Traceability", "Controls LPN, lot, serial, expiration, seal, and chain-of-custody behavior.", defaults.Traceability,
                [
                    Bool("enableLpn", "Enable LPN"),
                    Bool("autoGenerateLpn", "Auto-generate LPN"),
                    Text("lpnFormat", "LPN format"),
                    Bool("requireLpnScan", "Require LPN scan"),
                    Bool("requirePalletId", "Require pallet ID"),
                    Bool("requireCartonId", "Require carton ID"),
                    Bool("requireLotWhenItemRequiresLotTracking", "Require lot when item requires lot tracking"),
                    Bool("requireSerialWhenItemRequiresSerialTracking", "Require serial when item requires serial tracking"),
                    Bool("requireExpirationDateWhenItemRequiresExpirationTracking", "Require expiration date when item requires expiration tracking"),
                    Bool("requireManufactureDateWhenItemRequiresManufactureDateTracking", "Require manufacture date when item requires manufacture date tracking"),
                    Bool("allowMixedLotsInLpn", "Allow mixed lots in LPN", risky: true),
                    Bool("allowMixedSkusInLpn", "Allow mixed SKUs in LPN", risky: true),
                    Bool("requireSealNumberForTrailerOrContainerReceiving", "Require seal number for trailer or container receiving"),
                    Bool("requireChainOfCustodyForControlledMaterials", "Require chain of custody for controlled materials")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.Movement, "Movement", "Controls ad hoc movement, hold/quarantine movement, replenishment, and cross-product task creation.", defaults.Movement,
                [
                    Bool("allowAdHocMovement", "Allow ad hoc movement"),
                    Bool("requireMovementReasonCode", "Require movement reason code"),
                    Bool("requireSourceScanForMovement", "Require source scan for movement"),
                    Bool("requireDestinationScanForMovement", "Require destination scan for movement"),
                    Bool("allowMovementFromHoldStatus", "Allow movement from hold status", risky: true),
                    Bool("requireApprovalToMoveHeldStock", "Require approval to move held stock"),
                    Bool("allowMovementIntoQuarantine", "Allow movement into quarantine"),
                    Bool("allowMovementOutOfQuarantine", "Allow movement out of quarantine", risky: true),
                    Bool("requireAssurArrDispositionBeforeReleaseFromQuarantine", "Require AssurArr disposition before release from quarantine"),
                    Bool("enableReplenishmentTasks", "Enable replenishment tasks"),
                    Bool("enableInternalTransferTasks", "Enable internal transfer tasks"),
                    Bool("enableMaintenanceIssueHandoffTasks", "Enable maintenance issue handoff tasks"),
                    Bool("autoCreateMovementTaskFromMaintainArrRequest", "Auto-create movement task from MaintainArr request"),
                    Bool("autoCreateMovementTaskFromOrdArrFulfillmentNeed", "Auto-create movement task from OrdArr fulfillment need"),
                    Bool("autoCreateMovementTaskFromReceivingCompletion", "Auto-create movement task from receiving completion")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.Exceptions, "Exceptions", "Controls receiving/inventory/damage exception handling and AssurArr handoffs.", defaults.Exceptions,
                [
                    Bool("enableReceivingExceptions", "Enable receiving exceptions"),
                    Bool("enableInventoryExceptions", "Enable inventory exceptions"),
                    Bool("enableDamageExceptions", "Enable damage exceptions"),
                    Bool("enableShortageExceptions", "Enable shortage exceptions"),
                    Bool("enableOverageExceptions", "Enable overage exceptions"),
                    Bool("enableWrongItemExceptions", "Enable wrong-item exceptions"),
                    Bool("enableExpiredMaterialExceptions", "Enable expired-material exceptions"),
                    Bool("enableFailedInspectionExceptions", "Enable failed-inspection exceptions"),
                    Bool("autoCreateAssurArrCase", "Auto-create AssurArr case"),
                    Bool("requireAssurArrDispositionBeforeRelease", "Require AssurArr disposition before release"),
                    Bool("allowLocalWarehouseDisposition", "Allow local warehouse disposition", risky: true),
                    Bool("requirePhotoEvidence", "Require photo evidence"),
                    Bool("requireSupervisorReview", "Require supervisor review"),
                    Bool("requireVendorNotificationThroughSupplyArr", "Require vendor notification through SupplyArr"),
                    Bool("autoHoldAffectedInventory", "Auto-hold affected inventory"),
                    Bool("autoBlockPutawayOnException", "Auto-block putaway on exception")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.Compliance, "Compliance", "Controls Compliance Core checks, training gates, ruling behavior, and decision snapshots.", defaults.Compliance,
                [
                    Bool("enableComplianceCoreChecks", "Enable Compliance Core checks", risky: true),
                    Bool("checkComplianceBeforeReceiving", "Check compliance before receiving"),
                    Bool("checkComplianceBeforePutaway", "Check compliance before putaway"),
                    Bool("checkComplianceBeforeMovement", "Check compliance before movement"),
                    Bool("checkHazardousStorageCompatibility", "Check hazardous storage compatibility"),
                    Bool("checkTemperatureStorageConstraints", "Check temperature storage constraints"),
                    Bool("checkPpeRequirementsForHandlingTask", "Check PPE requirements for handling task"),
                    Bool("checkRestrictedMaterialAccess", "Check restricted material access"),
                    Bool("checkTrainingRequirementBeforeTaskAssignment", "Check training requirement before task assignment"),
                    Bool("blockNonQualifiedWorkerAssignment", "Block non-qualified worker assignment"),
                    Enum("advisoryRulingBehavior", "Advisory ruling behavior", "advisoryRulingBehavior"),
                    Enum("failedRulingBehavior", "Failed ruling behavior", "failedRulingBehavior"),
                    Bool("requireComplianceOverrideReason", "Require compliance override reason"),
                    Bool("requireSupervisorApprovalForComplianceOverride", "Require supervisor approval for compliance override"),
                    Bool("storeComplianceDecisionSnapshots", "Store compliance decision snapshots")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.TaskAssignment, "Task assignment", "Controls manual/auto assignment, StaffArr and TrainArr checks, aging, and reassignment.", defaults.TaskAssignment,
                [
                    Bool("manualTaskAssignmentEnabled", "Manual task assignment enabled"),
                    Bool("autoAssignmentEnabled", "Auto-assignment enabled"),
                    Bool("allowWorkerSelfClaim", "Allow worker self-claim"),
                    Bool("requireStaffArrActivePersonStatus", "Require StaffArr active person status"),
                    Bool("requireStaffArrPermissionCheck", "Require StaffArr permission check"),
                    Bool("requireTrainArrQualificationCheck", "Require TrainArr qualification check"),
                    Bool("respectShiftAvailability", "Respect shift availability"),
                    Bool("respectZoneAssignment", "Respect zone assignment"),
                    Bool("respectEquipmentQualification", "Respect equipment qualification"),
                    Number("taskEscalationThresholdMinutes", "Task escalation threshold minutes", 0),
                    Number("taskAgingWarningThresholdMinutes", "Task aging warning threshold minutes", 0),
                    Bool("allowTaskReassignment", "Allow task reassignment"),
                    Bool("requireReasonForTaskReassignment", "Require reason for task reassignment")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.MobileScanner, "Mobile and scanner", "Controls barcode scans, manual entry, camera/external scanners, offline-readiness policy, and verification.", defaults.MobileScanner,
                [
                    Bool("requireBarcodeScanForReceipt", "Require barcode scan for receipt"),
                    Bool("requireBarcodeScanForPutaway", "Require barcode scan for putaway"),
                    Bool("requireBarcodeScanForMovement", "Require barcode scan for movement"),
                    Bool("allowManualBarcodeEntry", "Allow manual barcode entry"),
                    Bool("requireReasonForManualBarcodeEntry", "Require reason for manual barcode entry"),
                    Bool("enableCameraScanning", "Enable camera scanning"),
                    Bool("enableExternalScannerSupport", "Enable external scanner support"),
                    Bool("allowOfflineTaskExecution", "Prepare offline task execution policy", risky: true),
                    Enum("offlineSyncConflictPolicy", "Offline-readiness conflict policy", "offlineSyncConflictPolicy"),
                    Bool("requirePhotoCaptureForDamage", "Require photo capture for damage"),
                    Bool("requireSignatureCapture", "Require signature capture"),
                    Bool("requireLocationConfirmation", "Require location confirmation"),
                    Bool("requireSecondPersonVerificationForHighRiskMoves", "Require second-person verification for high-risk moves")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.LabelingAndDocuments, "Labels and documents", "Controls warehouse labels, document attachments, RecordArr handoff, and generated reports.", defaults.LabelingAndDocuments,
                [
                    Bool("generateReceivingLabels", "Generate receiving labels"),
                    Bool("generatePalletLabels", "Generate pallet labels"),
                    Bool("generateBinLabels", "Generate bin labels"),
                    Bool("generateQuarantineLabels", "Generate quarantine labels"),
                    Bool("generateLpnLabels", "Generate LPN labels"),
                    Enum("defaultLabelSize", "Default label size", "labelSize"),
                    Enum("defaultPrinterRoutingMode", "Default printer routing mode", "printerRoutingMode"),
                    Bool("requireLabelPrintBeforePutaway", "Require label print before putaway"),
                    Bool("attachPackingSlipToReceipt", "Attach packing slip to receipt"),
                    Bool("attachBolOrPodToReceipt", "Attach BOL or POD to receipt"),
                    Bool("sendReceivedDocumentPacketToRecordArr", "Send received document packet to RecordArr"),
                    Bool("generateReceivingSummaryPdf", "Generate receiving summary PDF"),
                    Bool("generateDiscrepancyReport", "Generate discrepancy report"),
                    Bool("generatePutawayReport", "Generate putaway report")
                ]),
                Section(LoadArrTenantSettingsSectionKeys.NotificationsAndEvents, "Notifications and events", "Controls owning-product notifications and LoadArr event emission.", defaults.NotificationsAndEvents,
                [
                    Bool("notifySupplyArrOnReceipt", "Notify SupplyArr on receipt"),
                    Bool("notifySupplyArrOnVariance", "Notify SupplyArr on variance"),
                    Bool("notifyMaintainArrWhenPartsArrive", "Notify MaintainArr when parts arrive"),
                    Bool("notifyRoutArrOnCheckInCheckOut", "Notify RoutArr on check-in/check-out"),
                    Bool("notifyAssurArrOnDamageOrQualityException", "Notify AssurArr on damage or quality exception"),
                    Bool("notifyStaffArrOnWorkerIncident", "Notify StaffArr on worker incident"),
                    Bool("notifyComplianceCoreOnComplianceRelevantWarehouseEvent", "Notify Compliance Core on compliance-relevant warehouse event"),
                    Bool("notifyExternalContactsThroughSupplyArr", "Notify external contacts through SupplyArr"),
                    Bool("emitStockLedgerEvents", "Emit stock ledger events"),
                    Bool("emitReceivingLifecycleEvents", "Emit receiving lifecycle events"),
                    Bool("emitDockLifecycleEvents", "Emit dock lifecycle events"),
                    Bool("emitPutawayLifecycleEvents", "Emit putaway lifecycle events"),
                    Bool("emitExceptionLifecycleEvents", "Emit exception lifecycle events"),
                    Bool("emitTaskLifecycleEvents", "Emit task lifecycle events")
                ])
            ],
            EnumOptions: CreateEnumOptions(),
            EventNames:
            [
                "loadarr.receipt.started",
                "loadarr.receipt.completed",
                "loadarr.receipt.variance_detected",
                "loadarr.receipt.exception_created",
                "loadarr.dock.appointment_created",
                "loadarr.dock.check_in_recorded",
                "loadarr.dock.check_out_recorded",
                "loadarr.putaway.task_created",
                "loadarr.putaway.completed",
                "loadarr.inventory.movement_completed",
                "loadarr.inventory.adjustment_requested",
                "loadarr.inventory.adjustment_approved",
                "loadarr.inventory.hold_applied",
                "loadarr.inventory.hold_released",
                "loadarr.task.created",
                "loadarr.task.assigned",
                "loadarr.task.completed",
                "loadarr.task.exceptioned"
            ]);
    }

    private static LoadArrTenantSettingsSectionOption Section(
        string key,
        string label,
        string description,
        object defaultValue,
        IReadOnlyList<LoadArrTenantSettingsFieldOption> fields) =>
        new(key, label, description, defaultValue, fields);

    private static LoadArrTenantSettingsFieldOption Bool(string key, string label, bool risky = false) =>
        new(key, label, "boolean", Risky: risky);

    private static LoadArrTenantSettingsFieldOption Number(
        string key,
        string label,
        decimal? min = null,
        decimal? max = null,
        bool risky = false) =>
        new(key, label, "number", min, max, Risky: risky);

    private static LoadArrTenantSettingsFieldOption Text(string key, string label, bool risky = false) =>
        new(key, label, "text", Risky: risky);

    private static LoadArrTenantSettingsFieldOption Enum(string key, string label, string enumKey, bool risky = false) =>
        new(key, label, "enum", EnumKey: enumKey, Risky: risky);

    private static IReadOnlyDictionary<string, IReadOnlyList<LoadArrTenantSettingsEnumOption>> CreateEnumOptions() =>
        new Dictionary<string, IReadOnlyList<LoadArrTenantSettingsEnumOption>>(StringComparer.OrdinalIgnoreCase)
        {
            ["taskPriorityMode"] =
            [
                Option(LoadArrTenantSettingsOptionValues.TaskPriorityManual, "Manual", "Users set task priority."),
                Option(LoadArrTenantSettingsOptionValues.TaskPriorityFifo, "FIFO", "Oldest ready task is prioritized first."),
                Option(LoadArrTenantSettingsOptionValues.TaskPriorityDueTime, "Due time", "Earliest due task is prioritized first."),
                Option(LoadArrTenantSettingsOptionValues.TaskPriorityMaintenancePriority, "Maintenance priority", "MaintainArr criticality influences task priority."),
                Option(LoadArrTenantSettingsOptionValues.TaskPriorityDockPriority, "Dock priority", "Dock schedule pressure influences task priority."),
                Option(LoadArrTenantSettingsOptionValues.TaskPriorityExceptionPriority, "Exception priority", "Exception risk influences task priority.")
            ],
            ["shiftCalendarSource"] =
            [
                Option(LoadArrTenantSettingsOptionValues.ShiftCalendarStaffArr, "StaffArr", "Use StaffArr shift/availability context."),
                Option(LoadArrTenantSettingsOptionValues.ShiftCalendarLoadArrManual, "LoadArr manual", "Use a LoadArr-local execution calendar."),
                Option(LoadArrTenantSettingsOptionValues.ShiftCalendarNone, "None", "Do not apply a default shift calendar.", risky: true)
            ],
            ["lateArrivalBehavior"] =
            [
                Option(LoadArrTenantSettingsOptionValues.LateArrivalAllow, "Allow", "Allow late arrivals."),
                Option(LoadArrTenantSettingsOptionValues.LateArrivalWarn, "Warn", "Show a warning on late arrival."),
                Option(LoadArrTenantSettingsOptionValues.LateArrivalRequireSupervisor, "Require supervisor", "Require supervisor review."),
                Option(LoadArrTenantSettingsOptionValues.LateArrivalRescheduleRequired, "Reschedule required", "Require appointment rescheduling.")
            ],
            ["putawayStrategy"] =
            [
                Option(LoadArrTenantSettingsOptionValues.PutawayManual, "Manual", "User selects destination."),
                Option(LoadArrTenantSettingsOptionValues.PutawayNearestAvailable, "Nearest available", "Suggest nearest available location."),
                Option(LoadArrTenantSettingsOptionValues.PutawayFixedBinFirst, "Fixed bin first", "Prefer fixed bin assignments."),
                Option(LoadArrTenantSettingsOptionValues.PutawayZoneFirst, "Zone first", "Prefer zone rules."),
                Option(LoadArrTenantSettingsOptionValues.PutawayFifo, "FIFO", "Prefer first-in-first-out."),
                Option(LoadArrTenantSettingsOptionValues.PutawayFefo, "FEFO", "Prefer first-expiring-first-out."),
                Option(LoadArrTenantSettingsOptionValues.PutawayMaintenancePriority, "Maintenance priority", "Prioritize maintenance-critical material."),
                Option(LoadArrTenantSettingsOptionValues.PutawayCrossDockFirst, "Cross-dock first", "Prefer cross-dock handling.")
            ],
            ["inventoryStatus"] =
            [
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusAvailable, "Available", "Usable available stock."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusReceivedPendingPutaway, "Received pending putaway", "Received but not yet put away."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusStaged, "Staged", "Staged for downstream work."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusInspectionHold, "Inspection hold", "Held pending inspection."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusQuarantine, "Quarantine", "Quarantined stock."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusDamaged, "Damaged", "Damaged stock."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusAllocated, "Allocated", "Allocated stock."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusReserved, "Reserved", "Reserved stock."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusCycleCountHold, "Cycle count hold", "Held for cycle count."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusMaintenanceHandoff, "Maintenance handoff", "Stock moving to maintenance work."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusShipped, "Shipped", "Shipped stock."),
                Option(LoadArrTenantSettingsOptionValues.InventoryStatusScrapped, "Scrapped", "Scrapped stock.")
            ],
            ["advisoryRulingBehavior"] =
            [
                Option(LoadArrTenantSettingsOptionValues.AdvisoryIgnore, "Ignore", "Ignore advisory rulings.", risky: true),
                Option(LoadArrTenantSettingsOptionValues.AdvisoryWarn, "Warn", "Show advisory warnings."),
                Option(LoadArrTenantSettingsOptionValues.AdvisoryRequireAcknowledgement, "Require acknowledgement", "Require acknowledgement before proceeding.")
            ],
            ["failedRulingBehavior"] =
            [
                Option(LoadArrTenantSettingsOptionValues.FailedWarn, "Warn", "Warn on failed rulings.", risky: true),
                Option(LoadArrTenantSettingsOptionValues.FailedBlock, "Block", "Block failed rulings."),
                Option(LoadArrTenantSettingsOptionValues.FailedRequireSupervisorOverride, "Require supervisor override", "Require supervisor override on failed rulings.")
            ],
            ["offlineSyncConflictPolicy"] =
            [
                Option(LoadArrTenantSettingsOptionValues.OfflineBlockSync, "Block future sync", "Block future offline sync until conflict is resolved."),
                Option(LoadArrTenantSettingsOptionValues.OfflineSupervisorReview, "Supervisor review", "Route future offline conflicts for supervisor review."),
                Option(LoadArrTenantSettingsOptionValues.OfflineLastWriteWinsNonInventory, "Last write wins for non-inventory fields", "Only non-inventory fields may use last-write-wins.", risky: true)
            ],
            ["labelSize"] =
            [
                Option(LoadArrTenantSettingsOptionValues.Label4x6, "4 x 6 label", "Standard warehouse label."),
                Option(LoadArrTenantSettingsOptionValues.Label2x1, "2 x 1 label", "Small item label."),
                Option(LoadArrTenantSettingsOptionValues.LabelLetter, "Letter", "Letter-sized output."),
                Option(LoadArrTenantSettingsOptionValues.LabelCustom, "Custom", "Custom label size.")
            ],
            ["printerRoutingMode"] =
            [
                Option(LoadArrTenantSettingsOptionValues.PrinterUserSelected, "User selected", "User picks a printer."),
                Option(LoadArrTenantSettingsOptionValues.PrinterLocationDefault, "Location default", "Route by StaffArr location snapshot."),
                Option(LoadArrTenantSettingsOptionValues.PrinterTaskTypeDefault, "Task type default", "Route by task type.")
            ]
        };

    private static LoadArrTenantSettingsEnumOption Option(
        string value,
        string label,
        string description,
        bool risky = false) =>
        new(value, label, description, risky);
}
