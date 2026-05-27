using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class VocabularyAlias : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid VocabularyTermId { get; set; }

    public string AliasText { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public VocabularyTerm? VocabularyTerm { get; set; }
}
