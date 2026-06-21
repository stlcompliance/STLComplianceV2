using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RecordArr.Api.Data;
using RecordArr.Api.Options;
using RecordArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Print;

namespace STLCompliance.OpenApi.Tests;

public sealed class RecordArrPrintProviderTests
{
    private static readonly ProductDescriptor Product = new("recordarr", "RecordArr", 5110);

    [Fact]
    public async Task GeneratePdfAsync_creates_recordarr_copy_without_internal_record_ids()
    {
        var store = new RecordArrStore();
        var provider = new RecordArrPrintableProvider(
            store,
            new StlPlainTextPdfRenderer(),
            new NoopArchiveClient());
        var principal = CreatePrincipal(displayName: "Avery Auditor", isPlatformAdmin: true);
        var record = CreatePrintableRecord(store, principal, "Record copy test");
        var context = new StlPrintProviderContext(
            Product,
            principal,
            principal.GetTenantId(),
            principal.GetPersonId());
        var template = new RecordArrPrintTemplateCatalog(Product).GetTemplate("recordarr.document.copy");

        Assert.NotNull(template);

        var result = await provider.GeneratePdfAsync(
            context,
            new StlPrintDocumentRequest
            {
                SourceEntityType = "record",
                SourceEntityId = record.RecordId,
                SourceDisplayRef = record.RecordNumber,
                TemplateKey = "recordarr.document.copy",
                TemplateVersion = "1",
                DocumentStatus = StlPrintDocumentStatuses.Copy,
                OptionsJson = "{\"tenantDisplayName\":\"North Yard Logistics\"}"
            },
            template!,
            StlPrintActions.DownloadPdf,
            CancellationToken.None);

        var pdfText = System.Text.Encoding.ASCII.GetString(result.Content);

        Assert.Equal("application/pdf", result.ContentType);
        Assert.NotEmpty(result.Content);
        Assert.NotNull(result.ContentHash);
        Assert.Contains("%PDF-1.4", pdfText);
        Assert.Contains("Copy", pdfText);
        Assert.DoesNotContain(record.RecordId, pdfText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ArchiveOfficialAsync_creates_generated_pdf_record_in_recordarr()
    {
        var store = new RecordArrStore();
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"recordarr-print-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var archiveClient = new RecordArrRecordArchiveClient(
                store,
                new RecordArrDocumentStorageService(
                    new TestHostEnvironment(tempDirectory),
                    Options.Create(new DocumentStorageOptions
                    {
                        RootPath = "print-archives"
                    })));
            var provider = new RecordArrPrintableProvider(
                store,
                new StlPlainTextPdfRenderer(),
                archiveClient);
            var principal = CreatePrincipal(displayName: "Avery Auditor", isPlatformAdmin: true);
            var record = CreatePrintableRecord(store, principal, "Archive test");
            var context = new StlPrintProviderContext(
                Product,
                principal,
                principal.GetTenantId(),
                principal.GetPersonId());
            var template = new RecordArrPrintTemplateCatalog(Product).GetTemplate("recordarr.document.original");

            Assert.NotNull(template);

            var archived = await provider.ArchiveOfficialAsync(
                context,
                new StlPrintDocumentRequest
                {
                    SourceEntityType = "record",
                    SourceEntityId = record.RecordId,
                    SourceDisplayRef = record.RecordNumber,
                    TemplateKey = "recordarr.document.original",
                    TemplateVersion = "1",
                    DocumentStatus = StlPrintDocumentStatuses.Official,
                    OptionsJson = "{\"tenantDisplayName\":\"North Yard Logistics\"}"
                },
                template!,
                CancellationToken.None);

            Assert.False(string.IsNullOrWhiteSpace(archived.RecordArrDocumentId));
            Assert.NotNull(archived.ContentHash);
            Assert.Equal("recordarr.document.original", archived.TemplateKey);

            var archivedRecord = store.GetRecord(principal, archived.RecordArrDocumentId!);
            Assert.NotNull(archivedRecord);
            Assert.Equal("generated_pdf", archivedRecord!.RecordType);
            Assert.Equal("application/pdf", archivedRecord.CurrentMimeType);
            Assert.Equal("approved", archivedRecord.Status);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public void TemplateCatalog_includes_recordarr_templates_and_shared_current_page_entries()
    {
        var catalog = new RecordArrPrintTemplateCatalog(Product);
        var templates = catalog.ListTemplates();

        Assert.Contains(templates, template => template.TemplateKey == "recordarr.current_page.working_copy");
        Assert.Contains(templates, template => template.TemplateKey == "recordarr.document.original");
        Assert.Contains(templates, template => template.TemplateKey == "recordarr.document.redacted_copy");
        Assert.Contains(templates, template => template.TemplateKey == "recordarr.record.packet");
        Assert.Contains(templates, template => template.TemplateKey == "recordarr.document.chain_of_custody");
    }

    private static ClaimsPrincipal CreatePrincipal(
        string? displayName = null,
        bool isPlatformAdmin = false)
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString("D")),
            new(ClaimTypes.Name, displayName ?? "Authorized User"),
            new(JwtRegisteredClaimNames.Name, displayName ?? "Authorized User"),
            new(StlClaimTypes.TenantId, Guid.NewGuid().ToString("D")),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString("D")),
            new(StlClaimTypes.TenantRoleKey, "evidence-manager"),
            new(StlClaimTypes.PersonId, userId.ToString("D")),
            new(StlClaimTypes.Entitlements, "recordarr"),
            new(StlClaimTypes.PlatformAdmin, isPlatformAdmin.ToString().ToLowerInvariant()),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth", ClaimTypes.Name, ClaimTypes.Role));
    }

    private static RecordArr.Api.Models.RecordArrRecordResponse CreatePrintableRecord(
        RecordArrStore store,
        ClaimsPrincipal principal,
        string title)
    {
        var personId = principal.GetPersonId().ToString("D");
        return store.CreateRecord(
            principal.GetTenantId().ToString("D"),
            title,
            "Printable test record.",
            "document",
            "procedure",
            "operations",
            "standard",
            "internal",
            "routarr",
            "trip",
            "trip-7781",
            "Route trip 7781",
            personId,
            personId,
            "printable-test.pdf",
            "application/pdf");
    }

    private sealed class NoopArchiveClient : IRecordArchiveClient
    {
        public Task<StlRecordArchiveReceipt> ArchiveAsync(
            StlRecordArchiveRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new StlRecordArchiveReceipt("archived-record-ref", request.FileName, request.ContentHash));
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";

        public string ApplicationName { get; set; } = "RecordArr.PrintTests";

        public string ContentRootPath { get; set; } = contentRootPath;

        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(contentRootPath);
    }
}
