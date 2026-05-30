using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class EvidenceReference : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EvidenceId { get; set; } = string.Empty;

    public string FactKey { get; set; } = string.Empty;

    public string SourceProduct { get; set; } = string.Empty;

    public string SourceEntity { get; set; } = string.Empty;

    public string SourceRecordId { get; set; } = string.Empty;

    public string SourceField { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string DocumentUrl { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string FileHash { get; set; } = string.Empty;

    public DateTimeOffset CapturedAt { get; set; }

    public DateTimeOffset? EffectiveAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public Guid? CreatedByPersonId { get; set; }

    public Guid? ReviewedByPersonId { get; set; }

    public string ReviewStatus { get; set; } = "pending";

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
