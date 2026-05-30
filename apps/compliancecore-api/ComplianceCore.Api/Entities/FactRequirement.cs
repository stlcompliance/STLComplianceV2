using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class FactRequirement : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid FactDefinitionId { get; set; }

    public Guid? RulePackId { get; set; }

    public Guid? CitationId { get; set; }

    public string RequirementKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ApplicabilityKey { get; set; } = string.Empty;

    public string SourceProduct { get; set; } = string.Empty;

    public string SourceEntity { get; set; } = string.Empty;

    public string SourceFieldOrRecordType { get; set; } = string.Empty;

    public string ValueType { get; set; } = FactValueTypes.Boolean;

    public string Operator { get; set; } = FactRequirementOperators.Equal;

    public string ExpectedValue { get; set; } = "true";

    public string EvidenceKind { get; set; } = FactRequirementEvidenceKinds.ProductRecord;

    public string RequiredDocumentType { get; set; } = string.Empty;

    public string RetentionPeriod { get; set; } = string.Empty;

    public string AuditQuestion { get; set; } = string.Empty;

    public string FailureSeverity { get; set; } = FactRequirementFailureSeverities.Major;

    public bool AutomaticFailureFlag { get; set; }

    public bool OverrideAllowed { get; set; } = true;

    public string OverridePermission { get; set; } = string.Empty;

    public bool RemediationRequired { get; set; } = true;

    public bool ExternallyAssertable { get; set; }

    public bool IsRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public FactDefinition? FactDefinition { get; set; }

    public RulePack? RulePack { get; set; }

    public RegulatoryCitation? Citation { get; set; }
}
