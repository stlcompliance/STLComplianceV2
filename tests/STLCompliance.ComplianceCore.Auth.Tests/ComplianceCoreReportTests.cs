using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
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
    public async Task Entity_export_manifest_lists_three_entities()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", _adminToken));
        response.EnsureSuccessStatusCode();

        var manifest = (await response.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Equal(3, manifest.Entities.Count);
        Assert.Contains(manifest.Entities, entity => entity.EntityKey == "findings");
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
