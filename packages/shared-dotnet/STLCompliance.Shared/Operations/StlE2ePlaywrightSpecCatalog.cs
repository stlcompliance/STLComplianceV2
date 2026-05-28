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
        ..OperatorJourneySmokeSpecs,
    ];
}
