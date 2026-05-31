using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private string _adminToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreReports-{Guid.NewGuid():N}";

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _complianceCoreClient = _complianceCoreFactory.CreateClient();
        _adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();

        await SeedFindingViaEvaluationAsync();
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
    }

    [Fact]
    public async Task Findings_report_summary_returns_aggregates()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/findings/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<FindingsReportSummaryResponse>())!;
        Assert.True(summary.TotalFindings >= 1);
        Assert.True(summary.OpenCount >= 1);
    }

    [Fact]
    public async Task Operator_report_summary_returns_aggregates()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/operator/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<OperatorReportSummaryResponse>())!;
        Assert.True(summary.EvaluationTotalCount >= 1);
        Assert.True(summary.EvaluationFailCount >= 1);
    }

    [Fact]
    public async Task Operator_report_alerts_include_missing_evidence_and_expiring_waivers()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/operator/alerts", _adminToken));
        response.EnsureSuccessStatusCode();

        var alerts = await response.Content.ReadFromJsonAsync<IReadOnlyList<OperatorReportAlertResponse>>();
        Assert.NotNull(alerts);
        Assert.Contains(alerts!, x => x.AlertType == "evidence_missing");
        Assert.Contains(alerts, x => x.AlertType == "waiver_expiring");
    }

    [Fact]
    public async Task Missing_evidence_report_summary_returns_aggregates()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/evidence/missing/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<MissingEvidenceReportSummaryResponse>())!;
        Assert.True(summary.TotalWarnings >= 1);
        Assert.True(summary.HighCount >= 1);
        Assert.True(summary.MissingMirrorCount >= 1);
    }

    [Fact]
    public async Task V1_reports_index_summaries_and_export_are_available()
    {
        var indexResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports", _adminToken));
        indexResponse.EnsureSuccessStatusCode();
        var indexJson = await indexResponse.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/reports/findings", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/operator", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/evidence/missing", indexJson, StringComparison.Ordinal);

        var findingsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/findings/summary?openOnly=true", _adminToken));
        findingsResponse.EnsureSuccessStatusCode();
        var findings = (await findingsResponse.Content.ReadFromJsonAsync<FindingsReportSummaryResponse>())!;
        Assert.True(findings.OpenCount >= 1);

        var operatorResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/operator/summary?attentionOnly=true", _adminToken));
        operatorResponse.EnsureSuccessStatusCode();
        var operatorSummary = (await operatorResponse.Content.ReadFromJsonAsync<OperatorReportSummaryResponse>())!;
        Assert.True(operatorSummary.EvaluationFailCount >= 1);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/findings/summary/export?openOnly=true", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);

        var missingEvidenceResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/evidence/missing/summary?severity=high", _adminToken));
        missingEvidenceResponse.EnsureSuccessStatusCode();
        var missingEvidence = (await missingEvidenceResponse.Content.ReadFromJsonAsync<MissingEvidenceReportSummaryResponse>())!;
        Assert.True(missingEvidence.TotalWarnings >= 1);

        var missingEvidenceExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/evidence/missing/summary/export?severity=high", _adminToken));
        missingEvidenceExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", missingEvidenceExportResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Entity_export_manifest_lists_current_entities()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", _adminToken));
        response.EnsureSuccessStatusCode();

        var manifest = (await response.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Equal(4, manifest.Entities.Count);
        Assert.Contains(manifest.Entities, entity => entity.EntityKey == "findings");
        Assert.Contains(manifest.Entities, entity => entity.EntityKey == "rule_packs");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "missing_evidence"
            && report.ExportPath == "/api/reports/evidence/missing/summary/export");
    }

    [Fact]
    public async Task Tenant_member_can_read_findings_report_but_cannot_export_manifest()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");

        var readResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/findings/summary", memberToken));
        readResponse.EnsureSuccessStatusCode();

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, exportResponse.StatusCode);
    }

    private async Task SeedFindingViaEvaluationAsync()
    {
        var programId = await CreateRegulatoryProgramAsync(_adminToken);
        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", _adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var pack = (await (await _complianceCoreClient.SendAsync(createPackRequest)).Content.ReadFromJsonAsync<RulePackResponse>())!;

        var contentRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", _adminToken);
        contentRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid driver license",
                    "fact_boolean",
                    "driver_license_valid",
                    true),
            ])));
        (await _complianceCoreClient.SendAsync(contentRequest)).EnsureSuccessStatusCode();

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{pack.RulePackId}/evaluate", _adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(
            new Dictionary<string, bool> { ["driver_license_valid"] = false },
            EmitFindings: true));
        (await _complianceCoreClient.SendAsync(evaluateRequest)).EnsureSuccessStatusCode();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;

        var warningRunId = Guid.NewGuid();
        db.MissingEvidenceWarningRuns.Add(new MissingEvidenceWarningRun
        {
            Id = warningRunId,
            TenantId = PlatformSeeder.DemoTenantId,
            ScopeKey = "tenant",
            PacksAnalyzedCount = 1,
            WarningsEmittedCount = 1,
            HighestSeverity = MissingEvidenceWarningSeverities.High,
            EvaluatedAt = now
        });

        db.MissingEvidenceWarnings.Add(new MissingEvidenceWarning
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            RunId = warningRunId,
            ScopeKey = "tenant",
            RulePackId = pack.RulePackId,
            PackKey = pack.PackKey,
            FactKey = "driver_license_valid",
            WarningType = MissingEvidenceWarningTypes.RulePackFact,
            Severity = MissingEvidenceWarningSeverities.High,
            ReasonCode = MissingEvidenceReasonCodes.MissingMirror,
            HasMirrorAtScope = false,
            IsRequiredInRule = true,
            IsRequiredInCatalog = true,
            Summary = "Missing evidence for driver license validation fact.",
            EvaluatedAt = now
        });

        db.ComplianceWaivers.Add(new ComplianceWaiver
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            WaiverKey = "expiring-waiver-seed",
            RulePackId = pack.RulePackId,
            PackKey = pack.PackKey,
            SubjectScopeKey = "tenant",
            ReasonCode = "operations_override",
            Explanation = "Temporary override while vendor evidence is in transit.",
            Status = WaiverStatuses.Approved,
            EffectiveAt = now.AddDays(-1),
            ExpiresAt = now.AddDays(3),
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            ApprovedByUserId = PlatformSeeder.DemoAdminUserId,
            ApprovedAt = now.AddDays(-1),
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now
        });

        await db.SaveChangesAsync();
    }

    private async Task<Guid> CreateRegulatoryProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "U.S. Department of Transportation",
            "Federal transportation safety and compliance authority."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal",
            "United States Federal",
            "Federal jurisdiction for interstate transportation rules."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "fmcsa_safety",
            "FMCSA Safety Compliance",
            "Federal motor carrier safety compliance program."));
        var program = (await (await _complianceCoreClient.SendAsync(programRequest)).Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
    }

    private string CreateComplianceCoreAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
