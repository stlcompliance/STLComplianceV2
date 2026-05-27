namespace ComplianceCore.Api.Entities;

public sealed class VocabularyType
{
    public Guid Id { get; set; }

    public string TypeKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
}
