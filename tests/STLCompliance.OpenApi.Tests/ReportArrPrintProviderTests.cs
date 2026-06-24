using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using ReportArr.Api.Data;
using ReportArr.Api.Endpoints;
using ReportArr.Api.Models;
using ReportArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Print;

namespace STLCompliance.OpenApi.Tests;

public sealed class ReportArrPrintProviderTests
{
    private static readonly ProductDescriptor Product = new("reportarr", "ReportArr", 5185);

    [Fact]
    public void TemplateCatalog_includes_reportarr_templates_and_shared_current_page_entries()
    {
        var catalog = new ReportArrPrintTemplateCatalog(Product);
        var templates = catalog.ListTemplates();

        Assert.Contains(templates, template => template.TemplateKey == "reportarr.current_page.working_copy");
        Assert.Contains(templates, template => template.TemplateKey == "reportarr.report.print");
        Assert.Contains(templates, template => template.TemplateKey == "reportarr.report.pdf_export");
        Assert.Contains(templates, template => template.TemplateKey == "reportarr.dashboard.snapshot");
        Assert.Contains(templates, template => template.TemplateKey == "reportarr.scheduled_report.output");
        Assert.Contains(templates, template => template.TemplateKey == "reportarr.audit.packet");
        Assert.Contains(templates, template => template.TemplateKey == "reportarr.compliance_readiness.packet");
        Assert.Contains(templates, template => template.TemplateKey == "reportarr.management.summary");
    }

    [Fact]
    public async Task GeneratePdfAsync_dashboard_snapshot_omits_internal_dashboard_ids()
    {
        var store = new ReportArrStore();
        var provider = new ReportArrPrintableProvider(
            store,
            new StlPlainTextPdfRenderer(),
            new CapturingArchiveClient());
        var principal = CreatePrincipal(displayName: "Avery Auditor", isPlatformAdmin: true);
        var dashboard = store.CreateDashboard(
            principal,
            new IntegrationEndpoints.CreateDashboardRequest(
                "transport-summary",
                "Transport summary",
                "Transportation dashboard.",
                "transportation",
                "last_30_days",
                principal.GetPersonId().ToString("D")));
        var context = CreateContext(principal);
        var template = new ReportArrPrintTemplateCatalog(Product).GetTemplate("reportarr.dashboard.snapshot");

        Assert.NotNull(template);

        var result = await provider.GeneratePdfAsync(
            context,
            new StlPrintDocumentRequest
            {
                SourceEntityType = "dashboard",
                SourceEntityId = dashboard.DashboardId,
                SourceDisplayRef = dashboard.DashboardNumber,
                TemplateKey = "reportarr.dashboard.snapshot",
                TemplateVersion = "1",
                DocumentStatus = StlPrintDocumentStatuses.WorkingCopy,
                OptionsJson = "{\"tenantDisplayName\":\"North Yard Logistics\"}"
            },
            template!,
            StlPrintActions.DownloadPdf,
            CancellationToken.None);

        var pdfText = System.Text.Encoding.ASCII.GetString(result.Content);

        Assert.Equal("application/pdf", result.ContentType);
        Assert.Contains("%PDF-1.4", pdfText);
        Assert.Contains(dashboard.DashboardNumber, pdfText);
        Assert.DoesNotContain(dashboard.DashboardId, pdfText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeneratePdfAsync_report_pdf_export_omits_internal_run_ids()
    {
        var store = new ReportArrStore();
        var provider = new ReportArrPrintableProvider(
            store,
            new StlPlainTextPdfRenderer(),
            new CapturingArchiveClient());
        var principal = CreatePrincipal(displayName: "Avery Auditor", isPlatformAdmin: true);
        var definition = store.CreateReportDefinition(
            principal,
            new IntegrationEndpoints.CreateReportDefinitionRequest(
                "dispatch-performance",
                "Dispatch performance",
                "Dispatch performance report.",
                "operational",
                "layout:grid:2x2",
                ["pdf"],
                principal.GetPersonId().ToString("D")));
        var run = store.CreateReportRun(
            principal,
            new IntegrationEndpoints.CreateReportRunRequest(
                definition.ReportDefinitionId,
                principal.GetPersonId().ToString("D"),
                "pdf",
                ["period=last_30_days"],
                ["site=all"]));
        var context = CreateContext(principal);
        var template = new ReportArrPrintTemplateCatalog(Product).GetTemplate("reportarr.report.pdf_export");

        Assert.NotNull(template);

        var result = await provider.GeneratePdfAsync(
            context,
            new StlPrintDocumentRequest
            {
                SourceEntityType = "report_run",
                SourceEntityId = run.ReportRunId,
                SourceDisplayRef = run.ReportRunNumber,
                TemplateKey = "reportarr.report.pdf_export",
                TemplateVersion = "1",
                DocumentStatus = StlPrintDocumentStatuses.Copy,
                OptionsJson = "{\"tenantDisplayName\":\"North Yard Logistics\"}"
            },
            template!,
            StlPrintActions.DownloadPdf,
            CancellationToken.None);

        var pdfText = System.Text.Encoding.ASCII.GetString(result.Content);

        Assert.Equal("application/pdf", result.ContentType);
        Assert.Contains("%PDF-1.4", pdfText);
        Assert.Contains(run.ReportRunNumber, pdfText);
        Assert.False(string.IsNullOrWhiteSpace(run.ExportJobId));
        Assert.DoesNotContain(run.ReportRunId, pdfText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(definition.ReportDefinitionId, pdfText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(run.ExportJobId!, pdfText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ArchiveOfficialAsync_audit_packet_uses_recordarr_archive_request()
    {
        var store = new ReportArrStore();
        var archiveClient = new CapturingArchiveClient();
        var provider = new ReportArrPrintableProvider(
            store,
            new StlPlainTextPdfRenderer(),
            archiveClient);
        var principal = CreatePrincipal(displayName: "Avery Auditor", isPlatformAdmin: true);
        var definition = store.CreateReportDefinition(
            principal,
            new IntegrationEndpoints.CreateReportDefinitionRequest(
                "readiness-summary",
                "Readiness summary",
                "Compliance readiness report.",
                "compliance",
                "layout:grid:1x2",
                ["pdf"],
                principal.GetPersonId().ToString("D")));
        store.CreateReportRun(
            principal,
            new IntegrationEndpoints.CreateReportRunRequest(
                definition.ReportDefinitionId,
                principal.GetPersonId().ToString("D"),
                null,
                ["scope=annual"],
                ["department=all"]));
        AddAuditScope(
            store,
            new ReportArrAuditScopeResponse(
                "aud-scope-001",
                "cross_product",
                DateTimeOffset.UtcNow.AddDays(-30),
                DateTimeOffset.UtcNow,
                ["reportarr", "compliancecore"],
                ["obj-001", "obj-002"],
                ["rulepack-001"],
                ["site-001"],
                ["dept-001"],
                IncludeEvidence: true,
                IncludeSourceTrace: true,
                TenantId: principal.GetTenantId().ToString("D")));
        var auditPackage = store.CreateAuditPackage(
            principal,
            new IntegrationEndpoints.CreateAuditPackageRequest(
                "aud-scope-001",
                "Quarterly readiness packet",
                "Cross-product audit readiness packet.",
                principal.GetPersonId().ToString("D")));
        var context = CreateContext(principal);
        var template = new ReportArrPrintTemplateCatalog(Product).GetTemplate("reportarr.audit.packet");

        Assert.NotNull(template);

        var archived = await provider.ArchiveOfficialAsync(
            context,
            new StlPrintDocumentRequest
            {
                SourceEntityType = "audit_package",
                SourceEntityId = auditPackage.AuditReportPackageId,
                SourceDisplayRef = auditPackage.PackageNumber,
                TemplateKey = "reportarr.audit.packet",
                TemplateVersion = "1",
                DocumentStatus = StlPrintDocumentStatuses.Official,
                OptionsJson = "{\"tenantDisplayName\":\"North Yard Logistics\"}"
            },
            template!,
            CancellationToken.None);

        Assert.Equal("recordarr-archive-001", archived.RecordArrDocumentId);
        Assert.NotNull(archiveClient.LastRequest);
        Assert.Equal("reportarr", archiveClient.LastRequest!.SourceProductKey);
        Assert.Equal("audit_package", archiveClient.LastRequest.SourceEntityType);
        Assert.Equal(auditPackage.AuditReportPackageId, archiveClient.LastRequest.SourceEntityId);
        Assert.Equal(auditPackage.PackageNumber, archiveClient.LastRequest.SourceDisplayRef);
        Assert.Equal("reportarr.audit.packet", archiveClient.LastRequest.TemplateKey);
        Assert.Equal("compliance", archiveClient.LastRequest.DocumentClass);
        Assert.Equal("audit_packet", archiveClient.LastRequest.DocumentType);
    }

    private static StlPrintProviderContext CreateContext(ClaimsPrincipal principal) =>
        new(
            Product,
            principal,
            principal.GetTenantId(),
            principal.GetPersonId());

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
            new(StlClaimTypes.TenantRoleKey, "reportarr_admin"),
            new(StlClaimTypes.PersonId, userId.ToString("D")),
            new(StlClaimTypes.Entitlements, "reportarr"),
            new(StlClaimTypes.Entitlements, "compliancecore"),
            new(StlClaimTypes.PlatformAdmin, isPlatformAdmin.ToString().ToLowerInvariant()),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth", ClaimTypes.Name, ClaimTypes.Role));
    }

    private static void AddAuditScope(ReportArrStore store, ReportArrAuditScopeResponse scope)
    {
        GetPrivateList<ReportArrAuditScopeResponse>(store, "_auditScopes").Add(scope);
    }

    private static List<T> GetPrivateList<T>(ReportArrStore store, string fieldName)
    {
        var field = typeof(ReportArrStore).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found.");

        return (List<T>?)field.GetValue(store)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was null.");
    }

    private sealed class CapturingArchiveClient : IRecordArchiveClient
    {
        public StlRecordArchiveRequest? LastRequest { get; private set; }

        public Task<StlRecordArchiveReceipt> ArchiveAsync(
            StlRecordArchiveRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new StlRecordArchiveReceipt("recordarr-archive-001", request.FileName, request.ContentHash));
        }
    }
}
