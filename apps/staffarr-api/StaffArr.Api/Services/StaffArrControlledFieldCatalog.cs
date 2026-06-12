using StaffArr.Api.Contracts;

namespace StaffArr.Api.Services;

internal static class StaffArrControlledFieldCatalog
{
    private const string StaffArrOwner = "staffarr";
    private const string ComplianceCoreOwner = "compliancecore";
    private const string NexArrOwner = "nexarr";
    private const string StaffArrSource = "staffarr.fieldset";
    private const string ComplianceCoreMappedSource = "compliancecore.mapped_staffarr_fieldset";
    private const string NexArrProductCatalogSource = "nexarr.product_catalog";

    public static readonly IReadOnlyList<string> ImplementedProductKeys =
    [
        "nexarr",
        "staffarr",
        "trainarr",
        "maintainarr",
        "routarr",
        "supplyarr",
        "compliancecore",
        "loadarr",
        "recordarr",
        "reportarr",
        "assurarr",
        "fieldcompanion",
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> EmploymentStatusOptions =
    [
        Option("applicant", "Applicant"),
        Option("pending_start", "Pending start"),
        Option("onboarding", "Onboarding"),
        Option("active", "Active"),
        Option("leave", "Leave"),
        Option("suspended", "Suspended"),
        Option("terminated", "Terminated"),
        Option("inactive", "Inactive"),
        Option("archived", "Archived"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> WorkRelationshipOptions =
    [
        Option("employee", "Employee"),
        Option("contractor", "Contractor"),
        Option("temp", "Temp"),
        Option("vendor_worker", "Vendor worker"),
        Option("customer_contact", "Customer contact"),
        Option("auditor", "Auditor"),
        Option("service_account_contact", "Service account contact"),
        Option("other", "Other"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> EmploymentTypeOptions =
    [
        Option("full_time", "Full time"),
        Option("part_time", "Part time"),
        Option("seasonal", "Seasonal"),
        Option("temporary", "Temporary"),
        Option("contract", "Contract"),
        Option("non_employee", "Non-employee"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> IncidentStatusOptions =
    [
        Option("draft", "Draft"),
        Option("submitted", "Submitted"),
        Option("open", "Open"),
        Option("in_review", "In review"),
        Option("closed", "Closed"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> IncidentSeverityOptions =
    [
        Option("low", "Low", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("medium", "Medium", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("high", "High", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("critical", "Critical", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> IncidentReasonCategoryOptions =
    [
        Option("safety", "Safety", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("conduct", "Conduct"),
        Option("behavior", "Behavior"),
        Option("injury", "Injury", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("equipment", "Equipment"),
        Option("equipment_damage", "Equipment damage"),
        Option("training_compliance", "Training compliance", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("training_issue", "Training issue"),
        Option("policy", "Policy"),
        Option("policy_violation", "Policy violation", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("attendance", "Attendance"),
        Option("near_miss", "Near miss", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("other", "Other"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> IncidentSourceOptions =
    [
        Option("staffarr", "StaffArr intake"),
        Option("self_report", "Employee self-report"),
        Option("manager_report", "Manager report"),
        Option("safety_observation", "Safety observation"),
        .. ProductOptions(" handoff"),
        Option("other", "Other controlled source"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> IncidentTypeOptions =
    [
        Option("injury", "Injury", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("safety", "Safety", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("behavior", "Behavior"),
        Option("equipment_damage", "Equipment damage"),
        Option("policy_violation", "Policy violation", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("training_issue", "Training issue"),
        Option("attendance", "Attendance"),
        Option("near_miss", "Near miss", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("other", "Other"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> ReadinessDecisionOptions =
    [
        Option("allowed", "Allowed", "May continue normal work", ComplianceCoreOwner, ComplianceCoreMappedSource),
        Option("watched", "Watched", "Needs watch or monitoring", ComplianceCoreOwner, ComplianceCoreMappedSource),
        Option("restricted", "Restricted", "Must be restricted or limited", ComplianceCoreOwner, ComplianceCoreMappedSource),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> WorkRestrictionOptions =
    [
        Option("none", "None", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("modified_duty", "Modified duty", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("restricted_duty", "Restricted duty", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("removed_from_duty", "Removed from duty", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> YesNoPendingOptions =
    [
        Option("no", "No"),
        Option("yes", "Yes"),
        Option("pending", "Pending"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> MedicalAttentionOptions =
    [
        Option("none", "None", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("first_aid", "First aid", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("clinic", "Clinic", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("emergency", "Emergency", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("unknown", "Unknown", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> PpeConcernOptions =
    [
        Option("none", "None", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("damaged", "Damaged", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("missing", "Missing", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("inadequate", "Inadequate", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("unknown", "Unknown", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> FollowUpOptions =
    [
        Option("no", "No"),
        Option("yes", "Yes"),
        Option("conditional", "Conditional"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> TrainingReviewReasonOptions =
    [
        Option("certification_gap", "Certification gap", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("procedure_gap", "Procedure gap", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("behavior_coaching", "Behavior coaching"),
        Option("remedial_training", "Remedial training"),
        Option("other", "Other training reason"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> IncidentNoteTypeOptions =
    [
        Option("note", "Note"),
        Option("corrective_action", "Corrective action", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> IncidentNoteStatusOptions =
    [
        Option("open", "Open"),
        Option("completed", "Completed"),
    ];

    public static readonly IReadOnlySet<string> EmploymentStatusKeys = KeySet(EmploymentStatusOptions);
    public static readonly IReadOnlySet<string> WorkRelationshipKeys = KeySet(WorkRelationshipOptions);
    public static readonly IReadOnlySet<string> EmploymentTypeKeys = KeySet(EmploymentTypeOptions);
    public static readonly IReadOnlySet<string> IncidentStatusKeys = KeySet(IncidentStatusOptions);
    public static readonly IReadOnlySet<string> IncidentSeverityKeys = KeySet(IncidentSeverityOptions);
    public static readonly IReadOnlySet<string> IncidentReasonCategoryKeys = KeySet(IncidentReasonCategoryOptions);
    public static readonly IReadOnlySet<string> IncidentSourceKeys = KeySet(IncidentSourceOptions);
    public static readonly IReadOnlySet<string> IncidentTypeKeys = KeySet(IncidentTypeOptions);
    public static readonly IReadOnlySet<string> ReadinessDecisionKeys = KeySet(ReadinessDecisionOptions);
    public static readonly IReadOnlySet<string> WorkRestrictionKeys = KeySet(WorkRestrictionOptions);
    public static readonly IReadOnlySet<string> YesNoPendingKeys = KeySet(YesNoPendingOptions);
    public static readonly IReadOnlySet<string> MedicalAttentionKeys = KeySet(MedicalAttentionOptions);
    public static readonly IReadOnlySet<string> PpeConcernKeys = KeySet(PpeConcernOptions);
    public static readonly IReadOnlySet<string> FollowUpKeys = KeySet(FollowUpOptions);
    public static readonly IReadOnlySet<string> TrainingReviewReasonKeys = KeySet(TrainingReviewReasonOptions);
    public static readonly IReadOnlySet<string> IncidentNoteTypeKeys = KeySet(IncidentNoteTypeOptions);
    public static readonly IReadOnlySet<string> IncidentNoteStatusKeys = KeySet(IncidentNoteStatusOptions);
    public static readonly IReadOnlySet<string> SourceProductKeys = new HashSet<string>(ImplementedProductKeys, StringComparer.OrdinalIgnoreCase);

    public static StaffArrFieldsetResponse GetPeopleProfileFieldset() =>
        new(
            "people.profile",
            "People profile",
            "staff_person",
            "profile",
            [
                Field("employmentStatus", "Employment status", "select", true, EmploymentStatusOptions),
                Field("workRelationshipType", "Work relationship", "select", true, WorkRelationshipOptions),
                Field("employmentType", "Employment type", "select", false, EmploymentTypeOptions),
            ]);

    public static StaffArrFieldsetResponse GetPersonnelIncidentCreateFieldset() =>
        new(
            "personnel-incidents.create",
            "Personnel incident create",
            "personnel_incident",
            "create",
            [
                Field("incidentSource", "Incident source", "select", true, IncidentSourceOptions),
                Field("incidentType", "Incident type", "select", true, IncidentTypeOptions),
                Field("reasonCategoryKey", "Reason category", "select", true, IncidentReasonCategoryOptions),
                Field("severity", "Severity", "select", true, IncidentSeverityOptions),
                Field("readinessDecision", "Readiness decision", "select", true, ReadinessDecisionOptions),
                Field("workRestriction", "Work restriction", "select", true, WorkRestrictionOptions),
                Field("yesNoPending", "Yes/no/pending", "select", false, YesNoPendingOptions),
                Field("ppeConcern", "PPE concern", "select", false, PpeConcernOptions),
                Field("medicalAttention", "Medical attention", "select", false, MedicalAttentionOptions),
                Field("followUpRequired", "Follow-up required", "select", true, FollowUpOptions),
                Field("trainingReviewReason", "Training review reason", "select", false, TrainingReviewReasonOptions),
            ]);

    private static StaffArrFieldDefinitionResponse Field(
        string key,
        string label,
        string control,
        bool required,
        IReadOnlyList<StaffArrFieldOptionResponse> options) =>
        new(key, label, control, required, StaffArrOwner, StaffArrSource, options);

    private static StaffArrFieldOptionResponse Option(
        string value,
        string label,
        string? hint = null,
        string owner = StaffArrOwner,
        string sourceOfTruth = StaffArrSource) =>
        new(value, label, hint, owner, sourceOfTruth);

    private static StaffArrFieldOptionResponse[] ProductOptions(string labelSuffix) =>
        ImplementedProductKeys
            .Select(productKey => Option(
                productKey,
                $"{LabelizeProductKey(productKey)}{labelSuffix}",
                owner: NexArrOwner,
                sourceOfTruth: NexArrProductCatalogSource))
            .ToArray();

    private static string LabelizeProductKey(string productKey) =>
        productKey switch
        {
            "nexarr" => "NexArr",
            "staffarr" => "StaffArr",
            "trainarr" => "TrainArr",
            "maintainarr" => "MaintainArr",
            "routarr" => "RoutArr",
            "supplyarr" => "SupplyArr",
            "compliancecore" => "Compliance Core",
            "loadarr" => "LoadArr",
            "recordarr" => "RecordArr",
            "reportarr" => "ReportArr",
            "assurarr" => "AssurArr",
            "fieldcompanion" => "Field Companion",
            _ => productKey,
        };

    private static IReadOnlySet<string> KeySet(IReadOnlyList<StaffArrFieldOptionResponse> options) =>
        new HashSet<string>(options.Select(option => option.Value), StringComparer.OrdinalIgnoreCase);
}
