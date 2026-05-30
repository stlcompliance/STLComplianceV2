using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class TheoreticalSituation : IHasTenant
{
    public Guid SituationId { get; set; }

    public Guid TenantId { get; set; }

    public Guid CreatedByPersonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string SituationKind { get; set; } = string.Empty;

    public string Status { get; set; } = TheoreticalSituationStatuses.Draft;

    public string EvaluationMode { get; set; } = TheoreticalEvaluationModes.WhatIf;

    public bool SavedAsTemplate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TheoreticalSituationContext : IHasTenant
{
    public Guid ContextId { get; set; }

    public Guid TenantId { get; set; }

    public Guid SituationId { get; set; }

    public string ContextKey { get; set; } = string.Empty;

    public string ContextLabel { get; set; } = string.Empty;

    public string ContextValueKey { get; set; } = string.Empty;

    public string ContextValueLabel { get; set; } = string.Empty;

    public string ControlledVocabularyType { get; set; } = string.Empty;

    public decimal Confidence { get; set; } = 1m;

    public DateTimeOffset CreatedAt { get; set; }

    public TheoreticalSituation? Situation { get; set; }
}

public sealed class TheoreticalApplicabilityResult : IHasTenant
{
    public Guid ApplicabilityResultId { get; set; }

    public Guid TenantId { get; set; }

    public Guid SituationId { get; set; }

    public string ProgramKey { get; set; } = string.Empty;

    public string PackKey { get; set; } = string.Empty;

    public string CitationKey { get; set; } = string.Empty;

    public decimal ApplicabilityScore { get; set; }

    public string ApplicabilityBand { get; set; } = TheoreticalApplicabilityBands.InsufficientContext;

    public string MatchReasonsJson { get; set; } = "[]";

    public string MissingContextJson { get; set; } = "[]";

    public string ExclusionReasonsJson { get; set; } = "[]";

    public bool EdgeCase { get; set; }

    public string EdgeCaseReason { get; set; } = string.Empty;

    public int UserVisiblePriority { get; set; } = 100;

    public DateTimeOffset CreatedAt { get; set; }

    public TheoreticalSituation? Situation { get; set; }
}

public sealed class TheoreticalSituationFact : IHasTenant
{
    public Guid SituationFactId { get; set; }

    public Guid TenantId { get; set; }

    public Guid SituationId { get; set; }

    public string FactKey { get; set; } = string.Empty;

    public string RequirementKey { get; set; } = string.Empty;

    public string CitationKey { get; set; } = string.Empty;

    public string PackKey { get; set; } = string.Empty;

    public string SimulatedValue { get; set; } = string.Empty;

    public string ValueType { get; set; } = FactValueTypes.Boolean;

    public string SimulatedState { get; set; } = TheoreticalSimulatedStates.Unknown;

    public string EvidenceOptionKey { get; set; } = string.Empty;

    public string EvidenceKind { get; set; } = FactRequirementEvidenceKinds.ProductRecord;

    public string TargetKind { get; set; } = EvidenceOptionTargetKinds.ProductRecord;

    public bool Active { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public TheoreticalSituation? Situation { get; set; }
}

public sealed class TheoreticalSituationIncident : IHasTenant
{
    public Guid SituationIncidentId { get; set; }

    public Guid TenantId { get; set; }

    public Guid SituationId { get; set; }

    public string IncidentTypeKey { get; set; } = string.Empty;

    public string SeverityKey { get; set; } = string.Empty;

    public string InvolvedSubjectKind { get; set; } = string.Empty;

    public string InvolvedSubjectState { get; set; } = string.Empty;

    public string TriggerKey { get; set; } = string.Empty;

    public string TriggerValue { get; set; } = string.Empty;

    public string ReportabilityState { get; set; } = string.Empty;

    public string RemediationState { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public TheoreticalSituation? Situation { get; set; }
}

public sealed class TheoreticalSituationEvaluation : IHasTenant
{
    public Guid EvaluationId { get; set; }

    public Guid TenantId { get; set; }

    public Guid SituationId { get; set; }

    public DateTimeOffset EvaluatedAt { get; set; }

    public Guid EvaluatedByPersonId { get; set; }

    public string Result { get; set; } = TheoreticalEvaluationResults.InsufficientInformation;

    public string Summary { get; set; } = string.Empty;

    public string PrimaryProgramsJson { get; set; } = "[]";

    public string LikelyProgramsJson { get; set; } = "[]";

    public string EdgeCasesJson { get; set; } = "[]";

    public int PassCount { get; set; }

    public int FailCount { get; set; }

    public int WarningCount { get; set; }

    public int BlockedCount { get; set; }

    public int NotApplicableCount { get; set; }

    public int UnknownCount { get; set; }

    public int OverrideAvailableCount { get; set; }

    public int OverrideBlockedCount { get; set; }

    public TheoreticalSituation? Situation { get; set; }
}

public sealed class TheoreticalSituationEvaluationDetail : IHasTenant
{
    public Guid DetailId { get; set; }

    public Guid TenantId { get; set; }

    public Guid EvaluationId { get; set; }

    public string RequirementKey { get; set; } = string.Empty;

    public string FactKey { get; set; } = string.Empty;

    public string CitationKey { get; set; } = string.Empty;

    public string PackKey { get; set; } = string.Empty;

    public string AuditQuestion { get; set; } = string.Empty;

    public string SimulatedState { get; set; } = TheoreticalSimulatedStates.Unknown;

    public string ExpectedValue { get; set; } = string.Empty;

    public string ActualValue { get; set; } = string.Empty;

    public string Operator { get; set; } = string.Empty;

    public string Result { get; set; } = TheoreticalEvaluationResults.InsufficientInformation;

    public string FailureSeverity { get; set; } = string.Empty;

    public bool AutomaticFailureFlag { get; set; }

    public bool OverrideAllowed { get; set; }

    public string OverridePermission { get; set; } = string.Empty;

    public bool RemediationRequired { get; set; }

    public string NormalRuleResult { get; set; } = string.Empty;

    public string ExceptionExemptionKey { get; set; } = string.Empty;

    public string ExceptionExemptionType { get; set; } = string.Empty;

    public string ExceptionExemptionLabel { get; set; } = string.Empty;

    public bool ExceptionExemptionConsidered { get; set; }

    public bool ExceptionExemptionApplies { get; set; }

    public bool ExceptionExemptionProofRequired { get; set; }

    public bool ExceptionExemptionProofValid { get; set; }

    public string ResultBeforeException { get; set; } = string.Empty;

    public string ResultAfterException { get; set; } = string.Empty;

    public string FinalComplianceResult { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string SuggestedNextAction { get; set; } = string.Empty;

    public int VisiblePriority { get; set; } = 100;

    public TheoreticalSituationEvaluation? Evaluation { get; set; }
}

public static class TheoreticalSituationStatuses
{
    public const string Draft = "draft";
    public const string Evaluated = "evaluated";
    public const string Template = "template";
    public const string Archived = "archived";
}

public static class TheoreticalEvaluationModes
{
    public const string WhatIf = "what_if";
}

public static class TheoreticalApplicabilityBands
{
    public const string Primary = "primary";
    public const string Likely = "likely";
    public const string Possible = "possible";
    public const string EdgeCase = "edge_case";
    public const string NotApplicable = "not_applicable";
    public const string InsufficientContext = "insufficient_context";
}

public static class TheoreticalSimulatedStates
{
    public const string Valid = "valid";
    public const string Invalid = "invalid";
    public const string Expired = "expired";
    public const string Incomplete = "incomplete";
    public const string Missing = "missing";
    public const string Unknown = "unknown";
    public const string NotApplicable = "not_applicable";
    public const string AlternateEvidence = "alternate_evidence";
    public const string SystemFact = "system_fact";
    public const string ExternalRegistry = "external_registry";
    public const string Derived = "derived";
    public const string OverrideRequested = "override_requested";
    public const string NoExceptionClaimed = "no_exception_claimed";
    public const string ExceptionUnknown = "exception_unknown";
    public const string KnownExceptionApplies = "known_exception_applies";
    public const string ExemptionValid = "exemption_valid";
    public const string ExemptionExpired = "exemption_expired";
    public const string ExemptionMissingProof = "exemption_missing_proof";
    public const string SpecialPermitValid = "special_permit_valid";
    public const string SpecialPermitOutsideScope = "special_permit_outside_scope";
    public const string AlternateCompliancePathSelected = "alternate_compliance_path_selected";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Valid,
        Invalid,
        Expired,
        Incomplete,
        Missing,
        Unknown,
        NotApplicable,
        AlternateEvidence,
        SystemFact,
        ExternalRegistry,
        Derived,
        OverrideRequested,
        NoExceptionClaimed,
        ExceptionUnknown,
        KnownExceptionApplies,
        ExemptionValid,
        ExemptionExpired,
        ExemptionMissingProof,
        SpecialPermitValid,
        SpecialPermitOutsideScope,
        AlternateCompliancePathSelected
    };

    public static readonly IReadOnlySet<string> Passing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Valid,
        AlternateEvidence,
        SystemFact,
        ExternalRegistry,
        Derived,
        KnownExceptionApplies,
        ExemptionValid,
        SpecialPermitValid,
        AlternateCompliancePathSelected
    };

    public static readonly IReadOnlySet<string> ExceptionStates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        NoExceptionClaimed,
        ExceptionUnknown,
        KnownExceptionApplies,
        ExemptionValid,
        ExemptionExpired,
        ExemptionMissingProof,
        SpecialPermitValid,
        SpecialPermitOutsideScope,
        AlternateCompliancePathSelected
    };
}

public static class TheoreticalEvaluationResults
{
    public const string Compliant = "compliant";
    public const string NotCompliant = "not_compliant";
    public const string Blocked = "blocked";
    public const string AllowedWithWarning = "allowed_with_warning";
    public const string AllowedWithOverride = "allowed_with_override";
    public const string OverrideNotAllowed = "override_not_allowed";
    public const string NotApplicable = "not_applicable";
    public const string InsufficientInformation = "insufficient_information";
    public const string ReferenceOnly = "reference_only";
}
