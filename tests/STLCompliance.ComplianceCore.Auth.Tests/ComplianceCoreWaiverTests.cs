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
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreWaiverTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _workerExpireToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreWaivers-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrWaivers-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexarrDbName));
            });
        });

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(complianceDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        await SeedNexArrAsync();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();

        var adminToken = await LoginNexArrAdminAsync();
        _workerExpireToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["compliancecore"],
            ComplianceWaiverService.ExpireBatchActionScope);

        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedDriverQualificationRulePackAsync(complianceAdminToken);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Waiver_create_approve_and_list_round_trip()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/waivers", adminToken);
        createRequest.Content = JsonContent.Create(new CreateComplianceWaiverRequest(
            "driver-license-temp-waiver",
            rulePackId,
            "tenant",
            "temporary_ops_override",
            "Temporary waiver while license reinstatement paperwork is processed.",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddDays(7)));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ComplianceWaiverResponse>())!;
        Assert.Equal("pending", created.Status);

        var approveResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/waivers/{created.WaiverId}/approve", adminToken));
        approveResponse.EnsureSuccessStatusCode();
        var approved = (await approveResponse.Content.ReadFromJsonAsync<ComplianceWaiverResponse>())!;
        Assert.Equal("approved", approved.Status);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/waivers?status=approved", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var items = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<ComplianceWaiverResponse>>())!;
        Assert.Contains(items, x => x.WaiverKey == "driver-license-temp-waiver");
    }

    [Fact]
    public async Task Workflow_gate_check_returns_waived_when_approved_waiver_matches_failed_rules()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "driver_waiver_gate");

        var createRequest = Authorized(HttpMethod.Post, "/api/waivers", adminToken);
        createRequest.Content = JsonContent.Create(new CreateComplianceWaiverRequest(
            "driver-waiver-gate-pack",
            rulePackId,
            "tenant",
            "temporary_ops_override",
            "Approved waiver for failing driver qualification gate during controlled rollout.",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            null,
            RuleKey: null,
            GateKey: "driver_waiver_gate"));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ComplianceWaiverResponse>())!;
        (await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/waivers/{created.WaiverId}/approve", adminToken))).EnsureSuccessStatusCode();

        var checkRequest = Authorized(HttpMethod.Post, "/api/workflow-gates/check", adminToken);
        checkRequest.Content = JsonContent.Create(new WorkflowGateCheckRequest(
            "driver_waiver_gate",
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = false,
                ["medical_cert_on_file"] = true,
            },
            null));
        var checkResponse = await _complianceCoreClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var result = (await checkResponse.Content.ReadFromJsonAsync<WorkflowGateCheckResponse>())!;
        Assert.Equal(ComplianceEvaluationOutcomes.Waived, result.Outcome);
        Assert.Equal("compliance_waiver_applied", result.ReasonCode);
        Assert.Equal(created.WaiverId, result.AppliedWaiverId);
        Assert.Equal("driver-waiver-gate-pack", result.AppliedWaiverKey);

        using (var scope = _complianceCoreFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
            var persisted = await db.WorkflowGateCheckResults
                .AsNoTracking()
                .FirstAsync(x => x.Id == result.CheckResultId);
            Assert.Equal(created.WaiverId, persisted.AppliedWaiverId);
            Assert.Equal("driver-waiver-gate-pack", persisted.AppliedWaiverKey);
        }
    }

    [Fact]
    public async Task Workflow_gate_check_waiver_persists_to_audit_exports()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "driver_waiver_export_gate");

        var createRequest = Authorized(HttpMethod.Post, "/api/waivers", adminToken);
        createRequest.Content = JsonContent.Create(new CreateComplianceWaiverRequest(
            "driver-waiver-export-pack",
            rulePackId,
            "tenant",
            "temporary_ops_override",
            "Waiver for export audit trail verification.",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            null,
            RuleKey: null,
            GateKey: "driver_waiver_export_gate"));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ComplianceWaiverResponse>())!;
        (await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/waivers/{created.WaiverId}/approve", adminToken))).EnsureSuccessStatusCode();

        var checkRequest = Authorized(HttpMethod.Post, "/api/workflow-gates/check", adminToken);
        checkRequest.Content = JsonContent.Create(new WorkflowGateCheckRequest(
            "driver_waiver_export_gate",
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = false,
                ["medical_cert_on_file"] = true,
            },
            null));
        (await _complianceCoreClient.SendAsync(checkRequest)).EnsureSuccessStatusCode();

        var gateChecksExport = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/workflow-gate-checks", adminToken));
        gateChecksExport.EnsureSuccessStatusCode();
        var gateChecksCsv = await gateChecksExport.Content.ReadAsStringAsync();
        Assert.Contains("driver-waiver-export-pack", gateChecksCsv);
        Assert.Contains(created.WaiverId.ToString(), gateChecksCsv);

        var auditExport = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/audit-packages/export?format=json", adminToken));
        auditExport.EnsureSuccessStatusCode();
        var package = (await auditExport.Content.ReadFromJsonAsync<AuditPackageExportResponse>())!;
        Assert.True(package.Counts.WorkflowGateChecks >= 1);
        Assert.True(package.Counts.Waivers >= 1);
        Assert.Contains(
            package.WorkflowGateChecks,
            check => check.AppliedWaiverId == created.WaiverId && check.AppliedWaiverKey == "driver-waiver-export-pack");
        Assert.Contains(package.Waivers, waiver => waiver.WaiverKey == "driver-waiver-export-pack");
    }

    [Fact]
    public async Task Workflow_gate_check_stays_blocked_when_non_waivable_rule_fails_despite_waiver()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await UpdateDriverQualificationContentAsync(
            adminToken,
            rulePackId,
            nonWaivableLicenseRule: true);
        await CreateWorkflowGateAsync(adminToken, rulePackId, "driver_non_waivable_gate");

        var createRequest = Authorized(HttpMethod.Post, "/api/waivers", adminToken);
        createRequest.Content = JsonContent.Create(new CreateComplianceWaiverRequest(
            "driver-non-waivable-pack-waiver",
            rulePackId,
            "tenant",
            "temporary_ops_override",
            "Pack waiver must not apply when a non-waivable rule fails.",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            null,
            RuleKey: null,
            GateKey: "driver_non_waivable_gate"));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ComplianceWaiverResponse>())!;
        (await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/waivers/{created.WaiverId}/approve", adminToken))).EnsureSuccessStatusCode();

        var checkRequest = Authorized(HttpMethod.Post, "/api/workflow-gates/check", adminToken);
        checkRequest.Content = JsonContent.Create(new WorkflowGateCheckRequest(
            "driver_non_waivable_gate",
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = false,
                ["medical_cert_on_file"] = true,
            },
            null));
        var checkResponse = await _complianceCoreClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var result = (await checkResponse.Content.ReadFromJsonAsync<WorkflowGateCheckResponse>())!;
        Assert.Equal(ComplianceEvaluationOutcomes.Block, result.Outcome);
        Assert.Null(result.AppliedWaiverId);
        Assert.Contains(result.Reasons, reason =>
            string.Equals(reason.Code, "non_waivable_rule_failed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Waiver_create_rejects_non_waivable_rule_key()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);
        await UpdateDriverQualificationContentAsync(
            adminToken,
            rulePackId,
            nonWaivableLicenseRule: true);

        var createRequest = Authorized(HttpMethod.Post, "/api/waivers", adminToken);
        createRequest.Content = JsonContent.Create(new CreateComplianceWaiverRequest(
            "license-rule-waiver-rejected",
            rulePackId,
            "tenant",
            "temporary_ops_override",
            "Should not be able to target a non-waivable rule directly.",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            null,
            RuleKey: "license_valid",
            GateKey: null));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Conflict, createResponse.StatusCode);
    }

    [Fact]
    public async Task Internal_expire_batch_marks_expired_waivers()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await GetDriverQualificationRulePackIdAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/waivers", adminToken);
        createRequest.Content = JsonContent.Create(new CreateComplianceWaiverRequest(
            "expired-waiver-sample",
            rulePackId,
            "tenant",
            "temporary_ops_override",
            "Waiver that should be expired by the worker batch endpoint.",
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1)));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ComplianceWaiverResponse>())!;
        (await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/waivers/{created.WaiverId}/approve", adminToken))).EnsureSuccessStatusCode();

        var expireRequest = ServiceAuthorized(HttpMethod.Post, "/api/internal/waivers/expire-batch", _workerExpireToken);
        expireRequest.Content = JsonContent.Create(new ProcessExpiredWaiversRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50));
        var expireResponse = await _complianceCoreClient.SendAsync(expireRequest);
        expireResponse.EnsureSuccessStatusCode();
        var batch = (await expireResponse.Content.ReadFromJsonAsync<ProcessExpiredWaiversResponse>())!;
        Assert.True(batch.ExpiredCount >= 1);
        Assert.Contains("expired-waiver-sample", batch.ExpiredWaiverKeys);

        var getResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/waivers/{created.WaiverId}", adminToken));
        getResponse.EnsureSuccessStatusCode();
        var loaded = (await getResponse.Content.ReadFromJsonAsync<ComplianceWaiverResponse>())!;
        Assert.Equal("expired", loaded.Status);
    }

    private async Task<Guid> GetDriverQualificationRulePackIdAsync(string adminToken)
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/rule-packs", adminToken));
        response.EnsureSuccessStatusCode();
        var packs = (await response.Content.ReadFromJsonAsync<IReadOnlyList<RulePackResponse>>())!;
        var pack = packs.First(p => p.PackKey == "driver_qualification");
        return pack.RulePackId;
    }

    private async Task SeedDriverQualificationRulePackAsync(string adminToken)
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

        await CreateBooleanFactDefinitionAsync(adminToken, "driver_license_valid");
        await CreateBooleanFactDefinitionAsync(adminToken, "medical_cert_on_file");

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto("license_valid", "Valid driver license", "fact_boolean", "driver_license_valid", true),
                new RuleDefinitionDto("med_cert", "Medical certificate on file", "fact_boolean", "medical_cert_on_file", true),
            ]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();
    }

    private async Task UpdateDriverQualificationContentAsync(
        string adminToken,
        Guid rulePackId,
        bool nonWaivableLicenseRule)
    {
        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid driver license",
                    "fact_boolean",
                    "driver_license_valid",
                    true,
                    nonWaivableLicenseRule),
                new RuleDefinitionDto("med_cert", "Medical certificate on file", "fact_boolean", "medical_cert_on_file", true),
            ]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{rulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();
    }

    private async Task CreateWorkflowGateAsync(string adminToken, Guid rulePackId, string gateKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/workflow-gates", adminToken);
        request.Content = JsonContent.Create(new CreateWorkflowGateDefinitionRequest(
            gateKey,
            gateKey.Replace('_', ' '),
            "Workflow gate for waiver tests.",
            rulePackId));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            factKey.Replace('_', ' '),
            "Test fact for waiver tests.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        return created.FactDefinitionId;
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private async Task<string> LoginNexArrAdminAsync()
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-waiver-{Guid.NewGuid():N}",
            $"{sourceProduct} waiver test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            allowedProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private static HttpRequestMessage NexArrAuthorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
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

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
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
