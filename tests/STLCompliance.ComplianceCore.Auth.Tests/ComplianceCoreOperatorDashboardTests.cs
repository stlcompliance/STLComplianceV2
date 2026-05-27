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

public class ComplianceCoreOperatorDashboardTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreOperatorDash-{Guid.NewGuid():N}";

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
    public async Task Operator_dashboard_returns_zero_counts_for_empty_tenant()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dashboards/operator", adminToken));
        response.EnsureSuccessStatusCode();
        var dashboard = (await response.Content.ReadFromJsonAsync<OperatorDashboardResponse>())!;

        Assert.Equal(0, dashboard.Findings.OpenCount);
        Assert.Equal(0, dashboard.RulePacks.TotalCount);
        Assert.Equal(0, dashboard.Evaluations.TotalCount);
        Assert.Equal(0, dashboard.WorkflowGates.DefinitionCount);
        Assert.Equal(0, dashboard.AuditEvents.TotalCount);
        Assert.Empty(dashboard.RecentEvaluations);
    }

    [Fact]
    public async Task Operator_dashboard_reflects_findings_evaluations_and_gate_checks()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateRulePackWithContentAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(
            new Dictionary<string, bool> { ["driver_license_valid"] = false, ["medical_cert_on_file"] = true },
            EmitFindings: true));
        (await _complianceCoreClient.SendAsync(evaluateRequest)).EnsureSuccessStatusCode();

        await CreateWorkflowGateAsync(adminToken, rulePackId, "dash_gate_block");
        var gateRequest = Authorized(HttpMethod.Post, "/api/workflow-gates/check", adminToken);
        gateRequest.Content = JsonContent.Create(new WorkflowGateCheckRequest(
            "dash_gate_block",
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = false,
                ["medical_cert_on_file"] = true,
            },
            null,
            EmitFindings: false));
        (await _complianceCoreClient.SendAsync(gateRequest)).EnsureSuccessStatusCode();

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dashboards/operator", adminToken));
        response.EnsureSuccessStatusCode();
        var dashboard = (await response.Content.ReadFromJsonAsync<OperatorDashboardResponse>())!;

        Assert.True(dashboard.Findings.OpenCount >= 1);
        Assert.True(dashboard.Findings.OpenBlockSeverityCount >= 1);
        Assert.Equal(1, dashboard.RulePacks.TotalCount);
        Assert.True(dashboard.Evaluations.TotalCount >= 1);
        Assert.True(dashboard.Evaluations.FailCount >= 1);
        Assert.Equal(1, dashboard.WorkflowGates.DefinitionCount);
        Assert.True(dashboard.WorkflowGates.BlockOutcomeCount >= 1);
        Assert.NotEmpty(dashboard.RecentEvaluations);
        Assert.Equal("fail", dashboard.RecentEvaluations[0].OverallResult);
        Assert.True(dashboard.AuditEvents.TotalCount >= 1);
    }

    [Fact]
    public async Task Operator_dashboard_requires_authentication()
    {
        var response = await _complianceCoreClient.GetAsync("/api/dashboards/operator");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Operator_dashboard_requires_compliancecore_entitlement()
    {
        var staffArrToken = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dashboards/operator", staffArrToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Operator_dashboard_member_can_read()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/dashboards/operator", memberToken));
        response.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateRulePackWithContentAsync(string adminToken)
    {
        var programId = await CreateSampleProgramAsync(adminToken);
        var packRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        packRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification",
            "Baseline driver qualification rule pack."));
        var pack = (await (await _complianceCoreClient.SendAsync(packRequest)).Content.ReadFromJsonAsync<RulePackResponse>())!;

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid driver license",
                    "fact_boolean",
                    "driver_license_valid",
                    true),
                new RuleDefinitionDto(
                    "med_cert",
                    "Medical certificate on file",
                    "fact_boolean",
                    "medical_cert_on_file",
                    true),
            ]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();
        return pack.RulePackId;
    }

    private async Task CreateWorkflowGateAsync(string adminToken, Guid rulePackId, string gateKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/workflow-gates", adminToken);
        request.Content = JsonContent.Create(new CreateWorkflowGateDefinitionRequest(
            gateKey,
            gateKey.Replace('_', ' '),
            "Dashboard test gate.",
            rulePackId));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
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
