using STLCompliance.Shared.Data;

namespace STLCompliance.Shared.Print;

public sealed class StlPrintExportLog : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string SourceEntityType { get; set; } = string.Empty;

    public string SourceEntityId { get; set; } = string.Empty;

    public string SourceDisplayRef { get; set; } = string.Empty;

    public string TemplateKey { get; set; } = string.Empty;

    public string TemplateVersion { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string DocumentStatus { get; set; } = string.Empty;

    public Guid RequestedByPersonId { get; set; }

    public DateTimeOffset RequestedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public string? RecordArrDocumentId { get; set; }

    public string? FileName { get; set; }

    public string? ContentHash { get; set; }

    public string? ReprintReason { get; set; }

    public string? FailureReason { get; set; }

    public string? MetadataJson { get; set; }
}
