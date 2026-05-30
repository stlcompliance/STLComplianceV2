using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class AuditTrace : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string AuditTraceId { get; set; } = string.Empty;

    public string PackKey { get; set; } = string.Empty;

    public string FactKey { get; set; } = string.Empty;

    public string CitationKey { get; set; } = string.Empty;

    public string SubjectKind { get; set; } = string.Empty;

    public string SubjectId { get; set; } = string.Empty;

    public string EvaluatedValue { get; set; } = string.Empty;

    public string ExpectedValue { get; set; } = string.Empty;

    public string Operator { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public string FailureSeverity { get; set; } = string.Empty;

    public bool AutomaticFailureFlag { get; set; }

    public bool OverrideUsed { get; set; }

    public Guid? OverridePersonId { get; set; }

    public string OverrideReason { get; set; } = string.Empty;

    public bool RemediationRequired { get; set; }

    public string ClaimedExceptionExemptionKey { get; set; } = string.Empty;

    public string ClaimedExceptionExemptionType { get; set; } = string.Empty;

    public string ExceptionExemptionLegalBasis { get; set; } = string.Empty;

    public string ExceptionExemptionProofKey { get; set; } = string.Empty;

    public string ExceptionExemptionScopeResult { get; set; } = string.Empty;

    public string ExceptionExemptionEffectiveResult { get; set; } = string.Empty;

    public string ResultBeforeException { get; set; } = string.Empty;

    public string ResultAfterException { get; set; } = string.Empty;

    public string FinalComplianceResult { get; set; } = string.Empty;

    public bool ExceptionExemptionApplied { get; set; }

    public bool ExceptionExemptionProofRequired { get; set; }

    public bool ExceptionExemptionProofValid { get; set; }

    public DateTimeOffset EvaluatedAt { get; set; }
}
