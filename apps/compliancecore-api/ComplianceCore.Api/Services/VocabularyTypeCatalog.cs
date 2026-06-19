using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class VocabularyTypeCatalog
{
    public static readonly (string TypeKey, string Label, string Description, int SortOrder)[] SystemTypes =
    [
        ("material_hazard", "Material Hazard", "Material hazard classifications used by SupplyArr and rule packs.", 10),
        ("physical_state", "Physical State", "Physical state of materials such as gas, liquid, or solid.", 20),
        ("compliance_domain", "Compliance Domain", "High-level compliance requirement domains.", 30),
        ("certification_category", "Certification Category", "Certification and qualification categories for StaffArr.", 40),
        ("readiness_blocker", "Readiness Blocker", "Categories for workforce readiness blockers.", 50),
        ("incident_reason", "Incident Reason", "Personnel incident reason categories for StaffArr.", 60),
        ("training_requirement", "Training Requirement", "Training requirement categories for TrainArr.", 70),
        ("inspection_category", "Inspection Category", "Maintenance inspection categories for MaintainArr.", 80),
        ("defect_severity", "Defect Severity", "Defect severity levels for maintenance workflows.", 90),
        ("dispatch_category", "Dispatch Category", "Dispatch work categories for RoutArr.", 100),
        ("dvir_reason", "DVIR Reason", "Driver vehicle inspection report reason codes.", 110),
        ("route_exception", "Route Exception", "Route exception categories for RoutArr.", 120),
        ("vendor_compliance", "Vendor Compliance", "Vendor compliance categories for SupplyArr.", 130),
        ("evidence_type", "Evidence Type", "Evidence document types used across products.", 140),
        ("document_class", "Document Class", "Platform reference data for document class selections.", 150),
        ("document_type", "Document Type", "Platform reference data for document type selections.", 160),
        ("document_subtype", "Document Subtype", "Platform reference data for document subtype selections.", 170)
    ];

    public static readonly HashSet<string> TypeKeys = SystemTypes
        .Select(x => x.TypeKey)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
