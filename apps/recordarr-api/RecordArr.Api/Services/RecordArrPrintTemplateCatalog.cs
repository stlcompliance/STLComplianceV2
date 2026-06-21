using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Print;

namespace RecordArr.Api.Services;

public sealed class RecordArrPrintTemplateCatalog(ProductDescriptor product) : IPrintTemplateCatalog
{
    private readonly StlDefaultPrintTemplateCatalog _defaultCatalog = new(product);

    public IReadOnlyList<StlPrintTemplateDescriptor> ListTemplates() =>
    [
        .. _defaultCatalog.ListTemplates(),
        BuildTemplate(
            "recordarr.document.original",
            "Record original PDF",
            "Official server-generated PDF for the current record.",
            StlPrintFormats.Pdf,
            StlPrintDocumentStatuses.Official,
            requiresArchive: true,
            requiresOfficialIssue: true,
            requiresReprintReason: true),
        BuildTemplate(
            "recordarr.document.copy",
            "Record copy PDF",
            "Server-generated copy PDF for internal distribution.",
            StlPrintFormats.Pdf,
            StlPrintDocumentStatuses.Copy,
            requiresArchive: false,
            requiresOfficialIssue: true,
            requiresReprintReason: true),
        BuildTemplate(
            "recordarr.document.redacted_copy",
            "Redacted copy PDF",
            "Redacted server-generated PDF for approved distribution.",
            StlPrintFormats.Pdf,
            StlPrintDocumentStatuses.Redacted,
            requiresArchive: false,
            requiresOfficialIssue: true,
            requiresReprintReason: true),
        BuildTemplate(
            "recordarr.document.cover_sheet",
            "Record cover sheet",
            "Browser-printable record cover sheet preview.",
            StlPrintFormats.HtmlPrint,
            StlPrintDocumentStatuses.WorkingCopy,
            requiresArchive: false,
            requiresOfficialIssue: false,
            requiresReprintReason: false),
        BuildTemplate(
            "recordarr.document.index",
            "Record document index",
            "Printable index of record files, links, and governance references.",
            StlPrintFormats.Pdf,
            StlPrintDocumentStatuses.Copy,
            requiresArchive: false,
            requiresOfficialIssue: false,
            requiresReprintReason: false),
        BuildTemplate(
            "recordarr.record.packet",
            "Record packet",
            "Packet view that combines record overview, evidence, and custody history.",
            StlPrintFormats.PacketPdf,
            StlPrintDocumentStatuses.Copy,
            requiresArchive: false,
            requiresOfficialIssue: false,
            requiresReprintReason: false),
        BuildTemplate(
            "recordarr.retention.audit_packet",
            "Retention audit packet",
            "Printable packet for retention, legal hold, and audit review.",
            StlPrintFormats.PacketPdf,
            StlPrintDocumentStatuses.Official,
            requiresArchive: false,
            requiresOfficialIssue: true,
            requiresReprintReason: true),
        BuildTemplate(
            "recordarr.evidence.binder",
            "Evidence binder",
            "Printable evidence binder for related files, packages, and redactions.",
            StlPrintFormats.PacketPdf,
            StlPrintDocumentStatuses.Copy,
            requiresArchive: false,
            requiresOfficialIssue: false,
            requiresReprintReason: false),
        BuildTemplate(
            "recordarr.document.history",
            "Document history",
            "Printable document history summary.",
            StlPrintFormats.Pdf,
            StlPrintDocumentStatuses.Copy,
            requiresArchive: false,
            requiresOfficialIssue: false,
            requiresReprintReason: false),
        BuildTemplate(
            "recordarr.document.chain_of_custody",
            "Chain of custody",
            "Printable chain-of-custody summary for the record.",
            StlPrintFormats.Pdf,
            StlPrintDocumentStatuses.Official,
            requiresArchive: false,
            requiresOfficialIssue: true,
            requiresReprintReason: true),
    ];

    public StlPrintTemplateDescriptor? GetTemplate(string templateKey) =>
        ListTemplates().FirstOrDefault(template =>
            string.Equals(template.TemplateKey, templateKey, StringComparison.OrdinalIgnoreCase));

    private static StlPrintTemplateDescriptor BuildTemplate(
        string templateKey,
        string name,
        string description,
        string format,
        string documentStatus,
        bool requiresArchive,
        bool requiresOfficialIssue,
        bool requiresReprintReason)
    {
        var fileStem = templateKey.Replace('.', '-');
        return new StlPrintTemplateDescriptor(
            TemplateKey: templateKey,
            ProductKey: "recordarr",
            Name: name,
            Description: description,
            Version: "1",
            DataContractVersion: "1",
            Format: format,
            PaperSize: "letter",
            Orientation: "portrait",
            DocumentStatus: documentStatus,
            IsSystemTemplate: true,
            TenantOverrideAllowed: false,
            RequiresArchive: requiresArchive,
            RequiresOfficialIssue: requiresOfficialIssue,
            RequiresReprintReason: requiresReprintReason,
            RetentionClass: requiresOfficialIssue ? "official_output" : "working_output",
            DefaultFileNamePattern: $"{fileStem}-{{sourceDisplayRef}}");
    }
}
