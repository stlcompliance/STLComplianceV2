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
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreScheduledEvaluationWorkerTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _sharedWorkerToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreScheduledEval-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrScheduledEval-{Guid.NewGuid():N}";

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
        _sharedWorkerToken = await IssueServiceTokenAsync(
            adminToken,
            sourceProduct: "shared-worker",
            allowedProducts: ["compliancecore"],
            ScheduledRuleEvaluationService.ProcessScheduledActionScope);

        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedPublishedScheduledRulePackAsync(complianceAdminToken);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _complianceCoreClient.PostAsJsonAsync(
            "/api/internal/scheduled-evaluations/process-batch",
            new ProcessScheduledRuleEvaluationsRequest(PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_trainarr_source_token()
    {
        var adminToken = await LoginNexArrAdminAsync();
        var trainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["compliancecore"],
            ScheduledRuleEvaluationService.ProcessScheduledActionScope);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/scheduled-evaluations/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trainarrToken);
        request.Content = JsonContent.Create(new ProcessScheduledRuleEvaluationsRequest(PlatformSeeder.DemoTenantId));

        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_returns_published_rule_pack_before_processing()
    {
        var request = ServiceAuthorized(
            HttpMethod.Get,
            "/api/internal/scheduled-evaluations/pending?tenantId="
                + PlatformSeeder.DemoTenantId
                + "&intervalHours=24&batchSize=50",
            _sharedWorkerToken);

        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<PendingScheduledRuleEvaluationsResponse>())!;
        Assert.Contains(body.Items, item => item.PackKey == "scheduled_driver_qualification");
    }

    [Fact]
    public async Task Process_batch_evaluates_due_published_rule_pack_and_records_run()
    {
        var request = ServiceAuthorized(
            HttpMethod.Post,
            "/api/internal/scheduled-evaluations/process-batch",
            _sharedWorkerToken);
        request.Content = JsonContent.Create(new ProcessScheduledRuleEvaluationsRequest(
            PlatformSeeder.DemoTenantId,
            IntervalHours: 24,
            BatchSize: 50,
            EmitFindings: true));

        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<ProcessScheduledRuleEvaluationsResponse>())!;
        Assert.Equal(1, body.PacksDueCount);
        Assert.Equal(1, body.EvaluatedCount);
        Assert.Equal(0, body.SkippedCount);
        Assert.Equal(1, body.AllowCount);
        Assert.Empty(body.EvaluationRunIds);

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var pack = await db.RulePacks.SingleAsync(x => x.PackKey == "scheduled_driver_qualification");
        Assert.NotNull(pack.LastScheduledEvaluationAt);
        Assert.Empty(await db.RuleEvaluationRuns.Where(x => x.RulePackId == pack.Id).ToListAsync());

        var run = await db.ScheduledRuleEvaluationRuns.SingleAsync(x => x.Id == body.ScheduledRunId);
        Assert.Equal(ScheduledRuleEvaluationRunStatuses.Completed, run.Status);
        Assert.Equal(1, run.EvaluatedCount);
    }

    [Fact]
    public async Task Process_batch_skips_recently_evaluated_packs_until_interval_elapses()
    {
        var firstRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/internal/scheduled-evaluations/process-batch",
            _sharedWorkerToken);
        firstRequest.Content = JsonContent.Create(new ProcessScheduledRuleEvaluationsRequest(
            PlatformSeeder.DemoTenantId,
            IntervalHours: 24,
            BatchSize: 50));
        (await _complianceCoreClient.SendAsync(firstRequest)).EnsureSuccessStatusCode();

        var secondRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/internal/scheduled-evaluations/process-batch",
            _sharedWorkerToken);
        secondRequest.Content = JsonContent.Create(new ProcessScheduledRuleEvaluationsRequest(
            PlatformSeeder.DemoTenantId,
            IntervalHours: 24,
            BatchSize: 50));
        var secondResponse = await _complianceCoreClient.SendAsync(secondRequest);
        secondResponse.EnsureSuccessStatusCode();
        var secondBody = (await secondResponse.Content.ReadFromJsonAsync<ProcessScheduledRuleEvaluationsResponse>())!;
        Assert.Equal(0, secondBody.PacksDueCount);
        Assert.Equal(0, secondBody.EvaluatedCount);
    }

    private async Task SeedPublishedScheduledRulePackAsync(string adminToken)
    {
        var programId = await CreateSampleProgramAsync(adminToken);
        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "scheduled_driver_qualification",
            "Scheduled Driver Qualification",
            "Worker scheduled evaluation rule pack."));
        var createPackResponse = await _complianceCoreClient.SendAsync(createPackRequest);
        createPackResponse.EnsureSuccessStatusCode();
        var pack = (await createPackResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;

        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, "scheduled_driver_license_valid");
        var createSourceRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "scheduled_license_flag",
            "static_config",
            "Scheduled license flag",
            "Static default for scheduled evaluation.",
            null,
            null,
            """{"booleanValue":true}""",
            0));
        (await _complianceCoreClient.SendAsync(createSourceRequest)).EnsureSuccessStatusCode();

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid driver license",
                    "fact_boolean",
                    "scheduled_driver_license_valid",
                    true),
            ]);

        var updateContentRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateContentRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateContentRequest)).EnsureSuccessStatusCode();

        var reviewRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{pack.RulePackId}/status", adminToken);
        reviewRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Review));
        (await _complianceCoreClient.SendAsync(reviewRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{pack.RulePackId}/status", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Published));
        (await _complianceCoreClient.SendAsync(publishRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot_sched",
            "Department of Transportation",
            "US DOT"));
        var bodyResponse = await _complianceCoreClient.SendAsync(bodyRequest);
        bodyResponse.EnsureSuccessStatusCode();
        var body = (await bodyResponse.Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal_sched",
            "US Federal",
            "United States federal jurisdiction."));
        var jurisdictionResponse = await _complianceCoreClient.SendAsync(jurisdictionRequest);
        jurisdictionResponse.EnsureSuccessStatusCode();
        var jurisdiction = (await jurisdictionResponse.Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "driver_compliance_sched",
            "Driver Compliance",
            "Driver qualification program."));
        var programResponse = await _complianceCoreClient.SendAsync(programRequest);
        programResponse.EnsureSuccessStatusCode();
        var program = (await programResponse.Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
    }

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            factKey.Replace('_', ' '),
            "Test fact for scheduled evaluation.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        return created.FactDefinitionId;
    }

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        return request;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
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
            $"{sourceProduct}-scheduled-eval-{Guid.NewGuid():N}",
            $"{sourceProduct} scheduled evaluation test",
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private static HttpRequestMessage NexArrAuthorized(HttpMethod method, string url, string accessToken)
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
