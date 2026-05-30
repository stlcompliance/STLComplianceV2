using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ComplianceEvidenceOptionGroup : IHasTenant
{
    public Guid EvidenceOptionGroupId { get; set; }

    public Guid TenantId { get; set; }

    public string RequirementKey { get; set; } = string.Empty;

    public string FactKey { get; set; } = string.Empty;

    public string PackKey { get; set; } = string.Empty;

    public string CitationKey { get; set; } = string.Empty;

    public string LogicType { get; set; } = EvidenceOptionLogicTypes.AnyOf;

    public string ApplicabilityKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ComplianceEvidenceOption : IHasTenant
{
    public Guid EvidenceOptionId { get; set; }

    public Guid TenantId { get; set; }

    public Guid EvidenceOptionGroupId { get; set; }

    public string OptionKey { get; set; } = string.Empty;

    public string OptionLabel { get; set; } = string.Empty;

    public string EvidenceKind { get; set; } = FactRequirementEvidenceKinds.ProductRecord;

    public string TargetKind { get; set; } = EvidenceOptionTargetKinds.ProductRecord;

    public string SourceProduct { get; set; } = string.Empty;

    public string SourceEntity { get; set; } = string.Empty;

    public string SourceFieldOrRecordType { get; set; } = string.Empty;

    public string DocumentTypeKey { get; set; } = string.Empty;

    public string MaterialKey { get; set; } = string.Empty;

    public string PartKey { get; set; } = string.Empty;

    public string SystemKey { get; set; } = string.Empty;

    public string AssetKind { get; set; } = string.Empty;

    public string ExternalRegistryKey { get; set; } = string.Empty;

    public string FactKey { get; set; } = string.Empty;

    public bool Required { get; set; } = true;

    public int Priority { get; set; } = 1;

    public decimal? ConfidenceHint { get; set; }

    public bool Active { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ComplianceEvidenceOptionGroup? EvidenceOptionGroup { get; set; }
}

public static class EvidenceOptionLogicTypes
{
    public const string AnyOf = "any_of";
    public const string AllOf = "all_of";
    public const string OneOf = "one_of";
    public const string Conditional = "conditional";
    public const string Derived = "derived";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        AnyOf,
        AllOf,
        OneOf,
        Conditional,
        Derived
    };
}

public static class EvidenceOptionTargetKinds
{
    public const string DocumentType = "document_type";
    public const string DocumentRecord = "document_record";
    public const string Material = "material";
    public const string Part = "part";
    public const string System = "system";
    public const string Asset = "asset";
    public const string ProductRecord = "product_record";
    public const string Fact = "fact";
    public const string ExternalRegistry = "external_registry";
    public const string DerivedFact = "derived_fact";
    public const string Signature = "signature";
    public const string Review = "review";
    public const string NoDocumentRequired = "no_document_required";
    public const string ExceptionExemption = "exception_exemption";
    public const string Waiver = "waiver";
    public const string Variance = "variance";
    public const string SpecialPermit = "special_permit";
    public const string Approval = "approval";
    public const string AlternateCompliancePath = "alternate_compliance_path";
    public const string ConditionalExclusion = "conditional_exclusion";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        DocumentType,
        DocumentRecord,
        Material,
        Part,
        System,
        Asset,
        ProductRecord,
        Fact,
        ExternalRegistry,
        DerivedFact,
        Signature,
        Review,
        NoDocumentRequired,
        ExceptionExemption,
        Waiver,
        Variance,
        SpecialPermit,
        Approval,
        AlternateCompliancePath,
        ConditionalExclusion
    };
}

public static class MappingTargetKinds
{
    public const string ExistingDocumentType = "existing_document_type";
    public const string NewDocumentType = "new_document_type";
    public const string ExistingDocumentRecord = "existing_document_record";
    public const string NewDocumentRecord = "new_document_record";
    public const string ExistingMaterial = "existing_material";
    public const string NewMaterial = "new_material";
    public const string ExistingPart = "existing_part";
    public const string NewPart = "new_part";
    public const string ExistingSystem = "existing_system";
    public const string NewSystem = "new_system";
    public const string ExistingAsset = "existing_asset";
    public const string NewAsset = "new_asset";
    public const string ExistingEvidenceReference = "existing_evidence_reference";
    public const string NewEvidenceReference = "new_evidence_reference";
    public const string ExistingFactDefinition = "existing_fact_definition";
    public const string NewFactDefinition = "new_fact_definition";
    public const string ExistingComplianceKey = "existing_compliance_key";
    public const string NewComplianceKey = "new_compliance_key";
    public const string ExistingCitation = "existing_citation";
    public const string NewCitation = "new_citation";
    public const string ExistingControlledVocabularyTerm = "existing_controlled_vocabulary_term";
    public const string NewControlledVocabularyTerm = "new_controlled_vocabulary_term";
    public const string ExistingAlias = "existing_alias";
    public const string NewAlias = "new_alias";
    public const string NoDocumentRequired = "no_document_required";
    public const string ExternalRegistry = "external_registry";
    public const string ProductRecord = "product_record";
    public const string DerivedFact = "derived_fact";
    public const string ExceptionExemption = "exception_exemption";
    public const string Waiver = "waiver";
    public const string Variance = "variance";
    public const string SpecialPermit = "special_permit";
    public const string Approval = "approval";
    public const string AlternateCompliancePath = "alternate_compliance_path";
    public const string ConditionalExclusion = "conditional_exclusion";
}
