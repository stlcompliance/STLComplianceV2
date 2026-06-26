using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Print;

namespace STLCompliance.Shared.Tests;

public class StlDefaultPrintTemplateCatalogTests
{
    [Fact]
    public void ListTemplates_returns_product_prefixed_system_templates()
    {
        var catalog = new StlDefaultPrintTemplateCatalog(new ProductDescriptor("staffarr", "StaffArr", 5102));

        var templates = catalog.ListTemplates();

        Assert.NotEmpty(templates);
        Assert.All(templates, template =>
        {
            Assert.StartsWith("staffarr.", template.TemplateKey, StringComparison.OrdinalIgnoreCase);
            Assert.True(template.IsSystemTemplate);
        });
        Assert.Contains(templates, template =>
            template.DocumentStatus == StlPrintDocumentStatuses.Official
            && template.RequiresOfficialIssue);
    }
}

public class StlDefaultPrintPermissionEvaluatorTests
{
    private readonly StlDefaultPrintPermissionEvaluator _evaluator =
        new(new ProductDescriptor("staffarr", "StaffArr", 5102));

    [Fact]
    public void Working_copy_browser_print_allows_permissionless_preview_when_no_permission_claims_exist()
    {
        var principal = PrintTestPrincipalFactory.BuildPrincipal();

        _evaluator.EnsureActionAllowed(
            principal,
            StlPrintActions.BrowserPrint,
            StlPrintDocumentStatuses.WorkingCopy);
    }

    [Fact]
    public void Working_copy_browser_print_allows_users_after_non_product_launch_context()
    {
        var principal = PrintTestPrincipalFactory.BuildPrincipal(includeLaunchContext: false);

        _evaluator.EnsureActionAllowed(
            principal,
            StlPrintActions.BrowserPrint,
            StlPrintDocumentStatuses.WorkingCopy);
    }

    [Fact]
    public void Official_reprint_requires_explicit_permissions()
    {
        var principal = PrintTestPrincipalFactory.BuildPrincipal();

        var exception = Assert.Throws<StlApiException>(() =>
            _evaluator.EnsureActionAllowed(
                principal,
                StlPrintActions.Reprint,
                StlPrintDocumentStatuses.Official));

        Assert.Equal(403, exception.StatusCode);
        Assert.Equal("print.forbidden", exception.Code);
    }

    [Fact]
    public void Explicit_reprint_and_official_permissions_allow_official_reprint()
    {
        var principal = PrintTestPrincipalFactory.BuildPrincipal(
            permissionClaims:
            [
                "staffarr.print.reprint",
                "staffarr.print.official"
            ]);

        _evaluator.EnsureActionAllowed(
            principal,
            StlPrintActions.Reprint,
            StlPrintDocumentStatuses.Official);
    }
}

public class StlPrintLogServiceTests
{
    [Fact]
    public async Task Preview_uses_registered_provider_and_writes_preview_log()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var principal = PrintTestPrincipalFactory.BuildPrincipal();

        var response = await service.PreviewAsync(
            principal,
            new StlPrintDocumentRequest
            {
                SourceEntityType = "person",
                SourceEntityId = "person-1",
                TemplateKey = "staffarr.current_page.working_copy"
            },
            CancellationToken.None);

        Assert.Equal("Person profile", response.DocumentTitle);
        Assert.Equal("Person profile", response.SourceDisplayRef);
        Assert.Contains("Preview generated from provider.", response.Warnings);

        var history = await service.GetHistoryAsync(principal, "person", "person-1", 10, CancellationToken.None);
        Assert.Contains(history.Items, item => item.Action == StlPrintActions.Preview);
    }

    [Fact]
    public async Task Browser_print_log_is_written_and_returned_in_history()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var principal = PrintTestPrincipalFactory.BuildPrincipal();

        var response = await service.LogBrowserPrintAsync(
            principal,
            new StlBrowserPrintLogRequest
            {
                SourceEntityType = "person",
                SourceEntityId = "person-1",
                SourceDisplayRef = "Person profile",
                MetadataJson = """{"routeRef":"/people/person-1"}"""
            },
            CancellationToken.None);

        var history = await service.GetHistoryAsync(
            principal,
            "person",
            "person-1",
            10,
            CancellationToken.None);

        Assert.Equal(StlPrintActions.BrowserPrint, response.Action);
        Assert.Single(history.Items);
        Assert.Equal("Person profile", history.Items[0].SourceDisplayRef);
        Assert.Equal(StlPrintDocumentStatuses.WorkingCopy, history.Items[0].DocumentStatus);
        Assert.Null(history.Items[0].FailureReason);
    }

    [Fact]
    public async Task Official_reprints_require_a_reason()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var principal = PrintTestPrincipalFactory.BuildPrincipal(
            permissionClaims:
            [
                "staffarr.print.reprint",
                "staffarr.print.official"
            ]);

        var exception = await Assert.ThrowsAsync<StlApiException>(() =>
            service.LogReprintAsync(
                principal,
                new StlReprintRequest
                {
                    SourceEntityType = "person",
                    SourceEntityId = "person-1",
                    SourceDisplayRef = "Person profile",
                    DocumentStatus = StlPrintDocumentStatuses.Official
                },
                CancellationToken.None));

        Assert.Equal("print.reprint_reason_required", exception.Code);
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task Pdf_generation_uses_provider_and_stores_content_hash()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var principal = PrintTestPrincipalFactory.BuildPrincipal(
            permissionClaims:
            [
                "staffarr.print.download",
                "staffarr.print.history.view"
            ]);

        var response = await service.GeneratePdfAsync(
            principal,
            new StlPrintDocumentRequest
            {
                SourceEntityType = "person",
                SourceEntityId = "person-1",
                TemplateKey = "staffarr.current_page.working_copy"
            },
            CancellationToken.None);

        Assert.Equal("application/pdf", response.File.ContentType);
        Assert.Equal("person-profile.pdf", response.File.FileName);
        Assert.False(string.IsNullOrWhiteSpace(response.File.ContentHash));

        var history = await service.GetHistoryAsync(principal, "person", "person-1", 10, CancellationToken.None);
        var download = Assert.Single(history.Items, item => item.Action == StlPrintActions.DownloadPdf);
        Assert.Equal(response.File.ContentHash, download.ContentHash);
        Assert.Equal("person-profile.pdf", download.FileName);
    }

    [Fact]
    public async Task Archive_official_records_recordarr_reference()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var principal = PrintTestPrincipalFactory.BuildPrincipal(
            permissionClaims:
            [
                "staffarr.print.archive",
                "staffarr.print.official",
                "staffarr.print.history.view"
            ]);

        var response = await service.ArchiveOfficialAsync(
            principal,
            new StlPrintDocumentRequest
            {
                SourceEntityType = "person",
                SourceEntityId = "person-1",
                TemplateKey = "staffarr.current_page.official",
                DocumentStatus = StlPrintDocumentStatuses.Official
            },
            CancellationToken.None);

        Assert.Equal("recordarr-doc-1", response.RecordArrDocumentId);

        var history = await service.GetHistoryAsync(principal, "person", "person-1", 10, CancellationToken.None);
        var archived = Assert.Single(history.Items, item => item.Action == StlPrintActions.ArchiveOfficial);
        Assert.Equal("recordarr-doc-1", archived.RecordArrDocumentId);
    }

    private static PrintTestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PrintTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PrintTestDbContext(options);
    }

    private static StlPrintLogService CreateService(PrintTestDbContext db)
    {
        var product = new ProductDescriptor("staffarr", "StaffArr", 5102);
        return new StlPrintLogService(
            db,
            product,
            new StlDefaultPrintTemplateCatalog(product),
            new StlDefaultPrintPermissionEvaluator(product),
            new TestAuditWriter(),
            [new TestPrintableProvider()],
            [new TestCompliancePrintAdvisor()]);
    }

    private sealed class PrintTestDbContext(DbContextOptions<PrintTestDbContext> options)
        : PlatformDbContext(options);

    private sealed class TestAuditWriter : IPrintExportAuditWriter
    {
        public Task WriteAsync(StlPrintExportLog logEntry, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class TestPrintableProvider : IPrintableProvider
    {
        public bool CanHandle(
            StlPrintProviderContext context,
            StlPrintDocumentRequest request,
            StlPrintTemplateDescriptor template) =>
            string.Equals(context.Product.ProductKey, "staffarr", StringComparison.OrdinalIgnoreCase)
            && string.Equals(request.SourceEntityType, "person", StringComparison.OrdinalIgnoreCase);

        public Task<StlPrintPreviewResult> BuildPreviewAsync(
            StlPrintProviderContext context,
            StlPrintDocumentRequest request,
            StlPrintTemplateDescriptor template,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new StlPrintPreviewResult(
                    "Person profile",
                    "Person profile",
                    template.TemplateKey,
                    template.Version,
                    "<article>Preview</article>",
                    null,
                    ["Preview generated from provider."],
                    []));

        public Task<StlGeneratedPrintFile> GeneratePdfAsync(
            StlPrintProviderContext context,
            StlPrintDocumentRequest request,
            StlPrintTemplateDescriptor template,
            string action,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new StlGeneratedPrintFile(
                    "Person profile",
                    "Person profile",
                    template.TemplateKey,
                    template.Version,
                    "person-profile.pdf",
                    "application/pdf",
                    "%PDF-1.4 provider".Select(value => (byte)value).ToArray(),
                    ["PDF generated from provider."],
                    [],
                    null));

        public Task<StlPrintArchiveResult> ArchiveOfficialAsync(
            StlPrintProviderContext context,
            StlPrintDocumentRequest request,
            StlPrintTemplateDescriptor template,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new StlPrintArchiveResult(
                    "Person profile",
                    "Person profile",
                    template.TemplateKey,
                    template.Version,
                    "recordarr-doc-1",
                    "person-profile.pdf",
                    "hash-1",
                    ["Archived from provider."],
                    []));
    }

    private sealed class TestCompliancePrintAdvisor : ICompliancePrintAdvisor
    {
        public Task<StlCompliancePrintAdvice?> GetAdviceAsync(
            StlPrintProviderContext context,
            StlPrintDocumentRequest request,
            StlPrintTemplateDescriptor template,
            CancellationToken cancellationToken) =>
            Task.FromResult<StlCompliancePrintAdvice?>(
                new StlCompliancePrintAdvice(
                    ["Shared compliance warning."],
                    []));
    }
}

internal static class PrintTestPrincipalFactory
{
    public static ClaimsPrincipal BuildPrincipal(
        IEnumerable<string>? permissionClaims = null,
        bool includeLaunchContext = true)
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var tenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var personId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(StlClaimTypes.TenantId, tenantId.ToString()),
            new(StlClaimTypes.PersonId, personId.ToString()),
            new(StlClaimTypes.TenantRoleKey, "tenant_member")
        };

        if (includeLaunchContext)
        {
            claims.Add(new Claim(StlClaimTypes.LaunchableProductKeys, "staffarr"));
        }

        if (permissionClaims is not null)
        {
            claims.Add(new Claim("permissions", string.Join(',', permissionClaims)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
    }
}

