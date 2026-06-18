using STLCompliance.Shared.Integration;

namespace STLCompliance.Shared.Operations;

public enum StlMasterWorkflowSectionKind
{
    UniversalMechanic,
    ProductWorkflow,
    CrossSuiteWorkflow,
}

public enum StlWorkflowCompletionMode
{
    SharedMechanicAppliedByOwningProduct,
    ProductOwnedWorkflow,
    CrossProductWorkflowPack,
    PublicSiteIntakeToOwningProduct,
}

public sealed record StlMasterWorkflowSectionDescriptor(
    string SectionKey,
    string Title,
    string CodePrefix,
    int FirstOrdinal,
    int LastOrdinal,
    StlMasterWorkflowSectionKind Kind,
    StlWorkflowCompletionMode CompletionMode,
    string? PrimaryOwnerProductKey,
    IReadOnlyList<string> CompletionOwnerProductKeys,
    IReadOnlyList<string> RequiredCompletionCapabilities)
{
    public int Count => LastOrdinal - FirstOrdinal + 1;

    public IReadOnlyList<StlMasterWorkflowDescriptor> Expand() =>
        Enumerable
            .Range(FirstOrdinal, Count)
            .Select(ordinal => new StlMasterWorkflowDescriptor(
                WorkflowId: $"{CodePrefix}-{ordinal:00}",
                SectionKey,
                SectionTitle: Title,
                Ordinal: ordinal,
                Kind,
                CompletionMode,
                PrimaryOwnerProductKey,
                CompletionOwnerProductKeys,
                RequiredCompletionCapabilities))
            .ToArray();
}

public sealed record StlMasterWorkflowDescriptor(
    string WorkflowId,
    string SectionKey,
    string SectionTitle,
    int Ordinal,
    StlMasterWorkflowSectionKind Kind,
    StlWorkflowCompletionMode CompletionMode,
    string? PrimaryOwnerProductKey,
    IReadOnlyList<string> CompletionOwnerProductKeys,
    IReadOnlyList<string> RequiredCompletionCapabilities)
{
    public bool HasCompletionContract =>
        CompletionOwnerProductKeys.Count > 0 &&
        RequiredCompletionCapabilities.Count > 0 &&
        CompletionOwnerProductKeys.All(StlProductKeys.IsCanonical);
}

public static class StlMasterWorkflowCatalog
{
    public const string CatalogName = "STL Compliance Master Workflow Catalog";
    public const int ExpectedWorkflowFamilyCount = 960;

    private static readonly IReadOnlyList<string> UniversalWorkflowOwners =
    [
        StlProductKeys.NexArr,
        StlProductKeys.StaffArr,
        StlProductKeys.TrainArr,
        StlProductKeys.MaintainArr,
        StlProductKeys.RoutArr,
        StlProductKeys.SupplyArr,
        StlProductKeys.LoadArr,
        StlProductKeys.AssurArr,
        StlProductKeys.RecordArr,
        StlProductKeys.OrdArr,
        StlProductKeys.CustomArr,
        StlProductKeys.LedgArr,
        StlProductKeys.ReportArr,
        StlProductKeys.ComplianceCore,
        StlProductKeys.FieldCompanion,
        StlProductKeys.StlComplianceSite,
    ];

    private static readonly IReadOnlyList<string> UniversalMechanicCapabilities =
    [
        "owning_product_executes",
        "draft_submit_validate",
        "assignment_approval_blocker",
        "audit_history",
        "recordarr_evidence_link",
        "completion_or_closeout_state",
    ];

    private static readonly IReadOnlyList<string> ProductWorkflowCapabilities =
    [
        "owning_product_record",
        "explicit_lifecycle_transition",
        "assignment_or_queue_target",
        "review_or_approval_gate",
        "blocker_clearance_path",
        "escalation_or_due_handling",
        "audit_activity_history",
        "recordarr_evidence_link",
        "completion_or_closeout_state",
    ];

    private static readonly IReadOnlyList<string> CrossSuiteWorkflowCapabilities =
    [
        "source_of_truth_table",
        "approved_product_api_or_handoff",
        "explicit_handoff_lifecycle",
        "event_envelope_and_idempotency",
        "product_owned_tasks",
        "blocker_clearance_path",
        "recordarr_evidence_package",
        "read_model_source_freshness",
        "field_companion_behavior",
        "completion_or_closeout_state",
    ];

    private static readonly IReadOnlyList<string> SiteWorkflowCapabilities =
    [
        "public_submission_intake",
        "nexarr_or_scoped_access_validation",
        "route_to_owning_product",
        "reviewable_product_record",
        "recordarr_evidence_link",
        "audit_activity_history",
    ];

    public static readonly IReadOnlyList<StlMasterWorkflowSectionDescriptor> Sections =
    [
        Universal("universal", "Universal workflow mechanics", "GEN", 30),
        Product("nexarr", "NexArr", "NEX", 35, StlProductKeys.NexArr),
        Product("staffarr", "StaffArr", "STA", 49, StlProductKeys.StaffArr),
        Product("trainarr", "TrainArr", "TRN", 44, StlProductKeys.TrainArr),
        Product("maintainarr", "MaintainArr", "MNT", 64, StlProductKeys.MaintainArr),
        Product("routarr", "RoutArr", "RTE", 52, StlProductKeys.RoutArr),
        Product("supplyarr", "SupplyArr", "SUP", 42, StlProductKeys.SupplyArr),
        Product("loadarr", "LoadArr", "LOD", 50, StlProductKeys.LoadArr),
        Product("assurarr", "AssurArr", "ASR", 37, StlProductKeys.AssurArr),
        Product("recordarr", "RecordArr", "REC", 36, StlProductKeys.RecordArr),
        Product("ordarr", "OrdArr", "ORD", 34, StlProductKeys.OrdArr),
        Product("customarr", "CustomArr", "CUS", 40, StlProductKeys.CustomArr),
        Product("ledgarr", "LedgArr", "LED", 68, StlProductKeys.LedgArr),
        Product("reportarr", "ReportArr", "RPT", 27, StlProductKeys.ReportArr),
        Product("compliancecore", "Compliance Core", "CC", 54, StlProductKeys.ComplianceCore),
        Product("fieldcompanion", "Field Companion", "FC", 27, StlProductKeys.FieldCompanion),
        Site("stlcompliancesite", "STLComplianceSite", "SITE", 18),
        CrossSuite(
            "cross-suite-platform",
            "Cross-suite: platform, onboarding, and administration",
            "E2E-PLAT",
            25,
            StlProductKeys.NexArr,
            StlProductKeys.StaffArr,
            StlProductKeys.TrainArr,
            StlProductKeys.RecordArr,
            StlProductKeys.ComplianceCore,
            StlProductKeys.ReportArr),
        CrossSuite(
            "cross-suite-workforce",
            "Cross-suite: workforce, training, safety, and access",
            "E2E-WORK",
            25,
            StlProductKeys.StaffArr,
            StlProductKeys.TrainArr,
            StlProductKeys.ComplianceCore,
            StlProductKeys.RecordArr,
            StlProductKeys.MaintainArr,
            StlProductKeys.AssurArr,
            StlProductKeys.RoutArr,
            StlProductKeys.FieldCompanion),
        CrossSuite(
            "cross-suite-customer",
            "Cross-suite: customer, CRM, order, fulfillment, and cash",
            "E2E-CUST",
            29,
            StlProductKeys.CustomArr,
            StlProductKeys.OrdArr,
            StlProductKeys.LoadArr,
            StlProductKeys.RoutArr,
            StlProductKeys.RecordArr,
            StlProductKeys.LedgArr,
            StlProductKeys.ComplianceCore,
            StlProductKeys.ReportArr),
        CrossSuite(
            "cross-suite-procurement",
            "Cross-suite: procurement, receiving, inventory, quality, and payables",
            "E2E-PROC",
            31,
            StlProductKeys.SupplyArr,
            StlProductKeys.LoadArr,
            StlProductKeys.AssurArr,
            StlProductKeys.RecordArr,
            StlProductKeys.LedgArr,
            StlProductKeys.ComplianceCore,
            StlProductKeys.RoutArr),
        CrossSuite(
            "cross-suite-assets",
            "Cross-suite: assets, maintenance, readiness, transportation, and cost",
            "E2E-AST",
            24,
            StlProductKeys.MaintainArr,
            StlProductKeys.SupplyArr,
            StlProductKeys.LoadArr,
            StlProductKeys.RoutArr,
            StlProductKeys.StaffArr,
            StlProductKeys.TrainArr,
            StlProductKeys.ComplianceCore,
            StlProductKeys.RecordArr,
            StlProductKeys.LedgArr,
            StlProductKeys.ReportArr),
        CrossSuite(
            "cross-suite-transportation",
            "Cross-suite: transportation, warehouse, customer, quality, claims, and settlement",
            "E2E-TRN",
            26,
            StlProductKeys.RoutArr,
            StlProductKeys.LoadArr,
            StlProductKeys.CustomArr,
            StlProductKeys.OrdArr,
            StlProductKeys.AssurArr,
            StlProductKeys.RecordArr,
            StlProductKeys.LedgArr,
            StlProductKeys.ComplianceCore,
            StlProductKeys.FieldCompanion,
            StlProductKeys.ReportArr),
        CrossSuite(
            "cross-suite-compliance",
            "Cross-suite: quality, compliance, records, audits, reporting, and remediation",
            "E2E-CMP",
            36,
            StlProductKeys.ComplianceCore,
            StlProductKeys.AssurArr,
            StlProductKeys.RecordArr,
            StlProductKeys.ReportArr,
            StlProductKeys.StaffArr,
            StlProductKeys.TrainArr,
            StlProductKeys.MaintainArr,
            StlProductKeys.LoadArr,
            StlProductKeys.SupplyArr,
            StlProductKeys.RoutArr,
            StlProductKeys.CustomArr),
        CrossSuite(
            "cross-suite-finance",
            "Cross-suite: finance, close, controls, and executive management",
            "E2E-FIN",
            25,
            StlProductKeys.LedgArr,
            StlProductKeys.ReportArr,
            StlProductKeys.RecordArr,
            StlProductKeys.StaffArr,
            StlProductKeys.OrdArr,
            StlProductKeys.CustomArr,
            StlProductKeys.SupplyArr,
            StlProductKeys.LoadArr,
            StlProductKeys.MaintainArr,
            StlProductKeys.RoutArr,
            StlProductKeys.ComplianceCore),
        CrossSuite(
            "cross-suite-resilience",
            "Cross-suite: resilience, exceptions, privacy, and recovery",
            "E2E-RES",
            32,
            StlProductKeys.NexArr,
            StlProductKeys.RecordArr,
            StlProductKeys.ReportArr,
            StlProductKeys.ComplianceCore,
            StlProductKeys.FieldCompanion,
            StlProductKeys.StaffArr,
            StlProductKeys.TrainArr,
            StlProductKeys.MaintainArr,
            StlProductKeys.RoutArr,
            StlProductKeys.SupplyArr,
            StlProductKeys.LoadArr,
            StlProductKeys.AssurArr,
            StlProductKeys.OrdArr,
            StlProductKeys.CustomArr,
            StlProductKeys.LedgArr),
    ];

    public static IReadOnlyList<StlMasterWorkflowDescriptor> AllWorkflows { get; } =
        Sections.SelectMany(section => section.Expand()).ToArray();

    private static readonly IReadOnlyDictionary<string, StlMasterWorkflowDescriptor> WorkflowById =
        AllWorkflows.ToDictionary(x => x.WorkflowId, StringComparer.OrdinalIgnoreCase);

    public static bool TryGetWorkflow(
        string workflowId,
        out StlMasterWorkflowDescriptor? workflow) =>
        WorkflowById.TryGetValue(workflowId, out workflow);

    public static StlMasterWorkflowDescriptor GetWorkflow(string workflowId) =>
        TryGetWorkflow(workflowId, out var workflow)
            ? workflow!
            : throw new KeyNotFoundException($"Workflow '{workflowId}' is not in {CatalogName}.");

    private static StlMasterWorkflowSectionDescriptor Universal(
        string sectionKey,
        string title,
        string codePrefix,
        int count) =>
        new(
            sectionKey,
            title,
            codePrefix,
            1,
            count,
            StlMasterWorkflowSectionKind.UniversalMechanic,
            StlWorkflowCompletionMode.SharedMechanicAppliedByOwningProduct,
            PrimaryOwnerProductKey: null,
            UniversalWorkflowOwners,
            UniversalMechanicCapabilities);

    private static StlMasterWorkflowSectionDescriptor Product(
        string sectionKey,
        string title,
        string codePrefix,
        int count,
        string productKey) =>
        new(
            sectionKey,
            title,
            codePrefix,
            1,
            count,
            StlMasterWorkflowSectionKind.ProductWorkflow,
            StlWorkflowCompletionMode.ProductOwnedWorkflow,
            productKey,
            [productKey],
            ProductWorkflowCapabilities);

    private static StlMasterWorkflowSectionDescriptor Site(
        string sectionKey,
        string title,
        string codePrefix,
        int count) =>
        new(
            sectionKey,
            title,
            codePrefix,
            1,
            count,
            StlMasterWorkflowSectionKind.ProductWorkflow,
            StlWorkflowCompletionMode.PublicSiteIntakeToOwningProduct,
            StlProductKeys.StlComplianceSite,
            [StlProductKeys.StlComplianceSite, StlProductKeys.NexArr],
            SiteWorkflowCapabilities);

    private static StlMasterWorkflowSectionDescriptor CrossSuite(
        string sectionKey,
        string title,
        string codePrefix,
        int count,
        params string[] productKeys) =>
        new(
            sectionKey,
            title,
            codePrefix,
            1,
            count,
            StlMasterWorkflowSectionKind.CrossSuiteWorkflow,
            StlWorkflowCompletionMode.CrossProductWorkflowPack,
            PrimaryOwnerProductKey: null,
            productKeys,
            CrossSuiteWorkflowCapabilities);
}
