using StaffArr.Api.Contracts;

namespace StaffArr.Api.Services;

internal static class StaffArrControlledFieldCatalog
{
    private const string StaffArrOwner = "staffarr";
    private const string ComplianceCoreOwner = "compliancecore";
    private const string NexArrOwner = "nexarr";
    private const string StaffArrSource = "staffarr.fieldset";
    private const string StaffArrPersonCatalogSource = "staffarr.person.field_catalog";
    private const string StaffArrEmploymentApplicationBuilderSource = "staffarr.employment_application_builder";
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
        "customarr",
        "ordarr",
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

    public static readonly IReadOnlyList<EmploymentApplicationControlOptionResponse> EmploymentApplicationControlOptions =
    [
        new("text", "Short text", "One-line response."),
        new("textarea", "Long text", "Multi-line response."),
        new("email", "Email", "Validated email address."),
        new("phone", "Phone", "Validated phone number."),
        new("date", "Date", "Calendar date picker."),
        new("select", "Single select", "Choose one option."),
        new("multi_select", "Multi select", "Choose one or more options."),
        new("number", "Number", "Numeric input."),
        new("yes_no", "Yes / no", "Boolean choice."),
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
        Option("staffarr", "StaffArr incident intake"),
        Option("self_report", "Employee self-report"),
        Option("manager_report", "Manager report"),
        Option("safety_observation", "Safety observation"),
        Option("nexarr", "NexArr access / launch issue", "Platform login, entitlement, or launch context", NexArrOwner, NexArrProductCatalogSource),
        Option("trainarr", "TrainArr training context", "Training, qualification, or remediation context", NexArrOwner, NexArrProductCatalogSource),
        Option("maintainarr", "MaintainArr defect / work-order context", "Maintenance or defect context", NexArrOwner, NexArrProductCatalogSource),
        Option("routarr", "RoutArr trip / dispatch context", "Route, trip, or dispatch exception context", NexArrOwner, NexArrProductCatalogSource),
        Option("supplyarr", "SupplyArr procurement context", "Vendor, item, or purchasing context", NexArrOwner, NexArrProductCatalogSource),
        Option("customarr", "CustomArr customer context", "Customer or account context", NexArrOwner, NexArrProductCatalogSource),
        Option("ordarr", "OrdArr request context", "Order or request context", NexArrOwner, NexArrProductCatalogSource),
        Option("compliancecore", "Compliance Core review context", "Rule, evidence, or applicability context", NexArrOwner, NexArrProductCatalogSource),
        Option("loadarr", "LoadArr warehouse context", "Receiving, stock, or warehouse context", NexArrOwner, NexArrProductCatalogSource),
        Option("recordarr", "RecordArr evidence context", "Controlled document or evidence context", NexArrOwner, NexArrProductCatalogSource),
        Option("reportarr", "ReportArr report / alert", "Dashboard, report, or alert context", NexArrOwner, NexArrProductCatalogSource),
        Option("assurarr", "AssurArr quality finding", "Assurance, CAPA, or nonconformance context", NexArrOwner, NexArrProductCatalogSource),
        Option("fieldcompanion", "Field Companion field report", "Mobile capture or field report context", NexArrOwner, NexArrProductCatalogSource),
        Option("other", "Other controlled source"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> IncidentTypeOptions =
    [
        Option("injury", "Injury", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("safety", "Safety", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
        Option("vehicle_accident", "Vehicle Accident", owner: ComplianceCoreOwner, sourceOfTruth: ComplianceCoreMappedSource),
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

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> EmploymentLifecycleActionOptions =
    [
        Option("hire", "Hire"),
        Option("rehire", "Rehire"),
        Option("transfer", "Transfer"),
        Option("promotion", "Promotion"),
        Option("demotion", "Demotion"),
        Option("supervisor_change", "Supervisor change"),
        Option("location_change", "Location change"),
        Option("job_change", "Job / position change"),
        Option("leave_start", "Leave start"),
        Option("leave_return", "Leave return"),
        Option("suspension", "Suspension"),
        Option("termination", "Termination"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> WorkerCategoryOptions =
    [
        Option("employee", "Employee"),
        Option("contractor", "Contractor"),
        Option("intern", "Intern"),
        Option("temporary", "Temporary"),
        Option("seasonal", "Seasonal"),
        Option("volunteer", "Volunteer"),
        Option("other", "Other"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> FlsaStatusOptions =
    [
        Option("exempt", "Exempt"),
        Option("non_exempt", "Non-exempt"),
        Option("outside_scope", "Outside scope"),
        Option("unknown", "Unknown"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> DocumentAccessLevelOptions =
    [
        Option("employee", "Employee"),
        Option("manager", "Manager"),
        Option("hr", "HR"),
        Option("restricted", "Restricted"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> RetentionCategoryOptions =
    [
        Option("personnel_file", "Personnel file"),
        Option("employment_eligibility", "Employment eligibility"),
        Option("discipline", "Discipline"),
        Option("performance", "Performance"),
        Option("leave", "Leave"),
        Option("termination", "Termination"),
        Option("medical", "Medical"),
        Option("eeo", "EEO"),
        Option("other", "Other"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> HrRecordStatusOptions =
    [
        Option("draft", "Draft"),
        Option("active", "Active"),
        Option("pending_review", "Pending review"),
        Option("closed", "Closed"),
        Option("archived", "Archived"),
        Option("sealed", "Sealed"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> PositionStatusOptions =
    [
        Option("planned", "Planned"),
        Option("open", "Open"),
        Option("filled", "Filled"),
        Option("vacant", "Vacant"),
        Option("closed", "Closed"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> CaseTypeOptions =
    [
        Option("hr_complaint", "HR complaint"),
        Option("investigation", "Investigation"),
        Option("grievance", "Grievance"),
        Option("accommodation", "Accommodation"),
        Option("performance", "Performance"),
        Option("injury", "Injury"),
        Option("labor_relation", "Labor relation"),
        Option("recruiting", "Recruiting"),
        Option("other", "Other"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> CaseSeverityOptions =
    [
        Option("low", "Low"),
        Option("medium", "Medium"),
        Option("high", "High"),
        Option("critical", "Critical"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> CaseConfidentialityOptions =
    [
        Option("open", "Open"),
        Option("manager", "Manager"),
        Option("hr", "HR"),
        Option("restricted", "Restricted"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> ReviewCycleOptions =
    [
        Option("probation", "Probation"),
        Option("quarterly", "Quarterly"),
        Option("annual", "Annual"),
        Option("ad_hoc", "Ad hoc"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> BenefitEnrollmentStatusOptions =
    [
        Option("eligible", "Eligible"),
        Option("pending", "Pending"),
        Option("enrolled", "Enrolled"),
        Option("waived", "Waived"),
        Option("terminated", "Terminated"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> CompensationChangeReasonOptions =
    [
        Option("hire", "Hire"),
        Option("promotion", "Promotion"),
        Option("market_adjustment", "Market adjustment"),
        Option("merit", "Merit"),
        Option("demotion", "Demotion"),
        Option("cost_of_living", "Cost of living"),
        Option("other", "Other"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> RecruitingStageOptions =
    [
        Option("requisition", "Requisition"),
        Option("candidate", "Candidate"),
        Option("application", "Application"),
        Option("interview", "Interview"),
        Option("offer", "Offer"),
        Option("hired", "Hired"),
        Option("withdrawn", "Withdrawn"),
        Option("closed", "Closed"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> LaborRelationStatusOptions =
    [
        Option("active", "Active"),
        Option("watch", "Watch"),
        Option("grievance", "Grievance"),
        Option("resolved", "Resolved"),
        Option("closed", "Closed"),
    ];

    public static readonly IReadOnlyList<StaffArrFieldOptionResponse> InjuryStatusOptions =
    [
        Option("open", "Open"),
        Option("restricted_duty", "Restricted duty"),
        Option("lost_time", "Lost time"),
        Option("returned", "Returned"),
        Option("closed", "Closed"),
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
    public static readonly IReadOnlySet<string> EmploymentLifecycleActionKeys = KeySet(EmploymentLifecycleActionOptions);
    public static readonly IReadOnlySet<string> WorkerCategoryKeys = KeySet(WorkerCategoryOptions);
    public static readonly IReadOnlySet<string> FlsaStatusKeys = KeySet(FlsaStatusOptions);
    public static readonly IReadOnlySet<string> DocumentAccessLevelKeys = KeySet(DocumentAccessLevelOptions);
    public static readonly IReadOnlySet<string> RetentionCategoryKeys = KeySet(RetentionCategoryOptions);
    public static readonly IReadOnlySet<string> HrRecordStatusKeys = KeySet(HrRecordStatusOptions);
    public static readonly IReadOnlySet<string> PositionStatusKeys = KeySet(PositionStatusOptions);
    public static readonly IReadOnlySet<string> CaseTypeKeys = KeySet(CaseTypeOptions);
    public static readonly IReadOnlySet<string> CaseSeverityKeys = KeySet(CaseSeverityOptions);
    public static readonly IReadOnlySet<string> CaseConfidentialityKeys = KeySet(CaseConfidentialityOptions);
    public static readonly IReadOnlySet<string> ReviewCycleKeys = KeySet(ReviewCycleOptions);
    public static readonly IReadOnlySet<string> BenefitEnrollmentStatusKeys = KeySet(BenefitEnrollmentStatusOptions);
    public static readonly IReadOnlySet<string> CompensationChangeReasonKeys = KeySet(CompensationChangeReasonOptions);
    public static readonly IReadOnlySet<string> RecruitingStageKeys = KeySet(RecruitingStageOptions);
    public static readonly IReadOnlySet<string> LaborRelationStatusKeys = KeySet(LaborRelationStatusOptions);
    public static readonly IReadOnlySet<string> InjuryStatusKeys = KeySet(InjuryStatusOptions);
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
                Field("workerCategory", "Worker category", "select", false, WorkerCategoryOptions),
                Field("flsaStatus", "FLSA status", "select", false, FlsaStatusOptions),
            ]);

    public static EmploymentApplicationBuilderCatalogResponse GetEmploymentApplicationBuilderCatalog() =>
        new(
            EmploymentApplicationControlOptions,
            [
                new(
                    "identity",
                    "Identity",
                    [
                        Target("legal_first_name", "Legal first name", "create", "Primary legal first name for the new person record."),
                        Target("legal_middle_name", "Legal middle name", "create", "Optional middle name from the applicant."),
                        Target("legal_last_name", "Legal last name", "create", "Primary legal last name for the new person record."),
                        Target("preferred_name", "Preferred name", "eventual", "Queued for profile review after submission."),
                        Target("pronouns", "Pronouns", "eventual", "Queued for profile review after submission."),
                        Target("given_name", "Given name", "create", "Derived display first name used by StaffArr."),
                        Target("family_name", "Family name", "create", "Derived display last name used by StaffArr."),
                    ]),
                new(
                    "contact",
                    "Contact",
                    [
                        Target("primary_email", "Primary email", "create", "Login and contact email."),
                        Target("alternate_email", "Alternate email", "create", "Optional secondary email address."),
                        Target("primary_phone", "Primary phone", "create", "Primary phone number."),
                        Target("alternate_phone", "Alternate phone", "create", "Optional alternate phone number."),
                        Target("work_phone", "Work phone", "create", "Work phone number."),
                        Target("can_login", "Allow login", "create", "Whether NexArr login should be provisioned."),
                    ]),
                new(
                    "employment",
                    "Employment",
                    [
                        Target("work_relationship_type", "Work relationship", "create", "Canonical StaffArr employment relationship."),
                        Target("employment_type", "Employment type", "create", "Canonical StaffArr employment type."),
                        Target("worker_category", "Worker category", "eventual", "Profile review queue value."),
                        Target("flsa_status", "FLSA status", "eventual", "Profile review queue value."),
                        Target("position_number", "Position number", "eventual", "Profile review queue value."),
                        Target("current_employment_action", "Current employment action", "eventual", "Profile review queue value."),
                        Target("current_employment_action_at", "Current employment action at", "eventual", "Profile review queue value."),
                        Target("leave_status", "Leave status", "eventual", "Profile review queue value."),
                        Target("eligible_for_rehire", "Eligible for rehire", "create", "Whether the applicant is eligible for rehire."),
                        Target("job_title", "Job title", "create", "Requested job title or position applying for."),
                        Target("start_date", "Start date", "create", "Actual start date when known."),
                        Target("expected_start_date", "Expected start date", "create", "Planned first day if not yet hired."),
                    ]),
                new(
                    "placement",
                    "Placement",
                    [
                        Target("primary_org_unit_id", "Primary org unit", "eventual", "Profile review queue value."),
                        Target("site_org_unit_id", "Site org unit", "eventual", "Profile review queue value."),
                        Target("department_org_unit_id", "Department org unit", "eventual", "Profile review queue value."),
                        Target("team_org_unit_id", "Team org unit", "eventual", "Profile review queue value."),
                        Target("position_org_unit_id", "Position org unit", "eventual", "Profile review queue value."),
                        Target("manager_person_id", "Manager person", "eventual", "Profile review queue value."),
                        Target("home_base_location_id", "Home base location", "eventual", "Profile review queue value."),
                    ]),
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

    public static StaffArrFieldsetResponse GetHrmProgramFieldset(string module) =>
        module.Trim().ToLowerInvariant() switch
        {
            "lifecycle" => new(
                "hrm.lifecycle",
                "Employment lifecycle",
                "employment_lifecycle",
                "Effective-dated hire, transfer, leave, and termination history.",
                [
                    Field("actionKey", "Action", "select", true, EmploymentLifecycleActionOptions),
                    Field("effectiveAt", "Effective at", "datetime", true),
                    Field("status", "Status", "select", true, HrRecordStatusOptions),
                    Field("personId", "Person ID", "text", false),
                    Field("reason", "Reason", "textarea", false),
                    Field("workerCategory", "Worker category", "select", false, WorkerCategoryOptions),
                    Field("flsaStatus", "FLSA status", "select", false, FlsaStatusOptions),
                    Field("positionNumber", "Position number", "text", false),
                    Field("eligibleForRehire", "Eligible for rehire", "select", false, YesNoPendingOptions),
                ]),
            "personnel-file" => new(
                "hrm.personnel-file",
                "Personnel file",
                "personnel_file",
                "Controlled personnel document metadata with retention and access rules.",
                [
                    Field("documentTypeKey", "Document type", "text", true),
                    Field("accessLevel", "Access level", "select", true, DocumentAccessLevelOptions),
                    Field("retentionCategory", "Retention category", "select", true, RetentionCategoryOptions),
                    Field("restrictedData", "Restricted data", "select", true, YesNoPendingOptions),
                    Field("title", "Title", "text", true),
                    Field("expiresAt", "Expires at", "datetime", false),
                ]),
            "onboarding-offboarding" => new(
                "hrm.onboarding-offboarding",
                "Onboarding and offboarding",
                "workforce_journey",
                "Task-driven new-hire and separation orchestration.",
                [
                    Field("journeyType", "Journey type", "select", true, [
                        Option("onboarding", "Onboarding"),
                        Option("offboarding", "Offboarding"),
                    ]),
                    Field("owner", "Task owner", "text", true),
                    Field("dueAt", "Due at", "datetime", false),
                    Field("externalHandoff", "External handoff", "select", false, YesNoPendingOptions),
                    Field("badgeAccess", "Badge / access", "select", false, YesNoPendingOptions),
                    Field("finalClearance", "Final clearance", "select", false, YesNoPendingOptions),
                ]),
            "position-control" => new(
                "hrm.position-control",
                "Position control",
                "position_profile",
                "Job architecture, headcount, and reporting structure.",
                [
                    Field("jobFamily", "Job family", "text", true),
                    Field("jobProfile", "Job profile", "text", true),
                    Field("jobCode", "Job code", "text", false),
                    Field("positionNumber", "Position number", "text", true),
                    Field("positionStatus", "Position status", "select", true, PositionStatusOptions),
                    Field("budgetedHeadcount", "Budgeted headcount", "text", false),
                    Field("reportsTo", "Reports to", "text", false),
                    Field("department", "Department", "text", false),
                    Field("homeLocation", "Home location", "text", false),
                ]),
            "time-leave" => new(
                "hrm.time-leave",
                "Time, leave, and attendance",
                "time_and_leave",
                "Availability, leave cases, schedules, and attendance controls.",
                [
                    Field("leaveStatus", "Leave status", "select", false, [
                        Option("active", "Active"),
                        Option("leave", "Leave"),
                        Option("suspended", "Suspended"),
                    ]),
                    Field("availability", "Availability", "text", false),
                    Field("schedule", "Schedule", "text", false),
                    Field("timekeepingMode", "Timekeeping mode", "text", false),
                    Field("overtimeEligible", "Overtime eligible", "select", false, YesNoPendingOptions),
                    Field("payrollReady", "Payroll ready", "select", false, YesNoPendingOptions),
                ]),
            "classification" => new(
                "hrm.classification",
                "Classification",
                "employment_classification",
                "FLSA, worker category, and classification review state.",
                [
                    Field("workerCategory", "Worker category", "select", true, WorkerCategoryOptions),
                    Field("flsaStatus", "FLSA status", "select", true, FlsaStatusOptions),
                    Field("employmentType", "Employment type", "select", false, EmploymentTypeOptions),
                    Field("reviewStatus", "Review status", "select", false, HrRecordStatusOptions),
                    Field("overtimeEligibility", "Overtime eligibility", "select", false, YesNoPendingOptions),
                    Field("mealBreakGroup", "Meal break group", "text", false),
                ]),
            "casework" => new(
                "hrm.casework",
                "HR casework",
                "hr_case",
                "Complaints, investigations, grievances, and accommodation cases.",
                [
                    Field("caseType", "Case type", "select", true, CaseTypeOptions),
                    Field("severity", "Severity", "select", true, CaseSeverityOptions),
                    Field("confidentiality", "Confidentiality", "select", true, CaseConfidentialityOptions),
                    Field("status", "Status", "select", true, HrRecordStatusOptions),
                    Field("outcome", "Outcome", "text", false),
                    Field("legalHold", "Legal hold", "select", false, YesNoPendingOptions),
                ]),
            "performance-benefits-compensation" => new(
                "hrm.performance-benefits-compensation",
                "Performance / benefits / compensation",
                "hr_performance_benefits_compensation",
                "Review cycles, eligibility, enrollments, and compensation change tracking.",
                [
                    Field("reviewCycle", "Review cycle", "select", false, ReviewCycleOptions),
                    Field("goalStatus", "Goal status", "select", false, HrRecordStatusOptions),
                    Field("benefitEnrollmentStatus", "Benefit enrollment", "select", false, BenefitEnrollmentStatusOptions),
                    Field("benefitClass", "Benefit class", "text", false),
                    Field("payGrade", "Pay grade", "text", false),
                    Field("payBand", "Pay band", "text", false),
                    Field("compChangeReason", "Comp change reason", "select", false, CompensationChangeReasonOptions),
                    Field("effectiveDate", "Effective date", "datetime", false),
                ]),
            "recruiting-labor-relations-injury" => new(
                "hrm.recruiting-labor-relations-injury",
                "Recruiting, labor relations, and injury",
                "hr_recruiting_labor_injury",
                "Applicant tracking, labor relations, and workplace injury case tracking.",
                [
                    Field("requisitionStage", "Requisition stage", "select", false, RecruitingStageOptions),
                    Field("candidateStage", "Candidate stage", "select", false, RecruitingStageOptions),
                    Field("unionLocal", "Union / local", "text", false),
                    Field("seniorityDate", "Seniority date", "datetime", false),
                    Field("injuryStatus", "Injury status", "select", false, InjuryStatusOptions),
                    Field("returnToWork", "Return to work", "select", false, YesNoPendingOptions),
                    Field("oshaRecordability", "OSHA recordability", "select", false, YesNoPendingOptions),
                ]),
            "analytics" => new(
                "hrm.analytics",
                "HR analytics",
                "hr_analytics",
                "Operational HR metrics and readiness snapshots.",
                [
                    Field("headcount", "Headcount", "text", false),
                    Field("turnover", "Turnover", "text", false),
                    Field("openPositions", "Open positions", "text", false),
                    Field("vacancyAging", "Vacancy aging", "text", false),
                    Field("absenceRate", "Absence rate", "text", false),
                    Field("overtimeExposure", "Overtime exposure", "text", false),
                    Field("coverageByRole", "Coverage by role", "text", false),
                ]),
            _ => new(
                "hrm.program",
                "HRM program",
                "hrm_program",
                "Composite catalog of StaffArr HRM/HCM feature surfaces.",
                [
                    Field("lifecycle", "Employment lifecycle", "text", false),
                    Field("personnelFile", "Personnel file", "text", false),
                    Field("onboardingOffboarding", "Onboarding / offboarding", "text", false),
                    Field("positionControl", "Position control", "text", false),
                    Field("timeLeave", "Time / leave", "text", false),
                    Field("classification", "Classification", "text", false),
                    Field("casework", "Casework", "text", false),
                    Field("performance", "Performance", "text", false),
                    Field("benefits", "Benefits", "text", false),
                    Field("compensation", "Compensation", "text", false),
                    Field("recruiting", "Recruiting", "text", false),
                    Field("laborRelations", "Labor relations", "text", false),
                    Field("injury", "Injury", "text", false),
                    Field("analytics", "Analytics", "text", false),
                ])
        };

    private static StaffArrFieldDefinitionResponse Field(
        string key,
        string label,
        string control,
        bool required,
        IReadOnlyList<StaffArrFieldOptionResponse> options) =>
        new(key, label, control, required, StaffArrOwner, StaffArrSource, options);

    private static StaffArrFieldDefinitionResponse Field(
        string key,
        string label,
        string control,
        bool required) =>
        new(key, label, control, required, StaffArrOwner, StaffArrSource, []);

    private static StaffArrFieldOptionResponse Option(
        string value,
        string label,
        string? hint = null,
        string owner = StaffArrOwner,
        string sourceOfTruth = StaffArrSource) =>
        new(value, label, hint, owner, sourceOfTruth);

    private static EmploymentApplicationTargetFieldResponse Target(
        string value,
        string label,
        string stage,
        string? hint = null) =>
        new(
            value,
            label,
            stage,
            hint,
            StaffArrOwner,
            stage == "create" ? StaffArrEmploymentApplicationBuilderSource : StaffArrPersonCatalogSource);

    private static IReadOnlySet<string> KeySet(IReadOnlyList<StaffArrFieldOptionResponse> options) =>
        new HashSet<string>(options.Select(option => option.Value), StringComparer.OrdinalIgnoreCase);
}
