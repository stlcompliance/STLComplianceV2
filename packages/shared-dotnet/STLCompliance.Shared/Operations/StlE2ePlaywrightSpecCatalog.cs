namespace STLCompliance.Shared.Operations;

/// <summary>
/// Playwright browser E2E spec filenames under tests/e2e-playwright/tests (W94+).
/// </summary>
public static class StlE2ePlaywrightSpecCatalog
{
    public const string SuiteLoginHandoffSmokeSpec = "suite-login-handoff-smoke.spec.ts";
    public const string ProductHandoffSmokeSpec = "product-handoff-smoke.spec.ts";
    public const string ProductHandoffTenantChromeSpec = "product-handoff-tenant-chrome.spec.ts";
    public const string CompanionFieldInboxTrainarrDeepLinkSpec =
        "companion-field-inbox-trainarr-deep-link.spec.ts";
    public const string ProductTrainarrAssignmentDeepLinkSpec =
        "product-trainarr-assignment-deep-link.spec.ts";
    public const string CompanionFieldInboxMaintainarrDeepLinkSpec =
        "companion-field-inbox-operations-deep-links.spec.ts";
    public const string PlatformAdminAuditExportSmokeSpec =
        "platform-admin-audit-export-smoke.spec.ts";

    public const string MaintainArrSettingsAuditExportSmokeSpec =
        "maintainarr-settings-audit-export-smoke.spec.ts";

    public const string ComplianceCoreM12WorkerSettingsSmokeSpec =
        "compliancecore-m12-worker-settings-smoke.spec.ts";

    public const string ComplianceCoreAuditDeliveryOrchestrationSmokeSpec =
        "compliancecore-audit-delivery-orchestration-smoke.spec.ts";

    public const string TrainArrAssignmentMaterialDemandSmokeSpec =
        "trainarr-assignment-material-demand-smoke.spec.ts";

    public const string RoutArrDispatchCommandCenterSmokeSpec =
        "routarr-dispatch-command-center-smoke.spec.ts";

    public const string RoutArrDispatchExceptionQueueSmokeSpec =
        "routarr-dispatch-exception-queue-smoke.spec.ts";

    public const string RoutArrDispatchActiveTripsSmokeSpec =
        "routarr-dispatch-active-trips-smoke.spec.ts";

    public const string RoutArrDispatchUnassignedWorkQueueSmokeSpec =
        "routarr-dispatch-unassigned-work-queue-smoke.spec.ts";

    public const string RoutArrDriverPortalSmokeSpec = "routarr-driver-portal-smoke.spec.ts";

    public const string RoutArrDispatchProofDvirReadSmokeSpec =
        "routarr-dispatch-proof-dvir-read-smoke.spec.ts";

    public const string SupplyArrSettingsIntegrationEventsSmokeSpec =
        "supplyarr-settings-integration-events-smoke.spec.ts";

    public const string SupplyArrReportsWorkspaceSmokeSpec =
        "supplyarr-reports-workspace-smoke.spec.ts";

    public const string StaffArrAdminAuditExportSmokeSpec =
        "staffarr-admin-audit-export-smoke.spec.ts";

    public const string TrainArrSettingsAuditExportSmokeSpec =
        "trainarr-settings-audit-export-smoke.spec.ts";

    public const string RoutArrReportsAuditExportSmokeSpec =
        "routarr-reports-audit-export-smoke.spec.ts";
    public const string ComplianceCoreOperatorRuleEvaluateSmokeSpec =
        "compliancecore-operator-rule-evaluate-smoke.spec.ts";
    public const string SuiteMultiProductHandoffJourneySpec =
        "suite-multi-product-handoff-journey.spec.ts";
    public const string CompanionOfflineQueueNotificationSpec =
        "companion-offline-queue-notification.spec.ts";

    public const string CompanionFieldTaskEvidenceSpec = "companion-field-task-evidence.spec.ts";

    public const string CompanionFieldSubmissionStateSpec = "companion-field-submission-state.spec.ts";

    public const string CompanionProductSwitcherSpec = "companion-product-switcher.spec.ts";

    public const string CompanionFieldScanSpec = "companion-field-scan.spec.ts";

    public static readonly IReadOnlyList<string> CompanionOperationalSpecs =
    [
        CompanionOfflineQueueNotificationSpec,
        CompanionFieldTaskEvidenceSpec,
        CompanionFieldSubmissionStateSpec,
        CompanionProductSwitcherSpec,
        CompanionFieldScanSpec,
    ];

    public static readonly IReadOnlyList<string> DeepLinkSmokeSpecs =
    [
        CompanionFieldInboxTrainarrDeepLinkSpec,
        ProductTrainarrAssignmentDeepLinkSpec,
        CompanionFieldInboxMaintainarrDeepLinkSpec,
    ];

    public static readonly IReadOnlyList<string> PlatformAdminSmokeSpecs = [PlatformAdminAuditExportSmokeSpec];

    public static readonly IReadOnlyList<string> ProductAdminSmokeSpecs =
    [
        MaintainArrSettingsAuditExportSmokeSpec,
        ComplianceCoreM12WorkerSettingsSmokeSpec,
        ComplianceCoreAuditDeliveryOrchestrationSmokeSpec,
        TrainArrAssignmentMaterialDemandSmokeSpec,
        RoutArrDispatchCommandCenterSmokeSpec,
        RoutArrDispatchExceptionQueueSmokeSpec,
        RoutArrDispatchActiveTripsSmokeSpec,
        RoutArrDispatchUnassignedWorkQueueSmokeSpec,
        RoutArrDriverPortalSmokeSpec,
        RoutArrDispatchProofDvirReadSmokeSpec,
        SupplyArrSettingsIntegrationEventsSmokeSpec,
        SupplyArrReportsWorkspaceSmokeSpec,
        StaffArrAdminAuditExportSmokeSpec,
        TrainArrSettingsAuditExportSmokeSpec,
        RoutArrReportsAuditExportSmokeSpec,
    ];

    public static readonly IReadOnlyList<string> OperatorJourneySmokeSpecs =
    [
        ComplianceCoreOperatorRuleEvaluateSmokeSpec,
        SuiteMultiProductHandoffJourneySpec,
    ];

    public static readonly IReadOnlyList<string> All =
    [
        SuiteLoginHandoffSmokeSpec,
        ProductHandoffSmokeSpec,
        ProductHandoffTenantChromeSpec,
        ..CompanionOperationalSpecs,
        ..DeepLinkSmokeSpecs,
        ..PlatformAdminSmokeSpecs,
        ..ProductAdminSmokeSpecs,
        ..OperatorJourneySmokeSpecs,
    ];
}
