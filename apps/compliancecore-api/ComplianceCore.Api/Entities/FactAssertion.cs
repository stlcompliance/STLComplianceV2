using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class FactAssertion : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string FactKey { get; set; } = string.Empty;

    public string SubjectKind { get; set; } = string.Empty;

    public string SubjectId { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string ValueType { get; set; } = FactValueTypes.String;

    public string SourceProduct { get; set; } = string.Empty;

    public string SourceRecordId { get; set; } = string.Empty;

    public Guid? EvidenceReferenceId { get; set; }

    public string? EvidenceId { get; set; }

    public DateTimeOffset AssertedAt { get; set; }

    public DateTimeOffset? EffectiveAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public EvidenceReference? EvidenceReference { get; set; }
}
