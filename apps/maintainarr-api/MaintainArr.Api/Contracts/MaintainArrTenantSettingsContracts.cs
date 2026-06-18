namespace MaintainArr.Api.Contracts;

public sealed record MaintainArrTenantSettingsDto(
    int SchemaVersion,
    MaintainArrOperatingSettingsDto Operating,
    MaintainArrAssetSettingsDto Assets,
    MaintainArrWorkOrderSettingsDto WorkOrders,
    MaintainArrDefectSettingsDto Defects,
    MaintainArrOutOfServiceSettingsDto OutOfService,
    MaintainArrPreventiveMaintenanceSettingsDto PreventiveMaintenance,
    MaintainArrInspectionSettingsDto Inspections,
    MaintainArrLaborSettingsDto Labor,
    MaintainArrPartsSettingsDto Parts,
    MaintainArrSchedulingSettingsDto Scheduling,
    MaintainArrEvidenceSettingsDto Evidence,
    MaintainArrNotificationDefaultsDto Notifications,
    MaintainArrMobileSettingsDto Mobile,
    MaintainArrComplianceSettingsDto Compliance,
    MaintainArrIntegrationSettingsDto Integrations,
    MaintainArrUiSettingsDto Ui);

public sealed record MaintainArrOperatingSettingsDto(
    string MaintenanceOperatingMode,
    string MaintenanceStrictness);

public sealed record MaintainArrAssetSettingsDto(
    string AssetNumberingMode,
    string? AssetNumberPrefix,
    bool RequireAssetClassOnCreate,
    bool RequireSiteOnAssetCreate,
    bool RequireVinOrSerial,
    string DefaultAssetStatus);

public sealed record MaintainArrWorkOrderSettingsDto(
    string WorkOrderNumberingMode,
    string? WorkOrderNumberPrefix,
    string DefaultPriority,
    bool AllowUnassignedWorkOrders,
    bool RequireAssetOnWorkOrder,
    bool RequireLaborBeforeClose,
    bool RequirePartsBeforeClose,
    bool RequireResolutionNotesBeforeClose,
    bool AllowReopenClosedWorkOrders);

public sealed record MaintainArrDefectSettingsDto(
    bool AllowOperatorDefectReports,
    bool AllowDefectSubmissionWithoutAsset,
    bool RequireSeverityOnDefect,
    bool RequirePhotoForSafetyDefects,
    bool AutoCreateWorkOrderFromDefect,
    bool AutoMarkAssetOOSForCriticalDefect,
    bool EnableAIIntakeQuestions,
    bool AiQuestionsRequiredByDefault,
    bool AllowSubmitNowForSafetyIssue);

public sealed record MaintainArrOutOfServiceSettingsDto(
    bool EnableOutOfServiceStatus,
    bool RequireOOSReason,
    bool RequireSupervisorApprovalForRTS,
    bool RequireInspectionBeforeRTS,
    bool RequireAllCriticalDefectsClosedBeforeRTS,
    bool AllowRTSWithOpenMinorDefects);

public sealed record MaintainArrPreventiveMaintenanceSettingsDto(
    bool PmAutoGenerateWorkOrders,
    int PmGenerateDaysAhead,
    int PmGracePeriodDays,
    bool AllowPMDeferral,
    bool RequireDeferralReason,
    bool RequireApprovalForPMDeferral);

public sealed record MaintainArrInspectionSettingsDto(
    bool InspectionAutoCreateDefects,
    bool InspectionFailureCreatesWorkOrder,
    bool InspectionFailureMarksAssetOOS,
    bool RequireSignatureOnInspection,
    bool RequirePhotoForFailedInspectionItem);

public sealed record MaintainArrLaborSettingsDto(
    bool EnableLaborTracking,
    bool RequireLaborOnWorkOrderClose,
    bool AllowMultipleTechniciansPerWO,
    string LaborTimeEntryMode,
    int RoundLaborMinutesTo);

public sealed record MaintainArrPartsSettingsDto(
    bool AllowPartsRequestsFromWorkOrders,
    bool AllowNonCatalogParts,
    bool RequireReasonForNonCatalogPart,
    string PartsReservationMode);

public sealed record MaintainArrSchedulingSettingsDto(
    bool EnableMaintenanceScheduling,
    int DefaultScheduleDurationMinutes,
    bool AllowDragDropScheduling,
    bool AllowSchedulingWithoutTechnician,
    bool AllowSchedulingWithoutBay,
    bool RespectStaffArrAvailability,
    bool RespectTrainArrQualifications);

public sealed record MaintainArrEvidenceSettingsDto(
    bool EnablePhotoAttachments,
    bool RequirePhotoForCriticalDefect,
    bool RequireTechnicianSignatureOnWO,
    bool RequireSupervisorSignatureOnRTS,
    bool SendCompletedPacketsToRecordArr);

public sealed record MaintainArrNotificationDefaultsDto(
    bool NotifyOnCriticalDefect,
    bool NotifyOnAssetMarkedOOS,
    bool NotifyOnAssetReturnedToService,
    bool NotifyOnPMComingDue,
    bool NotifyOnPMOverdue,
    bool NotifyOnWOAssigned,
    bool NotifyOnWOCompleted,
    int PmDueNotificationDaysAhead);

public sealed record MaintainArrMobileSettingsDto(
    bool EnableMobileMode,
    bool AllowOfflineWorkOrders,
    bool AllowOfflineInspections,
    bool AllowCameraUpload,
    bool AllowVoiceNotes,
    bool RequireSyncBeforeClose);

public sealed record MaintainArrComplianceSettingsDto(
    bool EnableComplianceCoreChecks,
    string ComplianceCheckMode,
    bool CheckComplianceOnInspectionComplete,
    bool CheckComplianceOnReturnToService,
    bool ShowComplianceReasoningToUsers);

public sealed record MaintainArrIntegrationSettingsDto(
    bool EnableStaffArrPeopleLookup,
    bool EnableStaffArrLocationLookup,
    bool EnableTrainArrQualificationChecks,
    bool EnableSupplyArrPartsLookup,
    bool EnableLoadArrInventoryRequests,
    bool EnableRoutArrReadinessEvents,
    bool EnableRecordArrDocumentPackets);

public sealed record MaintainArrUiSettingsDto(
    string DefaultLandingPage,
    bool ShowAssetHealthScore,
    bool ShowComplianceBadges,
    bool ShowDowntimeMetrics,
    bool ShowInternalIds);

public sealed record UpsertMaintainArrTenantSettingsRequest(
    MaintainArrTenantSettingsDto Settings,
    string? ChangeReason);

public sealed record ResetMaintainArrTenantSettingsRequest(string? ChangeReason);

public sealed record MaintainArrTenantSettingsResponse(
    MaintainArrTenantSettingsDto Settings,
    DateTimeOffset CreatedAtUtc,
    string? CreatedByPersonId,
    DateTimeOffset UpdatedAtUtc,
    string? UpdatedByPersonId);

public sealed record MaintainArrTenantSettingsAuditResponse(
    IReadOnlyList<MaintainArrTenantSettingsAuditItem> Items);

public sealed record MaintainArrTenantSettingsAuditItem(
    DateTimeOffset ChangedAtUtc,
    string? ChangedByPersonId,
    string? ChangeReason,
    int SchemaVersion,
    IReadOnlyList<MaintainArrTenantSettingsAuditChange> Changes);

public sealed record MaintainArrTenantSettingsAuditChange(
    string Path,
    string? Before,
    string? After);
