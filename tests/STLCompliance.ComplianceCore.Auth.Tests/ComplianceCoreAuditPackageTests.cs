using System.IO.Compression;
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

public class ComplianceCoreAuditPackageTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreAuditPackage-{Guid.NewGuid():N}";

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

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
    }

    [Fact]
    public async Task Audit_package_manifest_lists_sections()
    {
        var reviewerToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_reviewer");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/manifest", reviewerToken));
        response.EnsureSuccessStatusCode();
        var manifest = (await response.Content.ReadFromJsonAsync<AuditPackageManifestResponse>())!;
        Assert.Equal("1", manifest.PackageVersion);
        Assert.Equal(6, manifest.Sections.Count);
        Assert.Contains(manifest.Sections, section => section.Key == "audit_events");
        Assert.Contains(manifest.Sections, section => section.Key == "workflow_gate_checks");
        Assert.Contains(manifest.Sections, section => section.Key == "waivers");
        Assert.Contains(manifest.Sections, section => section.FileName == "rule_packs.json");
    }

    [Fact]
    public async Task Audit_package_export_zip_contains_json_files()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedEvaluationDataAsync(adminToken);

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", adminToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", response.Content.Headers.ContentType?.MediaType);

        var zipBytes = await response.Content.ReadAsByteArrayAsync();
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        Assert.Equal(7, archive.Entries.Count);
        Assert.Contains(archive.Entries, entry => entry.Name == "manifest.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "findings.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "evaluation_runs.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "workflow_gate_checks.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "waivers.json");
        Assert.Contains(archive.Entries, entry => entry.Name == "rule_packs.json");
    }

    [Fact]
    public async Task Audit_package_export_json_returns_structured_package()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedEvaluationDataAsync(adminToken);

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", adminToken));
        response.EnsureSuccessStatusCode();
        var package = (await response.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.NotEqual(Guid.Empty, package.PackageId);
        Assert.Equal(PlatformSeeder.DemoTenantId, package.TenantId);
        Assert.True(package.Counts.RulePacks >= 1);
        Assert.True(package.Counts.EvaluationRuns >= 1);
        Assert.NotEmpty(package.RulePacks);
    }

    [Fact]
    public async Task Audit_package_export_writes_audit_event()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var beforeCount = await CountAuditPackageExportEventsAsync();

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", adminToken));
        response.EnsureSuccessStatusCode();

        var afterCount = await CountAuditPackageExportEventsAsync();
        Assert.Equal(beforeCount + 1, afterCount);
    }

    [Fact]
    public async Task Audit_package_export_denies_tenant_member()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Audit_package_export_rejects_invalid_date_range()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var from = DateTimeOffset.UtcNow;
        var to = from.AddDays(-1);
        var response = await _complianceCoreClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit-packages/export?format=json&from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                adminToken));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Audit_package_export_date_filter_limits_audit_events()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedAuditEventsWithDatesAsync();

        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 23, 59, 59, TimeSpan.Zero);
        var response = await _complianceCoreClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/audit-packages/export?format=json&from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}",
                adminToken));
        response.EnsureSuccessStatusCode();
        var package = (await response.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.Equal(1, package.Counts.AuditEvents);
        Assert.Equal("rule_pack.evaluate", package.AuditEvents[0].Action);
    }

    private async Task SeedEvaluationDataAsync(string adminToken)
    {
        var programId = await CreateSampleProgramAsync(adminToken);

        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var createPackResponse = await _complianceCoreClient.SendAsync(createPackRequest);
        createPackResponse.EnsureSuccessStatusCode();
        var pack = (await createPackResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;

        var licenseFactId = await CreateBooleanFactDefinitionAsync(adminToken, "driver_license_valid");
        await CreateStaticFactSourceAsync(adminToken, licenseFactId, "default_license_flag");

        var content = new RulePackContentBody(
            1,
            "all",
            [new RuleDefinitionDto("license_valid", "Valid driver license", "fact_boolean", "driver_license_valid", true)]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{pack.RulePackId}/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(
            new Dictionary<string, bool> { ["driver_license_valid"] = false },
            EmitFindings: true));
        (await _complianceCoreClient.SendAsync(evaluateRequest)).EnsureSuccessStatusCode();
    }

    private async Task SeedAuditEventsWithDatesAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        db.AuditEvents.AddRange(
            CreateAuditEvent("vocabulary.term_create", new DateTimeOffset(2026, 1, 5, 12, 0, 0, TimeSpan.Zero)),
            CreateAuditEvent("rule_pack.evaluate", new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero)),
            CreateAuditEvent("finding.create", new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero)));
        await db.SaveChangesAsync();
    }

    private static ComplianceCoreAuditEvent CreateAuditEvent(string action, DateTimeOffset occurredAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ActorUserId = PlatformSeeder.DemoAdminUserId,
            Action = action,
            TargetType = "test",
            TargetId = "sample",
            Result = "success",
            CorrelationId = Guid.NewGuid(),
            OccurredAt = occurredAt,
        };

    private async Task<int> CountAuditPackageExportEventsAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        return await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "audit_package.export");
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
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
            "Federal jurisdiction."));
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

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            factKey.Replace('_', ' '),
            "Test fact for audit package export.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        return created.FactDefinitionId;
    }

    private async Task CreateStaticFactSourceAsync(string adminToken, Guid factDefinitionId, string sourceKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        request.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            sourceKey,
            "static_config",
            "Static default",
            "Static default for audit package tests.",
            null,
            null,
            """{"booleanValue":true}""",
            0));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
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
