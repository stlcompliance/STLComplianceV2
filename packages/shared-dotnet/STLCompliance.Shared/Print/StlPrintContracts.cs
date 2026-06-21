using System.Security.Claims;
using STLCompliance.Shared.Hosting;

namespace STLCompliance.Shared.Print;

public static class StlPrintActions
{
    public const string Preview = "preview";
    public const string BrowserPrint = "browser_print";
    public const string DownloadPdf = "download_pdf";
    public const string DownloadLabelPdf = "download_label_pdf";
    public const string DownloadPacket = "download_packet";
    public const string ArchiveOfficial = "archive_official";
    public const string Send = "send";
    public const string Reprint = "reprint";

    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        Preview,
        BrowserPrint,
        DownloadPdf,
        DownloadLabelPdf,
        DownloadPacket,
        ArchiveOfficial,
        Send,
        Reprint
    };

    public static string Normalize(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Print action '{parameterName}' is required.", parameterName);
        }

        var trimmed = value.Trim();
        if (!KnownValues.Contains(trimmed))
        {
            throw new ArgumentException($"Unsupported print action '{trimmed}'.", parameterName);
        }

        return KnownValues.First(item => string.Equals(item, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}

public static class StlPrintDocumentStatuses
{
    public const string Draft = "draft";
    public const string WorkingCopy = "working_copy";
    public const string Official = "official";
    public const string Copy = "copy";
    public const string Redacted = "redacted";

    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        WorkingCopy,
        Official,
        Copy,
        Redacted
    };

    public static string Normalize(string? value, string fallback = WorkingCopy)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var trimmed = value.Trim();
        if (!KnownValues.Contains(trimmed))
        {
            throw new ArgumentException($"Unsupported document status '{trimmed}'.", nameof(value));
        }

        return KnownValues.First(item => string.Equals(item, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}

public static class StlPrintFormats
{
    public const string HtmlPrint = "html_print";
    public const string Pdf = "pdf";
    public const string LabelPdf = "label_pdf";
    public const string PacketPdf = "packet_pdf";
    public const string BadgePdf = "badge_pdf";
}

public sealed record StlPrintTemplateDescriptor(
    string TemplateKey,
    string ProductKey,
    string Name,
    string Description,
    string Version,
    string DataContractVersion,
    string Format,
    string PaperSize,
    string Orientation,
    string DocumentStatus,
    bool IsSystemTemplate,
    bool TenantOverrideAllowed,
    bool RequiresArchive,
    bool RequiresOfficialIssue,
    bool RequiresReprintReason,
    string? RetentionClass,
    string? DefaultFileNamePattern);

public sealed record StlPrintTemplateCatalogResponse(IReadOnlyList<StlPrintTemplateDescriptor> Templates);

public class StlPrintDocumentRequest
{
    public string? SourceEntityType { get; init; }

    public string? SourceEntityId { get; init; }

    public string? SourceDisplayRef { get; init; }

    public string? TemplateKey { get; init; }

    public string? TemplateVersion { get; init; }

    public string? DocumentStatus { get; init; }

    public string? OptionsJson { get; init; }

    public string? ReprintReason { get; init; }
}

public sealed class StlBrowserPrintLogRequest : StlPrintDocumentRequest
{
    public string? MetadataJson { get; init; }
}

public sealed class StlReprintRequest : StlPrintDocumentRequest;

public sealed record StlPrintActionResponse(
    Guid LogId,
    string ProductKey,
    string Action,
    string DocumentStatus,
    string TemplateKey,
    string TemplateVersion,
    DateTimeOffset RequestedAtUtc);

public sealed record StlPrintPreviewResponse(
    string DocumentTitle,
    string SourceDisplayRef,
    string TemplateKey,
    string TemplateVersion,
    string? PreviewHtml,
    string? PreviewRoute,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> MissingRequirements,
    Guid LogId);

public sealed record StlPrintArchiveResponse(
    string DocumentTitle,
    string SourceDisplayRef,
    string TemplateKey,
    string TemplateVersion,
    string? RecordArrDocumentId,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> MissingRequirements,
    Guid LogId);

public sealed record StlPrintHistoryItem(
    Guid Id,
    string ProductKey,
    string SourceEntityType,
    string SourceEntityId,
    string SourceDisplayRef,
    string TemplateKey,
    string TemplateVersion,
    string Action,
    string DocumentStatus,
    Guid RequestedByPersonId,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? RecordArrDocumentId,
    string? FileName,
    string? ContentHash,
    string? ReprintReason,
    string? FailureReason,
    string? MetadataJson);

public sealed record StlPrintHistoryResponse(IReadOnlyList<StlPrintHistoryItem> Items);

public sealed record StlPrintProviderContext(
    ProductDescriptor Product,
    ClaimsPrincipal Principal,
    Guid TenantId,
    Guid RequestedByPersonId);

public sealed record StlPrintPreviewResult(
    string DocumentTitle,
    string SourceDisplayRef,
    string TemplateKey,
    string TemplateVersion,
    string? PreviewHtml,
    string? PreviewRoute,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> MissingRequirements);

public sealed record StlGeneratedPrintFile(
    string DocumentTitle,
    string SourceDisplayRef,
    string TemplateKey,
    string TemplateVersion,
    string FileName,
    string ContentType,
    byte[] Content,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> MissingRequirements,
    string? ContentHash);

public sealed record StlPrintFileResponse(Guid LogId, StlGeneratedPrintFile File);

public sealed record StlPrintArchiveResult(
    string DocumentTitle,
    string SourceDisplayRef,
    string TemplateKey,
    string TemplateVersion,
    string? RecordArrDocumentId,
    string? FileName,
    string? ContentHash,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> MissingRequirements);

public sealed record StlRenderablePrintDocument(
    string DocumentTitle,
    string SourceDisplayRef,
    string TemplateKey,
    string TemplateVersion,
    string Html,
    string FileName,
    string ContentType,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> MissingRequirements);

public sealed record StlRecordArchiveRequest(
    Guid TenantId,
    string SourceProductKey,
    string SourceEntityType,
    string SourceEntityId,
    string SourceDisplayRef,
    string DocumentClass,
    string DocumentType,
    string DocumentSubtype,
    string Title,
    string TemplateKey,
    string TemplateVersion,
    DateTimeOffset IssuedAtUtc,
    Guid IssuedByPersonId,
    string? RetentionClass,
    string ContentHash,
    string FileName,
    byte[] Content);

public sealed record StlRecordArchiveReceipt(
    string RecordArrDocumentId,
    string? FileName,
    string? ContentHash);

public sealed record StlCompliancePrintAdvice(
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> MissingRequirements);

public interface IPrintableProvider
{
    bool CanHandle(StlPrintProviderContext context, StlPrintDocumentRequest request, StlPrintTemplateDescriptor template);

    Task<StlPrintPreviewResult> BuildPreviewAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken);

    Task<StlGeneratedPrintFile> GeneratePdfAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        string action,
        CancellationToken cancellationToken);

    Task<StlPrintArchiveResult> ArchiveOfficialAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken);
}

public interface IPrintRenderer
{
    Task<StlRenderablePrintDocument> RenderAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken);
}

public interface IPdfRenderer
{
    Task<StlGeneratedPrintFile> RenderPdfAsync(
        StlRenderablePrintDocument document,
        CancellationToken cancellationToken);
}

public interface IRecordArchiveClient
{
    Task<StlRecordArchiveReceipt> ArchiveAsync(
        StlRecordArchiveRequest request,
        CancellationToken cancellationToken);
}

public interface ICompliancePrintAdvisor
{
    Task<StlCompliancePrintAdvice?> GetAdviceAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken);
}

public interface IPrintTemplateCatalog
{
    IReadOnlyList<StlPrintTemplateDescriptor> ListTemplates();

    StlPrintTemplateDescriptor? GetTemplate(string templateKey);
}

public interface IPrintExportAuditWriter
{
    Task WriteAsync(StlPrintExportLog logEntry, CancellationToken cancellationToken);
}

public interface IPrintPermissionEvaluator
{
    void EnsureTemplateCatalogRead(ClaimsPrincipal principal);

    void EnsureHistoryRead(ClaimsPrincipal principal);

    void EnsureActionAllowed(ClaimsPrincipal principal, string action, string documentStatus);
}
