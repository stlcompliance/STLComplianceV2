using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ComplianceExceptionExemption : IHasTenant
{
    public Guid ExceptionExemptionId { get; set; }

    public Guid TenantId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Type { get; set; } = ComplianceExceptionExemptionTypes.RegulatoryException;

    public string GoverningBody { get; set; } = string.Empty;

    public string ProgramKey { get; set; } = string.Empty;

    public string PackKey { get; set; } = string.Empty;

    public string CitationKey { get; set; } = string.Empty;

    public string ApplicabilityKey { get; set; } = string.Empty;

    public string AppliesToSubjectKind { get; set; } = string.Empty;

    public string AppliesToSourceProduct { get; set; } = string.Empty;

    public string AppliesToSourceEntity { get; set; } = string.Empty;

    public string EffectType { get; set; } = ComplianceExceptionExemptionEffectTypes.MakesRequirementNotApplicable;

    public string ConditionLogicJson { get; set; } = "{}";

    public Guid? RequiredEvidenceOptionGroupId { get; set; }

    public string IssuingAuthority { get; set; } = string.Empty;

    public string AuthorizationNumber { get; set; } = string.Empty;

    public DateTimeOffset? EffectiveAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public bool Active { get; set; } = true;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ComplianceEvidenceOptionGroup? RequiredEvidenceOptionGroup { get; set; }
}

public static class ComplianceExceptionExemptionTypes
{
    public const string RegulatoryException = "regulatory_exception";
    public const string RegulatoryExemption = "regulatory_exemption";
    public const string Waiver = "waiver";
    public const string Variance = "variance";
    public const string SpecialPermit = "special_permit";
    public const string Approval = "approval";
    public const string AlternateCompliancePath = "alternate_compliance_path";
    public const string ConditionalExclusion = "conditional_exclusion";
    public const string Grandfathering = "grandfathering";
    public const string TemporaryRelief = "temporary_relief";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        RegulatoryException,
        RegulatoryExemption,
        Waiver,
        Variance,
        SpecialPermit,
        Approval,
        AlternateCompliancePath,
        ConditionalExclusion,
        Grandfathering,
        TemporaryRelief
    };
}

public static class ComplianceExceptionExemptionEffectTypes
{
    public const string MakesRequirementNotApplicable = "makes_requirement_not_applicable";
    public const string ChangesExpectedValue = "changes_expected_value";
    public const string ChangesRequiredEvidence = "changes_required_evidence";
    public const string AllowsAlternateEvidence = "allows_alternate_evidence";
    public const string ReducesRequirement = "reduces_requirement";
    public const string ExtendsDeadline = "extends_deadline";
    public const string AuthorizesOtherwiseBlockedAction = "authorizes_otherwise_blocked_action";
    public const string ChangesFrequency = "changes_frequency";
    public const string RequiresAdditionalConditions = "requires_additional_conditions";
    public const string ReferenceOnly = "reference_only";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        MakesRequirementNotApplicable,
        ChangesExpectedValue,
        ChangesRequiredEvidence,
        AllowsAlternateEvidence,
        ReducesRequirement,
        ExtendsDeadline,
        AuthorizesOtherwiseBlockedAction,
        ChangesFrequency,
        RequiresAdditionalConditions,
        ReferenceOnly
    };
}
